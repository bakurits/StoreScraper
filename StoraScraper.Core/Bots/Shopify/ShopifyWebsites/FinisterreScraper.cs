namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class FinisterreScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Finisterre";
        public override string WebsiteBaseUrl { get; set; } = "https://finisterre.com/";
        public override bool Active { get; set; }
    }
}