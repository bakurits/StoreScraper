using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using StoreScraper.Core;
using StoreScraper.Models;
using System.Text.RegularExpressions;
using System;


namespace StoreScraper.Bots.Solebox
{
    public class SoleboxScraper : ScraperBase
    {
        public override string WebsiteName { get; set; } = "Solebox";
        public override string WebsiteBaseUrl { get; set; } = "https://www.solebox.com";
        public override bool Active { get; set; }

        private const string SearchFormat = @"https://www.solebox.com/en/variant/?ldtype=grid&_artperpage=240&listorderby=oxinsert&listorder=desc&pgNr=0&searchparam={0}";
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


        public override ProductDetails GetProductDetails(Product product, CancellationToken token)
        {
            var document = GetWebpage(product.Url, token);
            ProductDetails details = new ProductDetails();
            
            var sizeCollection = document.SelectNodes("//div[@class='size ']");
      
            foreach (var size in sizeCollection)
            {
                string sz = size.SelectSingleNode("./a").GetAttributeValue("data-size-eu", null);
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
            return client.GetDoc(url, token).DocumentNode;
        }

        private HtmlNodeCollection GetProductCollection(SearchSettingsBase settings, CancellationToken token)
        {
            string url = string.Format(SearchFormat, settings.KeyWords);
            var document = GetWebpage(url, token);
            if (document.InnerHtml.Contains(noResults)) return null;
            return document.SelectNodes("//li[@class='productData']");
        }

        private void LoadSingleProduct(List<Product> listOfProducts, SearchSettingsBase settings, HtmlNode item)
        {
            if (GetStatus(item)) return;
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
            return !(item.SelectNodes(".//div[@class='release']") == null);
        }
        
        private string GetName(HtmlNode item)
        {
            return item.SelectSingleNode("./a/div[@class='titleBlock title']").SelectNodes("./div")[1].InnerHtml;
        }

        private string GetUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a").GetAttributeValue("href", null);
        }

        private double GetPrice(HtmlNode item)
        {
            string priceDiv = item.SelectSingleNode("./a/div[@class='priceBlock']/div").InnerHtml;
            return Convert.ToDouble(Regex.Match(priceDiv, @"(\d+,\d+)").Groups[0].Value.Replace(",", "."));
        }

        private string GetImageUrl(HtmlNode item)
        {
            return item.SelectSingleNode("./a/div[@class='gridPicture']/img").GetAttributeValue("src", null);
        }
    }
}
