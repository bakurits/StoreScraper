using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckOutBot.Models
{
    public class CartItem
    {
        [DisplayName("Product")]
        public Product ShopProduct { get; set; }

        [DisplayName("Purchase Options")]
        public object BuyOptions { get; set; }


        public CartItem(Product shopProduct, object buyOptions)
        {
            this.ShopProduct = shopProduct;
            this.BuyOptions = buyOptions;
        }


        public override string ToString()
        {
            return ShopProduct.StoreName +  ", " + ShopProduct.Name + ", " + ShopProduct.Price;
        }
    }
}
