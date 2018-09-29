namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class BluKicksScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Blu Kicks";
        public override string WebsiteBaseUrl { get; set; } = "https://blukicks.com/";
        public override bool Active { get; set; }
    }
}