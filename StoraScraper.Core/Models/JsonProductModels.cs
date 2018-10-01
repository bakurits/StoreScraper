using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StoreScraper.Models
{
  public class FeaturedImage
    {
        public BigInteger id { get; set; }
        public BigInteger product_id { get; set; }
        public int position { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object alt { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string src { get; set; }
        public List<long> variant_ids { get; set; }
    }

    public class Variant
    {
        public BigInteger id { get; set; }
        public string title { get; set; }
        public string option1 { get; set; }
        public string option2 { get; set; }
        public object option3 { get; set; }
        public string sku { get; set; }
        public bool requires_shipping { get; set; }
        public bool taxable { get; set; }
        public FeaturedImage featured_image { get; set; }
        public bool available { get; set; }
        public string price { get; set; }
        public int grams { get; set; }
        public object compare_at_price { get; set; }
        public int position { get; set; }
        public object product_id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class Image
    {
        public BigInteger id { get; set; }
        public DateTime created_at { get; set; }
        public int position { get; set; }
        public DateTime updated_at { get; set; }
        public object product_id { get; set; }
        public List<object> variant_ids { get; set; }
        public string src { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Option
    {
        public string name { get; set; }
        public int position { get; set; }
        public List<string> values { get; set; }
    }

    public class JsonProduct
    {
        public BigInteger id { get; set; }
        public string title { get; set; }
        public string handle { get; set; }
        public string body_html { get; set; }
        public DateTime published_at { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string vendor { get; set; }
        public string product_type { get; set; }
        public List<string> tags { get; set; }
        public List<Variant> variants { get; set; }
        public List<Image> images { get; set; }
        public List<Option> options { get; set; }
    }

    public class JsonProductsRoot
    {
        public List<JsonProduct> products { get; set; }
    }
}
