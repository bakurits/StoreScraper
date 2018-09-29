namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class MarcWennScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Marc Wenn";
        public override string WebsiteBaseUrl { get; set; } = "https://www.marcwenn.co.uk";
        public override bool Active { get; set; }
    }
}