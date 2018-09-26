using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Bots.Html.Mstanojevic.Excelsiormilano
{

    public class ExcelsiormilanoScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Excelsiormilano";
        public override string WebsiteBaseUrl { get; set; } = "https://www.excelsiormilano.com";
        public override bool Active { get; set; }

        private const string noResults = "Sorry, no results found for your searchterm";

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            HtmlNodeCollection itemCollection = GetProductCollection(settings, null, token);

            if (itemCollection == null)
            {
                return;
            }

            foreach (var item in itemCollection)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, settings, item);
#else
                LoadSingleProductTryCatchWraper(listOfProducts, settings, item);
#endif
            }



        }


        private readonly List<String> newArrivalPageUrls = new List<string>
        {
            "https://www.excelsiormilano.com/1258-what-s-new",
            "https://www.excelsiormilano.com/815-what-s-new",
            "https://www.excelsiormilano.com/833-what-s-new",
        };



        public override void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
        {
            ConcurrentDictionary<Product, byte> data = new ConcurrentDictionary<Product, byte>();
            Task.WhenAll(newArrivalPageUrls.Select(url => GetProductsForPage(url, data, null, token))).Wait(token);
            listOfProducts = new List<Product>(data.Keys);
        }


        private async Task GetProductsForPage(string url, ConcurrentDictionary<Product, byte> data,
            SearchSettingsBase settings, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var page = (await client.GetDocTask(url, token)).DocumentNode;
            HtmlNodeCollection collection = page.SelectNodes("//div[@class='product-container']");

            foreach (var item in collection)
            {
                token.ThrowIfCancellationRequested();
                Product product = GetProduct(item);
                if (product != null && (settings == null || Utils.SatisfiesCriteria(product, settings)))
                {
                    data.TryAdd(product, 0);
                }
            }

        }


        private Product GetProduct(HtmlNode item)
        {
            try
            {
                string name = GetName(item).TrimEnd();
                string url = GetUrl(item);
                var price = GetPrice(item);
                string imageUrl = GetImageUrl(item);
                return new Product(this, name, url, price.Value, imageUrl, url, price.Currency);
            }
            catch
            {
                return null;
            }
        }


 


        private void LoadSingleProductTryCatchWraper(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            try
            {
                LoadSingleProduct(listOfProducts, settings, item);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog(e.Message);
            }
        }


        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            var document = GetWebpage(productUrl, token);
            
            var price = Utils.ParsePrice(document.SelectSingleNode("//span[@itemprop='price']").InnerText.Replace(",","."));
            
            string name = document.SelectSingleNode("//h3[@itemprop='name']").InnerText;
            string image = document.SelectSingleNode("//li[@class='homeslider-container'][1]/img").GetAttributeValue("src", "");
            string brand = null;
            if (document.SelectSingleNode("//div/h2/a") != null)
            {
                brand = document.SelectSingleNode("//div/h2/a").InnerText;
            }
            ProductDetails details = new ProductDetails()
            {
                Price = price.Value,
                Name = name,
                Currency = price.Currency,
                ImageUrl = image,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this,
                BrandName = brand
            };
            var sizeCollection = document.SelectNodes("//select[@name='group_1']/option");
            if (sizeCollection != null)
            {
                foreach (var size in sizeCollection)
                {
                    string sz = size.InnerHtml;
                    if (sz.Length > 0)
                    {
                        details.AddSize(sz, "Unknown");
                    }

                }
            }

            return details;
        }

        private HtmlNode GetWebpage(string url, CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var document = client.GetDoc(url, token).DocumentNode;
            return document;
        }

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, string gender, CancellationToken token)
        {
            //string url = string.Format(SearchFormat, settings.KeyWords);
            //string url = WebsiteBaseUrl + "/cerca?controller=search&orderby=position&orderway=desc&search_query="+settings.KeyWords.Replace(" ", "+")+"&submit_search=";

            string url = "";

          
            url = WebsiteBaseUrl + "/cerca?controller=search&orderby=position&orderway=desc&search_query="+settings.KeyWords.Replace(" ", "+")+"&submit_search=";
            

            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;

            return document.SelectNodes("//div[@class='product-container']");

        }

        private bool CheckForValidProduct(HtmlNode item, SearchSettingsBase settings)
        {
            if (item.SelectSingleNode("./div/span/span[@class='out-of-stock']") != null)
                return false;

            return true;

        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            if (!CheckForValidProduct(item, settings)) return;

            if (item.SelectSingleNode("./div[2]/div/span") == null)
                return;

            string name = GetName(item).TrimEnd();
            string url = GetUrl(item);
            var price = GetPrice(item);
            string imageUrl = GetImageUrl(item);
            var product = new Product(this, name, url, price.Value, imageUrl, url, price.Currency);
            if (Utils.SatisfiesCriteria(product, settings))
            {
                listOfProducts.Add(product);
            }
        }

        private bool GetStatus(HtmlNode item)
        {
            return true;
        }

        private string GetName(HtmlNode item)
        {
            //Console.WriteLine("GetName");
            //Console.WriteLine(item.SelectSingleNode("./a").GetAttributeValue("title", ""));

            return item.SelectSingleNode("./div[2]/h5[@itemprop='name']/a").GetAttributeValue("title", "");
        }

        private string GetUrl(HtmlNode item)
        {
            return  item.SelectSingleNode("./div/div/a").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            /* string priceDiv = item.SelectSingleNode("./div[2]/div/span").InnerHtml.Replace("$", "").Replace("€", "").Replace(",", ".");

             return double.Parse(priceDiv);*/
            return Utils.ParsePrice(item.SelectSingleNode("./div[2]/div/span").InnerHtml.Replace(",", "."));
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/div/a/img").GetAttributeValue("src", null);
        }
    }
}
