using System;
using System.Collections.Generic;
using System.Threading;
using CheckoutBot.Interfaces;
using CheckoutBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StoreScraper.Bots.Sticky_bit.ChampsSports_FootLocker;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;

namespace CheckoutBot.CheckoutBots.EastBay
{
    public class EastBayBot : FootSimpleBase.EastBayScraper, IGuestCheckouter, IAccountCheckouter, IReleasePageScraper
    {
        public void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private string ApiUrl { get; } = "https://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/1";

        public List<Product> ScrapeReleasePage(CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var task = client.GetStringAsync(ApiUrl);
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
                {
                    string url = ((string) productData["buyNowURL"]).StringBefore("/?sid=");
                    double price = (double) productData["price"];
                    string image = (string) productData["primaryImageURL"];
                    string dateInJson = (string) productData["launchDateTimeUTC"];
                    string dateFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
                    DateTime date = DateTime.ParseExact(dateFormat, dateInJson, null);
                    Product product = new Product(null,
                        (string) productData["name"], // name
                        url, // url
                        price, // price
                        image, // image
                        url, //id
                        "USD", // currency 
                        date < DateTime.UtcNow ? (DateTime?) null : date); // date
                }
            }

            return products;
        }
    }
}