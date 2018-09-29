namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class RyderScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Ryder";
        public override string WebsiteBaseUrl { get; set; } = "https://ryderlabel.com/";
        public override bool Active { get; set; }
    }
}