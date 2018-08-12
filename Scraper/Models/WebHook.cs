using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using StoreScraper.Core;
using StoreScraper.Helpers;

namespace StoreScraper.Models
{
    public class WebHook
    {

        private static readonly Uri _slackHookHost = new Uri("https://hooks.slack.com");
        private static readonly Uri _discordHookHost = new Uri("https://discordapp.com");
        private static readonly SlackPoster _slackPoster = new SlackPoster();
        private static readonly DiscordPoster _discordPoster = new DiscordPoster();

        [Browsable(false)]
        [XmlIgnore]
        public IWebHookPoster Poster { get; set; }

        private string _webHookUrl = "";
        public string WebHookUrl { get => _webHookUrl;
            set
            {
                Uri parsed = new Uri(value);    
                if (Uri.Compare(parsed, _slackHookHost, UriComponents.Host, UriFormat.Unescaped, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    Poster = _slackPoster;
                    _webHookUrl = value;
                }
                else if(Uri.Compare(parsed, _discordHookHost, UriComponents.Host, UriFormat.Unescaped, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    Poster = _discordPoster;
                    _webHookUrl = value;
                }
                else
                {
                    throw new NotSupportedException("This kinf of webhook is not supported");
                }
            }
        }


        public override string ToString()
        {
            return this._webHookUrl;
        }
    }
}
