using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StoreScraper.Bots.Mstanojevic.Dtlr
{

    public class DtlrScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "DTLR";
        public override string WebsiteBaseUrl { get; set; } = "http://www.dtlr.com";
        public override bool Active { get; set; }

        private const string noResults = "Sorry, no results found for your searchterm";

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {


            listOfProducts = new List<Product>();
            HtmlNodeCollection itemCollection = GetProductCollection(settings, null, token);
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

        public override void ScrapeNewArrivalsPage(out List<Product> listOfProducts, CancellationToken token)
        {
            listOfProducts = new List<Product>();

            HtmlNodeCollection itemCollection = GetNewArriavalItems(WebsiteBaseUrl + "/men/footwear/new.html", token);
            foreach (var item in itemCollection)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleNewArrivalProduct(listOfProducts, item);
#else
                LoadSingleNewArrivalProductTryCatchWraper(listOfProducts, null, item);
#endif
            }

            itemCollection = GetNewArriavalItems(WebsiteBaseUrl + "/women/footwear/new.html", token);
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

            return document.SelectNodes("//li[@class='notmobile item last']");

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
            Price price;
            if (document.SelectSingleNode("//p[@class='special-price']/span[@class='price']") != null) { 
                price = Utils.ParsePrice(document.SelectSingleNode("//p[@class='special-price']/span[@class='price']").InnerText);
            }else{
                price = Utils.ParsePrice(document.SelectSingleNode("//div[@class='price-info']/div[@class='price-box']/span[@class='regular-price']/span").InnerText);
            }
            string name = document.SelectSingleNode("//h1[@itemprop='name']").InnerText;
            string image = document.SelectSingleNode("//div[@class='product-image-gallery']/img[1]").GetAttributeValue("src", "");

            ProductDetails details = new ProductDetails()
            {
                Price = price.Value,
                Name = name,
                Currency = price.Currency,
                ImageUrl = image,
                Url = productUrl,
                Id = productUrl,
                ScrapedBy = this
            };

            var strDoc = document.InnerHtml;

            if (strDoc.Contains("var spConfig = new Product.Config({"))
            {
                var start = strDoc.IndexOf("var spConfig = new Product.Config({");
                var trimmed = strDoc.Substring(start, strDoc.Length - start);
                var end = trimmed.IndexOf(");");

                trimmed = trimmed.Substring(0, end);

                trimmed = trimmed.Replace("var spConfig = new Product.Config(", "");
                JObject obj = JObject.Parse(trimmed);

                foreach (var attr in obj["attributes"])
                {

                    foreach (var x in attr)
                    {
                        if (x["code"].ToString() == "size")
                        {
                            foreach (var option in x["options"])
                            {
                                details.AddSize(option["label"].ToString(), "Unknown");
                            }
                        }


                    }
                }

            }
            // need to parse javascript to extract prices
            /*var sizeCollection = document.SelectNodes("//div[@class='sizeBox']/ul/li");

            foreach (var size in sizeCollection)
            {
                string sz = size.InnerHtml;
                if (sz.Length > 0)
                {
                    details.AddSize(sz, "Unknown");
                }

            }*/

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

            //string url = WebsiteBaseUrl + "/catalogsearch/result/?q="+settings.KeyWords.Replace(" ", "+");
            
            url = WebsiteBaseUrl + "/catalogsearch/result/?q=" + settings.KeyWords.Replace(" ", "+");
            if (gender != null)
            {
                url += "&gender=" + gender;
            }
            


            

            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;

            return document.SelectNodes("//li[@class='notmobile item last']");

        }

        private bool CheckForValidProduct(HtmlNode item, SearchSettingsBase settings)
        {
            string title = item.SelectSingleNode("./div/p[@class='product-name']").InnerText.ToLower();
            var validKeywords = settings.KeyWords.ToLower().Split(' ');
            var invalidKeywords = settings.NegKeyWords.ToLower().Split(' ');
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

        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            if (!CheckForValidProduct(item, settings)) return;
            string name = GetName(item).TrimEnd();
            string url = GetUrl(item);
            var price = GetPrice(item);

            /*if (!(price >= settings.MinPrice && price <= settings.MaxPrice) && (settings.MaxPrice != 0 && settings.MinPrice != 0))
            {
                return;
            }*/


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

            return item.SelectSingleNode("./div/p[@class='product-name']").InnerText;
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            /*var node = item.SelectSingleNode("./div/div[@class='price-box']/span/span");
            if (node != null)
            {
                string priceDiv = item.SelectSingleNode("./div/div[@class='price-box']/span/span").InnerHtml.Replace("€", "").Replace("&euro;", "").Replace("$", "");

                return double.Parse(priceDiv);
            }
            else
            {
                return 0;
            }*/

            return Utils.ParsePrice(item.SelectSingleNode("./div/div[@class='price-box']/span/span").InnerHtml);
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a/img").GetAttributeValue("src", null);
        }
    }
}
