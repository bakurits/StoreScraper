using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Mstanojevic.UebervartShop
{
    public class UebervartShopScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "UebervartShop";
        public override string WebsiteBaseUrl { get; set; } = "https://www.uebervart-shop.de";
        public override bool Active { get; set; }

        private const string noResults = "Sorry, no results found for your searchterm";

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            HtmlNodeCollection itemCollection = GetProductCollection(settings, token);
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

            if (document.SelectSingleNode("//ins/span[@class='woocommerce-Price-amount amount']") != null)
            {
                price = Utils.ParsePrice(document.SelectSingleNode("//ins/span[@class='woocommerce-Price-amount amount']").InnerText.Replace(",", ".").Replace("&nbsp;", ""));

            }
            else
            {
                price = Utils.ParsePrice(document.SelectSingleNode("//span[@class='woocommerce-Price-amount amount']").InnerText.Replace(",", ".").Replace("&nbsp;", ""));

            }



            string name = document.SelectSingleNode("//h3[@class='product_title']").InnerText.Trim();
            string image = document.SelectSingleNode("//div[@class='swiper-slide']/img").GetAttributeValue("src", "");



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

            var sizeCollection = document.SelectNodes("//td[@class='value']/label");

            foreach (var size in sizeCollection)
            {
                string sz = size.InnerHtml;
                if (sz.Length > 0)
                {
                    details.AddSize(sz, "Unknown");
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
            string url = WebsiteBaseUrl + "/?s=" + settings.KeyWords.Replace(" ", "+") + "&post_type=product";

            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;

            return document.SelectNodes("//article");

        }





        private bool CheckForValidProduct(HtmlNode item, SearchSettingsBase settings)
        {

            if (item.SelectSingleNode("./a/div[@class = 'soldout']") != null)
                return false;


            return true;

        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            if (!CheckForValidProduct(item, settings))
                return;

            string name = GetName(item).TrimEnd();
            string url = GetUrl(item);
            double price = GetPrice(item);


            string imageUrl = GetImageUrl(item);
            var product = new Product(this, name, url, price, imageUrl, url, "EUR");
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

            return item.SelectSingleNode("./a/h3").InnerText;
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a").GetAttributeValue("href", null);
        }

        private double GetPrice(HtmlNode item)
        {
            try
            {
                if (item.SelectSingleNode("./a/span/ins/span") != null)
                {
                    string priceDiv = item.SelectSingleNode("./a/span/ins/span").InnerText.Replace("€", "").Replace("&euro;", "").Replace("&nbsp;", "").Replace("$", "").Replace(",", ".");

                    return double.Parse(priceDiv);
                }
                else
                {
                    
                    string priceDiv = item.SelectSingleNode("./a/span/span").InnerText.Replace("€", "").Replace("&euro;", "").Replace("&nbsp;", "").Replace("$", "").Replace(",", ".");

                    return double.Parse(priceDiv);
                }
            }
            catch
            {
                return 0;
            }
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a/img").GetAttributeValue("src", null);
        }
    }
}

