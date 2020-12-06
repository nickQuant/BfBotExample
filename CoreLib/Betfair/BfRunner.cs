using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Betfair
{
    public class BfRunner
    {
        public long RunnerID { get; set; }
        public string Name { get; set; }
        public string NameWithoutNumber { get; set; }
        public int RunnerNumber { get; set; }
        public decimal ltp { get; set; }
        public decimal bab { get; set; }
        public decimal bal { get; set; }
    }
}
