namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class ArgentScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Argent";
        public override string WebsiteBaseUrl { get; set; } = "https://argentwork.com/";
        public override bool Active { get; set; }
    }
}