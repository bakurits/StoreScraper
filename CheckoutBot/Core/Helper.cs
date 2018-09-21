using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Threading;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using CheckoutBot.CheckoutBots.FootSites;
using StoreScraper.Models;
using DColor = System.Drawing.Color;
using MColor = System.Windows.Media.Color;

namespace CheckoutBot.Core
{
    public static class Helper
    {
        public static void AppendText(this RichTextBox box, string text, DColor color)
        {
            TextRange range = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            range.Text = text;
            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(ToMediaColor(color)));
        }


        public static MColor ToMediaColor(this DColor color)
        {
            return MColor.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}