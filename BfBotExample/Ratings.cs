using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BfBotExample
{
    public class Runner
    {
        public int bfExchangeSelectionId { get; set; }
        public string name { get; set; }
        public double ratedPrice { get; set; }
        public int number { get; set; }
    }

    public class Race
    {
        public string bfExchangeMarketId { get; set; }
        public string name { get; set; }
        public int number { get; set; }
        public List<Runner> runners { get; set; }
        public object comment { get; set; }
    }

    public class Meeting
    {
        public string name { get; set; }
        public object bfExchangeEventId { get; set; }
        public List<Race> races { get; set; }
    }

    public class Ratings
    {
        public List<Meeting> meetings { get; set; }
    }
}
