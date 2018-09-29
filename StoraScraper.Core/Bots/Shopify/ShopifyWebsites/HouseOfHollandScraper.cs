namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class HouseOfHollandScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "House Of Holland";
        public override string WebsiteBaseUrl { get; set; } = "https://www.houseofholland.co.uk/";
        public override bool Active { get; set; }
    }
}