using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betfair.ESAClient.Cache
{
    /// <summary>
    /// Atomic snap of the prices associated with a runner.
    /// </summary>
    public class MarketRunnerPrices
    {
        public static readonly MarketRunnerPrices EMPTY = new MarketRunnerPrices()
        {
            AvailableToLay = PriceSize.EmptyList,
            AvailableToBack = PriceSize.EmptyList,
            Traded = PriceSize.EmptyList,
            StartingPriceBack = PriceSize.EmptyList,
            StartingPriceLay = PriceSize.EmptyList,

            BestAvailableToBack = LevelPriceSize.EmptyList,
            BestAvailableToLay = LevelPriceSize.EmptyList,
            BestDisplayAvailableToBack = LevelPriceSize.EmptyList,
            BestDisplayAvailableToLay = LevelPriceSize.EmptyList,
        };

        public IList<PriceSize> AvailableToLay { get; set; } 
        public IList<PriceSize> AvailableToBack { get; set; }
        public IList<PriceSize> Traded { get; set; }
        public IList<PriceSize> StartingPriceBack { get; set; }
        public IList<PriceSize> StartingPriceLay { get; set; }

        public IList<LevelPriceSize> BestAvailableToBack { get; set; }
        public IList<LevelPriceSize> BestAvailableToLay { get; set; }
        public IList<LevelPriceSize> BestDisplayAvailableToBack { get; set; }
        public IList<LevelPriceSize> BestDisplayAvailableToLay { get; set; }

        public double LastTradedPrice { get; set; }
        public double StartingPriceNear { get; set; }
        public double StartingPriceFar { get; set; }
        public double TradedVolume { get; set; }

        public override string ToString()
        {
            return "MarketRunnerPrices{" +
                "AvailableToLay=" + String.Join(", ", AvailableToLay) +
                ", AvailableToBack=" + String.Join(", ", AvailableToBack) +
                ", Traded=" + String.Join(", ", Traded) +
                ", StartingPriceBack=" + String.Join(", ", StartingPriceBack) +
                ", StartingPriceLay=" + String.Join(", ", StartingPriceLay) +

                ", BestAvailableToBack=" + String.Join(", ", BestAvailableToBack) +
                ", BestAvailableToLay=" + String.Join(", ", BestAvailableToLay) +
                ", BestDisplayAvailableToBack=" + String.Join(", ", BestDisplayAvailableToBack) +
                ", BestDisplayAvailableToLay=" + String.Join(", ", BestDisplayAvailableToLay) +

                ", LastTradedPrice=" + LastTradedPrice +
                ", StartingPriceNear=" + StartingPriceNear +
                ", StartingPriceFar=" + StartingPriceFar +
                ", TradedVolume=" + TradedVolume +
                "}";
        }
    }
}
