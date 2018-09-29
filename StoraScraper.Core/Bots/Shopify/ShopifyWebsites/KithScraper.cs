namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class KithScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Kith";
        public override string WebsiteBaseUrl { get; set; } = "https://kith.com/";
        public override bool Active { get; set; }
    }
}