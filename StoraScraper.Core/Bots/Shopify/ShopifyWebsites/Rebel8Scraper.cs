namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class Rebel8Scraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Rebel 8";
        public override string WebsiteBaseUrl { get; set; } = "https://rebel8.com/";
        public override bool Active { get; set; }
    }
}