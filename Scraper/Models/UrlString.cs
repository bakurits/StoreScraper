using System;

namespace CheckOutBot.Models
{
    public class UrlString
    {
        public string Url { get; set; } = "";

        public static implicit operator string(UrlString s)
        {
            return s.Url;
        }

        public override string ToString()
        {

            return Url;
        }
    }
}