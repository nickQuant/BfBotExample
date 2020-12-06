using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betfair.ESAClient.Helpers
{
    public static class DateTimeHelpers
    {
        public static long BetFairPt(this DateTime date)
        {
            double pt = (date - new DateTime(1970, 1, 1)).TotalSeconds * 1000.0;
            return (long)pt;
        }
    }
}
