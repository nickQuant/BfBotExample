using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betfair.ESAClient.Cache
{
    /// <summary>
    /// Immutable tuple of price, size
    /// </summary>
    public class PriceSize
    {
        private double _price;
        private double _size;
        public static IList<PriceSize> EmptyList = new PriceSize[0];

        public PriceSize(List<double?> priceSize)
        {
            _price = (double)priceSize[0];
            _size = (double)priceSize[1];
        }

        public PriceSize(double price, double size)
        {
            _price = price;
            _size = size;
        }

        public PriceSize() { }

        public double Price
        {
            get
            {
                return _price;
            }
            set { _price = value; }
        }

        public double Size
        {
            get
            {
                return _size;
            }
            set { _size = value; }
        }

        public override string ToString()
        {
            return _size + "@" + _price;
        }
    }
}
