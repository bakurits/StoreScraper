namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class AllBirdsScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "All Birds";
        public override string WebsiteBaseUrl { get; set; } = "https://www.allbirds.com/";
        public override bool Active { get; set; }
    }
}