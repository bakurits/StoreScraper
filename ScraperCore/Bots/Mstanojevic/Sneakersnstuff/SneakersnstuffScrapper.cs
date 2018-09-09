using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Bots.Mstanojevic.Sneakersnstuff
{

    public class SneakersnstuffScrapper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Sneakersnstuff";
        public override string WebsiteBaseUrl { get; set; } = "https://www.sneakersnstuff.com";
        public override bool Active { get; set; }

        private const string noResults = "Sorry, no results found for your searchterm";

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            string gender = null;
            listOfProducts = new List<Product>();
            HtmlNodeCollection itemCollection = GetProductCollection(settings, gender, token);
            Console.WriteLine(itemCollection);
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

            HtmlNodeCollection itemCollection = GetNewArriavalItems(WebsiteBaseUrl + "/en/858/new-arrivals", token);
            foreach (var item in itemCollection)
            {
                token.ThrowIfCancellationRequested();
#if DEBUG
                LoadSingleNewArrivalProduct(listOfProducts, item);
#else
                LoadSingleNewArrivalProductTryCatchWraper(listOfProducts, item);
#endif
            }

          

        }


        private HtmlNodeCollection GetNewArriavalItems(string url, CancellationToken token)
        {
            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;

            return document.SelectNodes("//li[@class='product c-3']");

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

            if (document.SelectSingleNode("//div[@class='product-price']/span[@class='sale']") != null)
            {
                price = Utils.ParsePrice(document.SelectSingleNode("//div[@class='product-price']/span[@class='sale']").InnerText);
            }
            else
            {
                price = Utils.ParsePrice(document.SelectSingleNode("//div[@class='product-price']/span[@class='price']").InnerText);
            }



            string name = document.SelectSingleNode("//h1[@id='product-name']").InnerText.Trim();
            string image = WebsiteBaseUrl + document.SelectSingleNode("//img[@id='primary-image']").GetAttributeValue("src", "");



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
            var sizeCollection = document.SelectNodes("//span[@class='size-type']");
            if (sizeCollection != null)
            {

                foreach (var size in sizeCollection)
                {
                    string sz = size.GetAttributeValue("title", size.InnerText).Trim();
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
            //string url = WebsiteBaseUrl + "/en/858/new-arrivals";
            string url = WebsiteBaseUrl + "/en/search/searchbytext?key=" + settings.KeyWords;

            if (gender != null)
            {
                if (gender == "men")
                {
                    url += "?p=950&orderBy=Published";
                }
                else if (gender == "women")
                {
                    url += "?p=820&orderBy=Published";
                }
                else if (gender == "unisex")
                {
                    url += "?p=807&orderBy=Published";
                }
            }

            var document = GetWebpage(url, token);
            //Console.WriteLine(document);
            if (document.InnerHtml.Contains(noResults)) return null;
            
            return document.SelectNodes("//li[@class='product c-3']");

        }

        


        private bool CheckForValidProduct(HtmlNode item, SearchSettingsBase settings)
        {

            if (item.SelectSingleNode("./div[@class = 'list-product-countdown banner']") != null)
                return false;

            if (item.SelectSingleNode("./a/span[@class='sale-tag en soldout']") != null)
                return false;

            return true;

        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            if (!CheckForValidProduct(item, settings))
                return;

            string name = GetName(item).TrimEnd();
            string url = WebsiteBaseUrl + GetUrl(item);
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

            return item.SelectSingleNode("./div/h4/a").InnerHtml;
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a").GetAttributeValue("href", null);
        }

        private Price GetPrice(HtmlNode item)
        {
            /*string priceDiv = item.SelectSingleNode("./div/span[@class='price-container']/span").InnerHtml.Replace("$", "").Replace(",", ".");

            return double.Parse(priceDiv);*/
            return Utils.ParsePrice(item.SelectSingleNode("./div/span[@class='price-container']/span").InnerHtml.Replace(",", "."));
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a/img").GetAttributeValue("src", null);
        }
    }
}
