using CoreLib.Betfair;
using CoreLib.Betfair.TO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using Dapper;

namespace BfBotExample
{
    class Program
    {
        //logging
        private static log4net.ILog Log { get; } = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //BF API members
        private static readonly string _url = "https://api.betfair.com/exchange/betting";
        private static JsonRpcClient Client { get; set; }
        private static List<MarketCatalogue> Markets { get; set; } = new List<MarketCatalogue>();

        //BF HUB
        private static Ratings Ratings;

        //placed bets
        private static List<string> MarketsBetOn { get; set; } = new List<string>();

        private static void Main(string[] args)
        {
            InitialSetup();

            while (true)
            {
                try
                {
                    if (KillSwitchOn())
                        break;

                    var nextRaceCatalogue = GetNextRace(out var nextRaceMarketBook);
                    if (nextRaceCatalogue == null)
                    {
                        Log.Info($"No more races to process");
                        System.Threading.Thread.Sleep(15000);
                        continue;
                    }

                    Log.Info($"Next race is {nextRaceCatalogue.Event.Venue} {nextRaceCatalogue.MarketName} at {nextRaceCatalogue.Description.MarketTime.ToLocalTime()}");

                    GenerateTrades(nextRaceCatalogue, nextRaceMarketBook, out var runnersToBack, out var marketPct);
                    if (runnersToBack == null)
                        continue;

                    PlaceTradesIfRequired(nextRaceCatalogue, nextRaceMarketBook, runnersToBack, marketPct);
                    CheckForSleep(nextRaceCatalogue);
                }
                catch (System.Exception ex)
                {
                    Log.Error("Error", ex);
                }
            }
        }

        private static void CheckForSleep(MarketCatalogue mc)
        {

            if (mc.Description.MarketTime > DateTime.UtcNow.AddMinutes(3))
            {
                Log.Info($"Sleeping until 2 mins before next race. {mc.Event.Venue} {mc.MarketName}");
                System.Threading.Thread.Sleep(120000);
            }
            else
            {
                System.Threading.Thread.Sleep(15000); //avoid hitting the API too often
                Log.Info($"Sleeping 15s");
            }
        }

        private static void PlaceTradesIfRequired(MarketCatalogue mc, MarketBook mb, List<CoreLib.Betfair.TO.Runner> runnersToBack, double marketPct)
        {
            if (marketPct > double.Parse(ConfigurationManager.AppSettings["MaxAllowableMarketPct"]))
            {
                Log.Info($"Market percentage for {mc.MarketId} is very high at {marketPct}.  Will not place bets");
                return;
            }

            if (!MarketsBetOn.Contains(mb.MarketId) && mc.Description.MarketTime < DateTime.UtcNow)
            {
                MarketsBetOn.Add(mb.MarketId);
                Log.Info($"Trade Placement for {mc.Event.Name} {mc.MarketName}");
                PlaceBets(mc, mb, runnersToBack);
            }
        }

        static void PlaceBets(MarketCatalogue mc, MarketBook mb, List<CoreLib.Betfair.TO.Runner> runnersToBack)
        {
            var betPlaced = false;
            decimal unitSize = decimal.Parse(ConfigurationManager.AppSettings["UnitSize"]);
            Log.Info($"Unit size for {mc.MarketId}-:{unitSize:c2}");

            foreach (var runner in runnersToBack)
            {
                var betAmount = unitSize;

                List<PlaceInstruction> placeInstructions = new List<PlaceInstruction>();
                var placeInstructionBack = new PlaceInstruction();
                placeInstructionBack.Handicap = 0;
                placeInstructionBack.SelectionId = runner.SelectionId;

                placeInstructionBack.OrderType = OrderType.MARKET_ON_CLOSE;
                placeInstructionBack.MarketOnCloseOrder = new MarketOnCloseOrder();
                placeInstructionBack.MarketOnCloseOrder.Liability = (double)betAmount;
                placeInstructionBack.Side = Side.BACK;
                placeInstructions.Add(placeInstructionBack);

                var customerRef = Guid.NewGuid().ToString().Substring(0, 20);
                var placeExecutionReport = Program.Client.placeOrders(mc.MarketId, customerRef, placeInstructions);
                Log.Info($"Placement report for {mc.MarketId}-{runner.SelectionId} BACK bets\tStatus:{placeExecutionReport.Status}\tError Code:{placeExecutionReport.ErrorCode}");
                betPlaced = true;
            }

            if (betPlaced)
                Console.Beep();
        }

        private static void GenerateTrades(MarketCatalogue mc, MarketBook mb, out List<CoreLib.Betfair.TO.Runner> ret, out double marketPct)
        {
            ret = null;
            marketPct = 0;
            var ratingMeeting = (from m in Ratings.meetings where m.name.ToUpper() == mc.Event.Venue.ToUpper() select m).FirstOrDefault();
            if (ratingMeeting == null)
            {
                Log.Info($"No ratings for {mc.Event.Name} {mc.MarketName}");
                return;
            }

            var ratingsRace = (from r in ratingMeeting.races where r.bfExchangeMarketId == mc.MarketId select r).FirstOrDefault();
            if (ratingsRace == null)
                return;

            ret = new List<CoreLib.Betfair.TO.Runner>();
            var overs = float.Parse(ConfigurationManager.AppSettings["OversNeeded"]);
            foreach (var runner in mb.Runners)
            {
                var catalogueRunner = (from r in mc.Runners where r.SelectionId == runner.SelectionId select r).FirstOrDefault();
                if (catalogueRunner != null) { 
                    var modelRunner = (from runners in ratingsRace.runners where runners.name == catalogueRunner.RunnerName select runners).FirstOrDefault();
                    if (modelRunner != null)
                    {
                        if (runner.LastPriceTraded > modelRunner.ratedPrice * overs)
                        {
                            ret.Add(runner);
                        }
                    }
                }

                if (runner.LastPriceTraded.HasValue)
                    marketPct += (1.0 / (double)runner.LastPriceTraded);
            }
        }

        private static MarketCatalogue GetNextRace(out MarketBook mb)
        {
            var meetingNames = (from m in Ratings.meetings select m.name).ToList();
            mb = null;
            var race = (from m in Markets where m.Description.MarketTime > DateTime.UtcNow.AddSeconds(-30) && meetingNames.Contains(m.Event.Venue.ToUpper())
                        orderby m.Description.MarketTime select m).FirstOrDefault();
            if (race != null)
            {
                var pp = new PriceProjection();
                pp.PriceData = new HashSet<PriceData> { PriceData.EX_ALL_OFFERS, PriceData.EX_TRADED };
                var prices = Client.listMarketBook(new List<string> { race.MarketId }, pp);
                mb = prices.FirstOrDefault();
            }

            return race;
        }

        private static bool KillSwitchOn()
        {
            using (var connection = new SqlConnection(ConfigurationManager.AppSettings["Sql"]))
            {
                var killSwitchOn = connection.Query<int>("exec KillSwitchGet 'BfBotExample'").FirstOrDefault() > 0;
                if (killSwitchOn)
                {
                    Log.Info("Kill switch is on - exiting app");
                }
                return killSwitchOn;
            }
        }

        private static void InitialSetup()
        {
            //betfair connection 
            var provider = new AppKeyAndSessionProvider(ConfigurationManager.AppSettings["ssoHost"], ConfigurationManager.AppSettings["appKey"],
                ConfigurationManager.AppSettings["userName"], ConfigurationManager.AppSettings["password"]);
            var session = provider.GetOrCreateNewSession();
            Client = new JsonRpcClient(_url, session.AppKey, session.Session);

            //Betfair Hub
            var url = string.Format(ConfigurationManager.AppSettings["ratingsFile"], DateTime.Today);
            using (var client = new WebClient())
            {
                var ratings = client.DownloadString(url);
                System.IO.File.WriteAllText($"ratings-{DateTime.Now:yyyyMMddHHmm}.json", ratings);
                Ratings = JsonConvert.DeserializeObject<Ratings>(ratings);
            }

            LoadMarkets();
            var startTimeSpan = TimeSpan.FromMinutes(2);
            var periodTimeSpan = TimeSpan.FromMinutes(2);
            var timer = new System.Threading.Timer((e) =>
            {
                LoadMarkets();
            }, null, startTimeSpan, periodTimeSpan);
        }

        private static void LoadMarkets()
        {
            //get the markets required.  Probably safe to hard-code 7 here for but this lookup keeps the code clearer
            var marketFilter = new MarketFilter();
            var eventTypes = Client.listEventTypes(marketFilter);
            ISet<string> eventypeIds = new HashSet<string>();
            var desiredEvents = new List<string> { "Horse Racing" };
            foreach (EventTypeResult eventType in eventTypes)
            {
                if (desiredEvents.Contains(eventType.EventType.Name))
                {
                    Log.Info($"Found event type for {eventType.EventType.Name}: " + CoreLib.Betfair.Json.JsonConvert.Serialize<EventTypeResult>(eventType));
                    eventypeIds.Add(eventType.EventType.Id);
                }
            }

            //get the relevant markets
            var marketIds = new List<string>();
            foreach (var eventypeId in eventypeIds)
            {
                Log.Info($"Getting markets for eventypeId: {eventypeId}");
                marketFilter = new MarketFilter();
                marketFilter.EventTypeIds = new HashSet<string> { eventypeId };
                marketFilter.MarketStartTime = new TimeRange { From = DateTime.Now.AddDays(-1), To = DateTime.Now.AddDays(1) };
                marketFilter.MarketCountries = new SortedSet<string> { "AU" }; //add countries as required
                var marketSort = MarketSort.FIRST_TO_START;
                marketFilter.MarketTypeCodes = new HashSet<string> { "WIN" }; //add "PLACE" if required for model
                var maxResults = "200";

                ISet<MarketProjection> marketProjections = new HashSet<MarketProjection> { MarketProjection.EVENT, MarketProjection.MARKET_DESCRIPTION,
                MarketProjection.RUNNER_DESCRIPTION, MarketProjection.COMPETITION};

                var marketCatalogues = Client.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults);

                //get rid of trots events - they share the same market type
                lock (Markets)
                {
                    Markets = new List<MarketCatalogue>();
                    foreach (var market in marketCatalogues)
                    {
                        if (market.MarketName.ToLower().IndexOf("pace") == -1 && market.MarketName.ToLower().IndexOf("trot") == -1)
                            Markets.Add(market);
                    }
                }

                Log.Info($"Got {Markets.Count} markets for AU gallops");
            }
        }
    }
}
