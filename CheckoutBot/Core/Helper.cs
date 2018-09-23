using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Net;
using System.Threading;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using CheckoutBot.CheckoutBots.FootSites;
using ScraperCore.Interfaces;
using StoreScraper.Models;
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
                TextRange range = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
                range.Text = text;
                range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(ToMediaColor(color)));
            });
        }


        public static MColor ToMediaColor(this DColor color)
        {
            return MColor.FromArgb(color.A, color.R, color.G, color.B);
        }


        static Random rand = new Random();

        public static WebProxy GetRandomProxy(IWebsiteScraper bot)
        {
            var lst = AppData.Session.ParsedProxies[bot];
            return lst[rand.Next(lst.Count - 1)];
        }
    }
}