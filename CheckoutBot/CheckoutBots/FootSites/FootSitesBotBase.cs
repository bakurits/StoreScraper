using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Interfaces;
using CheckoutBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StoreScraper.Attributes;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;

namespace CheckoutBot.CheckoutBots.FootSites
{
    [DisableInGUI]
    public abstract class FootSitesBotBase : IGuestCheckouter, IAccountCheckouter, IReleasePageScraper
    {
        public string WebsiteName { get; set; }
        public string WebsiteBaseUrl { get; set; }
        public string ReleasePageApiEndpoint { get; set; }

        protected FootSitesBotBase(string WebsiteName, string WebSiteBaseUrl, string releasePageEndpoint)
        {
            this.WebsiteName = WebsiteName;
            this.WebsiteBaseUrl = WebSiteBaseUrl;
            this.ReleasePageApiEndpoint = releasePageEndpoint;
        }


        public abstract void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token);
        public abstract void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token);
        public abstract void Login(string username, string password);

        public List<Product> ScrapeReleasePage(CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var task = client.GetStringAsync(ReleasePageApiEndpoint);
            task.Wait(token);

            if (task.IsFaulted) throw new JsonException("Can't get data");

            var produtData = Utils.GetFirstJson(task.Result).GetValue("releases");
            var products = GetProducts(produtData);
            return products;
        }

        private List<Product> GetProducts(JToken data)
        {
            var products = new List<Product>();
            foreach (var day in data)
            {
                var productsOnDay = day["products"];
                foreach (var productData in productsOnDay)
                    try
                    {
                        var date = GetDate(productData);
                        if (date < DateTime.UtcNow) continue;
                        var name = GetName(productData);
                        var price = GetPrice(productData);
                        var url = GetUrl(productData);
                        var image = GetImage(productData);
                        

                        var product = new Product(null, name, url, price, image, url, "USD", date);

                        products.Add(product);
                    }
                    catch
                    {
                        Debug.WriteLine(productData);
                    }
            }

            return products;
        }

        private string GetName(JToken productData)
        {
            if (productData["name"].Type != JTokenType.Null)
                return (string) productData["name"];
            return "Not Avaliable";
        }

        private double GetPrice(JToken productData)
        {
            if (productData["price"].Type != JTokenType.Null)
                return (double) productData["price"];
            return 0;
        }

        private string GetUrl(JToken productData)
        {
            if (productData["buyNowURL"].Type != JTokenType.Null)
                return ((string) productData["buyNowURL"]).StringBefore("/?sid=");
            return "Not Avaliable";
        }

        private string GetImage(JToken productData)
        {
            if (productData["primaryImageURL"].Type != JTokenType.Null)
                return (string) productData["primaryImageURL"];
            return "Not Avaliable";
        }

        private DateTime GetDate(JToken productData)
        {
            if (productData["launchDateTimeUTC"].Type != JTokenType.Null)
            {
                var dateInJson = (string) productData["launchDateTimeUTC"];
                var date = DateTime.Parse(dateInJson);
                return date;
            }

            return DateTime.MaxValue;
        }
    }
}
