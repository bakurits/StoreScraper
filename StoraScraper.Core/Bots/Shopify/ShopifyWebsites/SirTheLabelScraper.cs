namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class SirTheLabelScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Sir.";
        public override string WebsiteBaseUrl { get; set; } = "https://sirthelabel.com/";
        public override bool Active { get; set; }
    }
}