using HtmlAgilityPack;
using ScrapySharp.Extensions;
using System.Collections.Generic;
using System.Linq;
using OtpSharp;
using System.Net;
using System.IO;
using System.Threading;
using System.Web.Script.Serialization;
using System.Configuration;
using System;
using System.Web.UI.WebControls;
using ClosedXML.Excel;

namespace CSGO_Scapping
{
    class Program
    {
        static void Main()
        {
            var skins = new List<Skin>();
            var totpgen = new Totp(Base32.Base32Encoder.Decode(ConfigurationManager.AppSettings["SecretCode"]));

            HtmlWeb oWeb = new HtmlWeb();
            HtmlDocument doc = oWeb.Load("https://hawkstore.com.ar/tienda/?orderby=price");

            var cantPages = doc.DocumentNode.CssSelect("a.page-numbers").Count();

            for (var j = 1; j <= cantPages; j++)
            {
                if (j != 1)
                {
                    doc = oWeb.Load($"https://hawkstore.com.ar/tienda/page/{j}/?orderby=price");
                }

                List<HtmlNode> skinName = doc.DocumentNode.CssSelect("h2.woocommerce-loop-product__title").ToList();
                List<HtmlNode> skinPrice = doc.DocumentNode.CssSelect("span.price").ToList();

                for (var i = 0; i < skinName.Count(); i++)
                {
                    var _price = skinPrice[i].InnerText
                        .Replace("&#36;", string.Empty)
                        .Replace(",", string.Empty)
                        .Split('.');

                    var intPrice = int.Parse(_price[0]);

                    if (!(skins.Any(x => x.Name == skinName[i].InnerHtml && x.Price == intPrice)))
                    {
                        skins.Add(new Skin { Name = skinName[i].InnerHtml,Price = intPrice, sales = 0, promedio = 0, dolar = 0, obs = "", SalesSkins = new List<SalesSkin>() });
                    }
                }
            }

            foreach (var skin in skins)
            {
                for (var i = 1; i < 6; i++)
                {
                    var bitSkin = WebRequest.Create($"https://bitskins.com/api/v1/get_sales_info/?api_key={ConfigurationManager.AppSettings["APIKey"]}&code={totpgen.ComputeTotp()}&market_hash_name={skin.Name}&page={i}");
                    bitSkin.Method = "GET";
                    var responseObj = (HttpWebResponse)bitSkin.GetResponse();

                    using (var stream = responseObj.GetResponseStream())
                    {
                        var sr = new StreamReader(stream);
                        var result = new JavaScriptSerializer().Deserialize<DevolSkin>(sr.ReadToEnd());
                        sr.Close();

                        foreach (var sale in result.data.sales)
                        {
                            sale.price = sale.price.Trim('0').Replace('.', ',');

                            if(sale.price[sale.price.Length - 1] == ',')
                            {
                                sale.price = sale.price.Replace(",", string.Empty);
                            }
                            sale.decimalPrice = decimal.Parse(sale.price) * (decimal)0.908;

                            skin.SalesSkins.Add(sale);
                        }
                    }

                    Thread.Sleep(125);
                }
            }

            var date = DateTime.Now;

            foreach (var skin in skins)
            {
                var promedioVenta = (decimal)0;
                var promedio = (decimal)0;
                foreach (var sale in skin.SalesSkins)
                {
                    promedioVenta += decimal.Parse(sale.price);
                    promedio += sale.decimalPrice;
                    DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    dtDateTime = dtDateTime.AddSeconds(sale.sold_at).ToLocalTime();

                    if ((date - dtDateTime).TotalDays < 15)
                    {
                        skin.sales++;
                    }
                }

                if (skin.SalesSkins.Count > 0)
                {
                    skin.promedioVenta = Math.Round((promedioVenta / skin.SalesSkins.Count), 2);
                    skin.promedio = Math.Round((promedio / skin.SalesSkins.Count), 2);
                    skin.dolar = Math.Round((skin.Price / skin.promedio), 2);
                }
                else
                {
                    skin.dolar = 0;
                    skin.obs = "Sin ventas en BitSkins";
                }
            }

            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Skins en venta");

            var headers = new List<string> { "Skin", "Precio AR$", "Promedio U$D", "U$D a recibir", "Precio por dolar", "Cant. Ventas en 15 días", "Observaciones" };

            for(var i = 0; i < skins.Count; i++)
            {
                ws.Cell(i + 2, 1).Value = skins[i].Name;
                ws.Cell(i + 2, 2).Value = skins[i].Price;
                ws.Cell(i + 2, 3).Value = skins[i].promedioVenta;
                ws.Cell(i + 2, 4).Value = skins[i].promedio;
                ws.Cell(i + 2, 5).Value = skins[i].dolar;
                ws.Cell(i + 2, 6).Value = skins[i].sales;
                ws.Cell(i + 2, 7).Value = skins[i].obs;
                ws.Cell($"A{i + 2}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                if(skins[i].dolar == 0)
                {
                    for (var j = 1; j <= headers.Count; j++)
                    {
                        ws.Cell(i + 2, j).Style.Fill.BackgroundColor = XLColor.FromArgb(249, 81, 81);
                    }
                }
                else if (skins[i].dolar < 65)
                {
                    for(var j = 1; j <= headers.Count; j++)
                    {
                        ws.Cell(i + 2, j).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 240, 124);
                    }
                }
            }

            for (var i = 0; i < headers.Count; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Column(i + 1).AdjustToContents();
            }
            
            wb.SaveAs(@"E:\Documentos\DolarSkins.xlsx");
        }
    }
}
