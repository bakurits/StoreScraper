namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class GreatsScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Greats";
        public override string WebsiteBaseUrl { get; set; } = "https://www.greats.com/";
        public override bool Active { get; set; }
    }
}