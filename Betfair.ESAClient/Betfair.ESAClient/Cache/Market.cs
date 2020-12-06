using Betfair.ESASwagger.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betfair.ESAClient.Cache
{
    /// <summary>
    /// Thread safe, reference invariant reference to a market.
    /// Repeatedly calling <see cref="Snap"/> will return atomic snapshots of the market.
    /// </summary>
    public class Market
    {
        private readonly Dictionary<RunnerId, MarketRunner> _marketRunners = new Dictionary<RunnerId, MarketRunner>();
        private MarketDefinition _marketDefinition;
        private double _tv;

        internal void OnMarketChange(MarketChange marketChange)
        {
            //initial image means we need to wipe our data
            bool isImage = marketChange.Img == true;

            if (marketChange.MarketDefinition != null)
            {
                //market definition changed
                OnMarketDefinitionChange(marketChange.MarketDefinition);
            }
            if (marketChange.Rc != null)
            {
                //runners changed
                foreach (RunnerChange runnerChange in marketChange.Rc)
                {
                    OnPriceChange(isImage, runnerChange);
                }
            }

            MarketSnap newSnap = new MarketSnap();
            newSnap.MarketId = MarketId;
            newSnap.MarketDefinition = _marketDefinition;
            newSnap.MarketRunners = _marketRunners.Values.Select(runner => runner.Snap).ToList();
            newSnap.TradedVolume = Utils.SelectPrice(isImage, ref _tv, marketChange.Tv);
            Snap = newSnap;
        }

        private MarketRunner GetOrAdd(RunnerId rid)
        {
            MarketRunner runner;
            if (!_marketRunners.TryGetValue(rid, out runner))
            {
                runner = new MarketRunner(this, rid);
                _marketRunners[rid] = runner;
            }
            return runner;
        }

        private void OnPriceChange(bool isImage, RunnerChange runnerChange)
        {
            MarketRunner marketRunner = GetOrAdd(new RunnerId(runnerChange.Id, runnerChange.Hc));
            //update the runner
            marketRunner.OnPriceChange(isImage, runnerChange);
        }

        private void OnMarketDefinitionChange(MarketDefinition marketDefinition)
        {
            _marketDefinition = marketDefinition;
            if (marketDefinition.Runners != null)
            {
                foreach (RunnerDefinition runnerDefinition in marketDefinition.Runners)
                {
                    OnRunnerDefinitionChange(runnerDefinition);
                }
            }
        }

        private void OnRunnerDefinitionChange(RunnerDefinition runnerDefinition)
        {
            MarketRunner marketRunner = GetOrAdd(new RunnerId(runnerDefinition.Id, runnerDefinition.Hc));
            //update runner
            marketRunner.OnRunnerDefinitionChange(runnerDefinition);
        }

        /// <summary>
        /// Market id
        /// </summary>
        public string MarketId { get; set; }

        /// <summary>
        /// Whether the market is closed.
        /// </summary>
        public bool IsClosed
        {
            get
            {
                return _marketDefinition != null && _marketDefinition.Status == MarketDefinition.StatusEnum.Closed;
            }
        }

        /// <summary>
        /// An atomic snapshot of the state of the market.
        /// </summary>
        public MarketSnap Snap { get; set; }

        public override string ToString()
        {
            return "Market{" +
                "MarketId=" + MarketId +
                ", MarketDefinition=" + _marketDefinition +
                ", MarketRunners=" + String.Join(", ", _marketRunners.Values) +
                "}";
        }
    }
}
