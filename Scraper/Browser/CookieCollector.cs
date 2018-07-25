using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using StoreScraper.Factory;
using StoreScraper.Helpers;
using Cookie = System.Net.Cookie;

namespace StoreScraper.Browser
{

    class CookieCollector
    {

        public class CollectionTask
        {
            /// <summary>
            /// Unique name of Collection task.
            /// Used for removing registered task
            /// </summary>
            public string Name { get; set; }

            public Action<HttpClient, CancellationToken> Func { get; set; }

            /// <summary>
            /// Interval to repeat cookie collection task
            /// </summary>
            public TimeSpan Interval { get; set; }

            /// <summary>
            /// Cookies after last update
            /// </summary>
            public List<Cookie> Cookies { get; set; }

            /// <summary>
            /// Next time to run cookie collection task. set dinamically after each collection
            /// </summary>
            public DateTime NextRun { get; set; }
        }

        public static CookieCollector Default { get; set; }


        private List<HttpClient> _clients;
        private List<CollectionTask> _registeredTasks = new List<CollectionTask>();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public const int MonitorInterval = 5000;
        private bool _diposed = false;
        private Random rand = new Random();

        public CookieCollector()
        {
            _clients = new List<HttpClient>();

            foreach (string proxy in AppSettings.Default.Proxies)
            {
                var webProxy = ClientFactory.ParseProxy(proxy);
                HttpClient client = ClientFactory.GetHttpClient(webProxy, true).AddHeaders(ClientFactory.FireFoxHeaders);
                _clients.Add(client);
            }
            Monitor();
        }

        private void Monitor()
        {
            
        }


        public void MonitorEpoch()
        {
            foreach (var task in _registeredTasks)
            {
                if (DateTime.Now <= task.NextRun) continue;
                foreach (var httpClient in _clients)
                {                    
                    Task.Run(() => task.Func.Invoke(httpClient, CancellationToken.None));
                }
            }
        }

        public void RegisterAction(string uniqueName, Action<HttpClient, CancellationToken> collectFunc, TimeSpan repeatInMilliSeconds)
        {
           
            _registeredTasks.Add(new CollectionTask()
           {
               Name = uniqueName,
               Func = collectFunc,
               Interval = repeatInMilliSeconds,
               NextRun = DateTime.Now
           });
        }

        public HttpClient GetClient()
        {
            return _clients[rand.Next(_clients.Count - 1)];
        }

        public void RemoveAction(string uniqueName)
        {
            var findIndex = _registeredTasks.FindIndex(task => task.Name == uniqueName);
            if(findIndex == -1) throw new NoSuchElementException("Actions with given name is not registered");
            _registeredTasks.RemoveAt(findIndex);
        }
    

        public void Dispose()
        {   
            if(_diposed) return;
            _diposed = true;
            this._cancellationTokenSource.Cancel();
            _clients.ForEach(client => client.Dispose());
        }
    }
}
