using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StoreScraper.Attributes;
using StoreScraper.Core;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Bots.Shopify
{
    [DisableInGUI]
    public abstract class ShopifyScraper : ScraperBase
    {

        private static readonly JsonSerializer serializer = new JsonSerializer()
        {
            Culture = CultureInfo.InvariantCulture,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            FloatParseHandling = FloatParseHandling.Double,
            DateParseHandling = DateParseHandling.DateTime,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
        };


        public virtual List<Uri> JsonEndpoints { get; set; } = new List<Uri>();
        public virtual List<Uri> XmlSitemapEndpoints { get; set; } = new List<Uri>();
        public virtual string DefaultCurrecy { get; set; } = "$";



        private ShopifyScrapMode DefaultScrapMode { get; set; } = ShopifyScrapMode.JsonProductList;


        protected ShopifyScraper()
        {
            InitEndpointList();
        }

        public virtual void InitEndpointList()
        {
            var baseurl = new Uri(this.WebsiteBaseUrl);

            JsonEndpoints = new List<Uri>()
            {
               new Uri(baseurl, "products.json")
            };
        }

        public override void FindItems(out List<Product> listOfProducts, SearchSettingsBase settings, CancellationToken token)
        {
            throw new NotSupportedException();
        }

        public override ProductDetails GetProductDetails(string productUrl, CancellationToken token)
        {
            throw new NotSupportedException();
        }

        public override void ScrapeAllProducts(out List<Product> listOfProducts, ScrappingLevel requiredInfo, CancellationToken token)
        {
            listOfProducts = new List<Product>();
            var client = ClientFactory.GetProxiedFirefoxClient(null, true);


            switch (DefaultScrapMode)
            {
                case ShopifyScrapMode.JsonProductList:
                    {
                        List<Product> products = listOfProducts;
                        Parallel.ForEach(this.JsonEndpoints, endpoint =>
                        {
                            var pageStream = client.GetAsync(endpoint).Result.Content.ReadAsStreamAsync().Result;
                            products.AddRange(ParseJsonProductsPage(pageStream));
                        });
                    }
                    return;
                case ShopifyScrapMode.XmlSitemap:
                    throw new NotSupportedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }



        private List<Product> ParseJsonProductsPage(Stream pageSourceStream)
        {
            using (StreamReader reader = new StreamReader(pageSourceStream))
            {
                JsonReader jReader = new JsonTextReader(reader);
                var parsed = serializer.Deserialize<JsonProductsRoot>(jReader);

                return parsed.products.Select(prod => ((Product)new ProductDetails()
                {
                    ScrappingLevel = ScrappingLevel.Detailed,
                    ScrapedBy = this,
                    Name = prod.title,
                    Url = new Uri(new Uri(this.WebsiteBaseUrl), prod.handle).AbsoluteUri,
                    BrandName = prod.vendor,
                    Price = prod.variants.Count > 0 ? double.Parse(prod.variants[prod.variants.Count / 2].price, NumberStyles.Number) : 0,
                    Currency = this.DefaultCurrecy,
                    KeyWords = string.Join(",", prod.tags),
                    ReleaseTime = prod.published_at,
                    Id = prod.id.ToString(),
                    ImageUrl = prod.images.FirstOrDefault()?.src,
                    SizesList = prod.variants.Where(v => v.available).Select(v => new StringPair() { Key = v.title, Value = "Unknown" }).ToList(),
                })).ToList();
            }
        }

    }


    public enum ShopifyScrapMode
    {
        JsonProductList,
        XmlSitemap,
    }
}
