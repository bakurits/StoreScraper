namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class EstherScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Esther & co.";
        public override string WebsiteBaseUrl { get; set; } = "https://www.esther.com.au/";
        public override bool Active { get; set; }
    }
}