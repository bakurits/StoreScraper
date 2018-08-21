using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Mstanojevic.Cruvoir
{

    public class CruvoirScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Cruvoir";
        public override string WebsiteBaseUrl { get; set; } = "https://www.Cruvoir.com";
        public override bool Active { get; set; }

        private const string noResults = "Sorry, no results found for your searchterm";

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            HtmlNodeCollection itemCollection = GetProductCollection(settings, token);
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
            if (document.SelectSingleNode("//h1[@class='price']/span[@class='sale']/span") != null)
            {
                price = Utils.ParsePrice(document.SelectSingleNode("//h1[@class='price']/span[@class='sale']/span").InnerText);
            }
            else
            {
                price = Utils.ParsePrice(document.SelectSingleNode("//h1[@class='price']").InnerText);
            }
            string name = document.SelectSingleNode("//h1[not(@class='price')]").InnerText;
            string image = document.SelectSingleNode("//div[@class='product-gallery']/img[1]").GetAttributeValue("src", "");

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

            var sizeCollection = document.SelectNodes("//select[@name='id']/option");
            if (sizeCollection != null)
            {
                foreach (var size in sizeCollection)
                {
                    string sz = size.InnerHtml.Trim();
                    if (sz.Contains("Sold out"))
                    {
                        continue;
                    }
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
            string url = WebsiteBaseUrl + "/collections/sneakers";

            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;

            return document.SelectNodes("//article[@class='product']");

        }

        private bool CheckForValidProduct(HtmlNode item, SearchSettingsBase settings)
        {
            string title = item.SelectSingleNode("./div/p[2]").InnerHtml.ToLower();
            var validKeywords = settings.KeyWords.ToLower().Split(' ');
            foreach (var keyword in validKeywords)
            {
                if (!title.Contains(keyword))
                    return false;
            }

            if (item.SelectSingleNode("./div/p[@class='sizes']/span[@class='sold-out']") != null)
            {
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

            return item.SelectSingleNode("./div/p[2]").InnerHtml;
        }

        private string GetUrl(HtmlNode item)
        {
            return WebsiteBaseUrl + item.SelectSingleNode("./a").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            /*try
            {
                string priceDiv = item.SelectSingleNode("./div/p[3]/span").InnerHtml.Replace("$", "").Replace("USD", "").Replace("€", "").Replace(",", ".");

                return double.Parse(priceDiv);
            }
            catch
            {
                return 0;
            }*/

            return Utils.ParsePrice(item.SelectSingleNode("./div/p[3]/span").InnerHtml.Replace(",","."));


        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a/img").GetAttributeValue("src", null);
        }
    }
}
