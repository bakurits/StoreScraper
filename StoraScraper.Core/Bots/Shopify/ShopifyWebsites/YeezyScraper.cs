namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class YeezyScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Yeezy";
        public override string WebsiteBaseUrl { get; set; } = "https://yeezysupply.com/";
        public override bool Active { get; set; }
    }
}