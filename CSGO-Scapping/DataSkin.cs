using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGO_Scapping
{
    public class DataSkin
    {
        public string app_id { get; set; }
        public string context_id { get; set; }
        public List<SalesSkin> sales { get; set; }
    }
}
