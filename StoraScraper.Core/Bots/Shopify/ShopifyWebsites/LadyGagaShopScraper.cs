namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class LadyGagaShopScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Lady Gaga Shop";
        public override string WebsiteBaseUrl { get; set; } = "https://shop.ladygaga.com/";
        public override bool Active { get; set; }
    }
}