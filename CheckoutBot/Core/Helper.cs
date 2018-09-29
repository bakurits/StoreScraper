using System;
using System.Net;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using StoreScraper.Interfaces;
using DColor = System.Drawing.Color;
using MColor = System.Windows.Media.Color;

namespace CheckoutBot.Core
{
    public static class Helper
    {
        public static void AppendText(this RichTextBox box, string text, DColor color)
        {
            box.Dispatcher.Invoke(() =>
            {
                TextRange range = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd) {Text = text};
                range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(ToMediaColor(color)));
            });
        }


        private static MColor ToMediaColor(this DColor color)
        {
            return MColor.FromArgb(color.A, color.R, color.G, color.B);
        }


        private static readonly Random Rand = new Random();

        public static WebProxy GetRandomProxy(IWebsiteScraper bot)
        {
            var lst = AppData.Session.ParsedProxies[bot];
            return lst[Rand.Next(lst.Count - 1)];
        }
    }
}