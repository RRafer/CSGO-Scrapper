using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGO_Scapping
{
    public class Skin
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal promedio { get; set; }
        public decimal promedioVenta { get; set; }
        public decimal dolar { get; set; }
        public int sales { get; set; }
        public string obs { get; set; }
        public List<SalesSkin> SalesSkins { get; set; }
    }
}
