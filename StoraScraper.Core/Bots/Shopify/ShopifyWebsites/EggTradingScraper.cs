namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class EggTradingScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "EggTrading";
        public override string WebsiteBaseUrl { get; set; } = "https://www.eggtrading.com";
        public override bool Active { get; set; }
    }
}