namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class TargetDryScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Target Dry";
        public override string WebsiteBaseUrl { get; set; } = "https://www.targetdry.com/";
        public override bool Active { get; set; }
    }
}