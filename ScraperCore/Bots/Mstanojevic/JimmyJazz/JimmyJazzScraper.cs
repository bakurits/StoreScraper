using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Mstanojevic.JimmyJazz
{

    public class JimmyJazzScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "JimmyJazz";
        public override string WebsiteBaseUrl { get; set; } = "http://www.jimmyjazz.com";
        public override bool Active { get; set; }

        private const string noResults = "Sorry, no results found for your searchterm";
        private ConcurrentBag<HtmlNodeCollection> cb = new ConcurrentBag<HtmlNodeCollection>();
        private const int pageDepth = 2;


        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {

            string gender = null;

            listOfProducts = new List<Product>();

            if (settings.Mode == ScraperCore.Models.SearchMode.SearchAPI)
            {

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
            else
            {

                HtmlNodeCollection itemCollection = GetProductCollection(settings, "man", token);
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


                itemCollection = GetProductCollection(settings, "woman", token);
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
        }

        public override void ScrapeNewArrivalsPage(out List<Product> listOfProducts, CancellationToken token)
        {
            listOfProducts = new List<Product>();

            HtmlNodeCollection itemCollection = GetNewArriavalItems(WebsiteBaseUrl + "/mens/specials/new-arrivals", token);
            foreach (var item in itemCollection)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleNewArrivalProduct(listOfProducts, item);
#else
                LoadSingleNewArrivalProductTryCatchWraper(listOfProducts, null, item);
#endif
            }

            itemCollection = GetNewArriavalItems(WebsiteBaseUrl + "/womens/specials/new-arrivals", token);
            foreach (var item in itemCollection)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleNewArrivalProduct(listOfProducts, item);
#else
                LoadSingleNewArrivalProductTryCatchWraper(listOfProducts, null, item);
#endif
            }

        }


        private HtmlNodeCollection GetNewArriavalItems(string url, CancellationToken token)
        {
            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;

            return document.SelectNodes("//li[contains(@class,'product_grid_item')]");

        }

        private void LoadSingleNewArrivalProduct(List<Product> listOfProducts, HtmlNode item)
        {
            string name = GetName(item).TrimEnd();
            string url = GetUrl(item);
            var price = GetPrice(item);
            string imageUrl = GetImageUrl(item);
            var product = new Product(this, name, url, price.Value, imageUrl, url, price.Currency);
            listOfProducts.Add(product);

        }

        private void LoadSingleNewArrivalProductTryCatchWraper(List<Product> listOfProducts, HtmlNode item)
        {
            try
            {
                LoadSingleNewArrivalProduct(listOfProducts, item);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteErrorLog(e.Message);
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



            ProductDetails details = new ProductDetails()
            {
                Price = price.Value,
                Name = name,
                Currency = price.Currency.Replace("&EURO;", "EUR"),
                ImageUrl = image,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this
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


            if (settings.Mode == ScraperCore.Models.SearchMode.SearchAPI)
            {

                url = "http://search.jimmyjazz.com/search/keywords-" + settings.KeyWords.Replace(" ", "_") + "--res_per_page-100";

                if (settings.MaxPrice > 0)
                {
                    url += "--Price-" + settings.MinPrice.ToString() + "%7C%7C" + settings.MaxPrice.ToString();
                }

                if (gender != null)
                {
                    url += "--Gender-" + gender;
                }

            }
            else
            {
                if (gender == "man")
                {
                    url = WebsiteBaseUrl + "/mens/specials/new-arrivals";
                }
                else
                {
                    url = WebsiteBaseUrl + "/womens/specials/new-arrivals";
                }
            }

            Console.WriteLine(url);
            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;

            if (settings.Mode == ScraperCore.Models.SearchMode.SearchAPI)
            {

                return document.SelectNodes("//div[contains(@class,'product_grid_item')]");
            }
            else
            {
                return document.SelectNodes("//li[contains(@class,'product_grid_item')]");

            }

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
