namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class TaylorStitchScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Taylor Stitch";
        public override string WebsiteBaseUrl { get; set; } = "https://www.taylorstitch.com/";
        public override bool Active { get; set; }
    }
}