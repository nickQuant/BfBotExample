using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CoreLib.Betfair.TO
{
    public class LimitOnCloseOrder
    {
        [JsonProperty(PropertyName = "price")]
        public double Price { get; set; }

        [JsonProperty(PropertyName = "liability")]
        public double Liability { get; set; }

        public override string ToString()
        {
            return new StringBuilder()
                        .AppendFormat("Price={0}", Price)
                        .AppendFormat(" : Liability={0}", Liability)
                        .ToString();
        }
    }
}
