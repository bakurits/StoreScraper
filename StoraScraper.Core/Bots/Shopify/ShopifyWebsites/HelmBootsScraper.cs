namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class HelmBootsScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Helm";
        public override string WebsiteBaseUrl { get; set; } = "https://helmboots.com";
        public override bool Active { get; set; }
    }
}