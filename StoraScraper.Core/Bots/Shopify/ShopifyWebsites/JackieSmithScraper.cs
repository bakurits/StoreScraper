namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class JackieSmithScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Jackie Smith";
        public override string WebsiteBaseUrl { get; set; } = "https://jackiesmith.com/";
        public override bool Active { get; set; }
    }
}