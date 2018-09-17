using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;


using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;


namespace StoreScraper.Bots.Mstanojevic.JimmyJazz
{

    public class JimmyJazzScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "JimmyJazz";
        public override string WebsiteBaseUrl { get; set; } = "https://www.jimmyjazz.com";
        public override bool Active { get; set; }

        private const string noResults = "Sorry, no results found for your searchterm";
        private ConcurrentBag<HtmlNodeCollection> cb = new ConcurrentBag<HtmlNodeCollection>();
        private const int pageDepth = 2;


        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {

            string gender = null;

            listOfProducts = new List<Product>();

            
                HtmlNodeCollection itemCollection = GetProductCollection(settings, gender, token);
                Console.WriteLine(itemCollection.Count);
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
            "https://www.jimmyjazz.com/mens/specials/new-arrivals?category=footwear&sort=most-recent&ppg=104",
            "https://www.jimmyjazz.com/mens/specials/new-arrivals?category=clothing&sort=most-recent&ppg=104",
            "https://www.jimmyjazz.com/mens/specials/new-arrivals?category=accessories&sort=most-recent&ppg=104",
        };



        public override void ScrapeNewArrivalsPage(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
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
            HtmlNodeCollection collection = page.SelectNodes("//li[contains(@class,'product_grid_item')]");

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
                string url = WebsiteBaseUrl + GetUrl(item);
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
            var price = Utils.ParsePrice(document.SelectSingleNode("//span[@class='product_price']").InnerHtml);


            string name = document.SelectSingleNode("//span[@class='name']").InnerText.Trim();
            string image = document.SelectSingleNode("//img[@id='main-image']").GetAttributeValue("src", "");

            string brand = null;
            if (document.SelectSingleNode("//span[@class='brand']") != null)
            {
                brand = document.SelectSingleNode("//span[@class='brand']").InnerText;
            }


            ProductDetails details = new ProductDetails()
            {
                Price = price.Value,
                Name = name,
                Currency = price.Currency.Replace("&EURO;", "EUR"),
                ImageUrl = image,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this,
                BrandName = brand
            };

            var sizeCollection = document.SelectNodes("//div[@class='psizeoptioncontainer']/div/a");
            if (sizeCollection != null)
            {
                foreach (var size in sizeCollection)
                {

                    if (!size.GetAttributeValue("class", "").Contains("piunavailable"))
                    {
                        string sz = size.InnerHtml;


                        if (sz.Length > 0)
                        {
                            details.AddSize(sz, "Unknown");
                        }
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
            string url = "";


       
                url = "https://search.jimmyjazz.com/search/keywords-" + settings.KeyWords.Replace(" ", "_") + "--res_per_page-100";

                if (settings.MaxPrice > 0)
                {
                    url += "--Price-" + settings.MinPrice.ToString() + "%7C%7C" + settings.MaxPrice.ToString();
                }

                if (gender != null)
                {
                    url += "--Gender-" + gender;
                }

            

            Console.WriteLine(url);
            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;

          

                return document.SelectNodes("//div[contains(@class,'product_grid_item')]");
           

        }

        /*private void scrapePage(int pg, CancellationToken token)
        {
            string url = WebsiteBaseUrl + "/mens/specials/new-arrivals?ppg=104&page="+pg.ToString();

            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return;
            Console.WriteLine(pg);
            Console.WriteLine(document.SelectNodes("//div[@class='product_grid_image quicklook-unprocessed']"));
            cb.Add(document.SelectNodes("//div[@class='product_grid_image quicklook-unprocessed']"));
        }*/

       /* private bool CheckForValidProduct(HtmlNode item, SearchSettingsBase settings)
        {
            string title = item.SelectSingleNode("./a").GetAttributeValue("title", "").ToLower();
            var validKeywords = settings.KeyWords.ToLower().Split(' ');
            var invalidKeywords = settings.NegKeyWrods.ToLower().Split(' ');
            foreach (var keyword in validKeywords)
            {
                if (!title.Contains(keyword))
                    return false;
            }

            
                foreach (var keyword in invalidKeywords)
                {
                if (keyword == "")
                    continue;
                    if (title.Contains(keyword))
                        return false;
                }
            

            return true;

        }*/

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            //if (!CheckForValidProduct(item, settings)) return;
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

            return item.SelectSingleNode("./div/a").GetAttributeValue("title", "");
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/a").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            /*try
            {
                string priceDiv = item.SelectSingleNode("./div[contains(@class,'product_grid_price ')]/span").InnerHtml.Replace("$", "");

                return double.Parse(priceDiv);
            }
            catch
            {
                return 0;
            }*/

            return Utils.ParsePrice(item.SelectSingleNode("./div[contains(@class,'product_grid_price ')]/span").InnerHtml.Replace(",", "."));
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/a/img").GetAttributeValue("src", null);
        }
    }
}
