using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGO_Scapping
{
    public class SalesSkin
    {
        public string market_hash_name { get; set; }
        public string price { get; set; }
        public decimal decimalPrice { get; set; }
        public string wear_value { get; set; }
        public Int32 sold_at { get; set; }
    }
}
