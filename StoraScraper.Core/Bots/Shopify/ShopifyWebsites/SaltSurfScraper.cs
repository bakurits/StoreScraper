namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class SaltSurfScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Salt Surf";
        public override string WebsiteBaseUrl { get; set; } = "https://store.saltsurf.com/";
        public override bool Active { get; set; }
    }
}