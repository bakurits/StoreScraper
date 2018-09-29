namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class SkinnyDipLondonScrapper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Skinny Dip";
        public override string WebsiteBaseUrl { get; set; } = "https://www.skinnydiplondon.com/";
        public override bool Active { get; set; }
    }
}