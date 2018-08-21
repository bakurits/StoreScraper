using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Mstanojevic.FootShop
{
    public class FootShopScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "FootShop";
        public override string WebsiteBaseUrl { get; set; } = "http://www.footshop.eu";
        public override bool Active { get; set; }

        private const string noResults = "Sorry, no results found for your searchterm";

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {

            listOfProducts = new List<Product>();

            string gender = "male";

            string restApiUrl = "http://www.footshop.eu/en/?controller=products&listing_type=search&listing_type_id=0&search_query=" + settings.KeyWords + "&page=1";

            if (gender != null)
            {
                restApiUrl += "&sex=%5B%22"+gender+"%22%5D&";

            }
            
            if (settings.MaxPrice > 0)
            {
                restApiUrl += "&price%5Bmin%5D=" + settings.MinPrice.ToString() + "&price%5Bmax%5D="+settings.MaxPrice.ToString();
            }

            Console.WriteLine(restApiUrl);
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var response = Utils.GetParsedJson(client, restApiUrl, token);

            foreach (var item in response["products"]["items"])
            {
                double price = 0;
                try
                {
                    string str = item["price"]["value"].ToString();
                    price = double.Parse(str);
                }
                catch
                {

                }
                var product = new Product(this, item["name"].ToString(), WebsiteBaseUrl+"/"+ item["url"].ToString(), price, item["image"].ToString(), item["id"].ToString(), "EUR");
                if (Utils.SatisfiesCriteria(product, settings))
                {
                    listOfProducts.Add(product);
                }

            }


            /*HtmlNodeCollection itemCollection = GetProductCollection(settings, token);
            Console.WriteLine(itemCollection.Count);
            foreach (var item in itemCollection)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleProduct(listOfProducts, settings, item);
#else
                LoadSingleProductTryCatchWraper(listOfProducts, settings, item);
#endif
            }*/

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
            var price = Utils.ParsePrice(document.SelectSingleNode("//p[@class='our_price_display']/span[@class='price']").InnerHtml);


            string name = document.SelectSingleNode("//h1").InnerText;
            string image = document.SelectSingleNode("//div[@class='owl-carousel']/div/img").GetAttributeValue("data-src", "");

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


            var sizeCollection = document.SelectNodes("//select[@id='size-select']/option");

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

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            //string url = string.Format(SearchFormat, settings.KeyWords);
            string url = WebsiteBaseUrl + "/en/1551-latest/categories-mens_shoes-womens_shoes-kids_shoes";

            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;

            return document.SelectNodes("//a[@class='product']");

        }

        private bool CheckForValidProduct(HtmlNode item, SearchSettingsBase settings)
        {
            string title = item.SelectSingleNode("./div[@class='product__name']/h3").InnerText.ToLower();
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

        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            //if (!CheckForValidProduct(item, settings)) return;
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

            return item.SelectSingleNode("./div[@class='product__name']/h3").InnerText;
        }

        private string GetUrl(HtmlNode item)
        {
            return WebsiteBaseUrl + item.SelectSingleNode(".").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            /*var node = item.SelectSingleNode("./div[@class='product__price']/b");
            if (node != null)
            {
                string priceDiv = item.SelectSingleNode("./div[@class='product__price']/b").InnerHtml.Replace("€", "").Replace("&euro;", "").Replace("$", "").Replace(",", ".");

                return double.Parse(priceDiv);
            }
            else
            {
                return 0;
            }*/

            return Utils.ParsePrice(item.SelectSingleNode("./div[@class='product__price']/b").InnerHtml.Replace(",", "."));
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./div/img").GetAttributeValue("src", null);
        }
    }
}

