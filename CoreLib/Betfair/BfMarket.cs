using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Betfair
{
    public class BfMarket
    {
        public string BetFairMarketId { get; set; }
        public int MarketBaseRate { get; set; }
        public long EventId { get; set; }
        public string MarketType { get; set; }
        public DateTime MarketTime { get; set; }
        public bool Complete { get; set; }
        public bool InPlay { get; set; }
        public int NumberOfActiveRunners { get; set; }
        public string Venue { get; set; }
        public string CountryCode { get; set; }
        public string Timezone { get; set; }
        public string Name { get; set; }
        public string EventName { get; set; }
    }
}
