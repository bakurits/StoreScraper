using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;

#pragma warning disable 4014

namespace StoreScraper.Http.CookieCollecting
{

    public class CookieCollector
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
            /// Next time to run cookie collection task. set dinamically after each collection
            /// </summary>
            public DateTime NextRun { get; set; }
        }

        public static CookieCollector Default { get; set; }


        private List<HttpClient> _proxiedClients;
        private HttpClient _proxylessClient;
        private List<CollectionTask> _registeredTasks = new List<CollectionTask>();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public const int MonitorInterval = 5000;
        private bool _diposed;
        private Random _rand = new Random();

        public CookieCollector()
        {
            _proxiedClients = new List<HttpClient>();

            if (AppSettings.Default.UseProxy && AppSettings.Default.Proxies.Count > 0)
            {
                foreach (var proxy in AppSettings.Default.Proxies)
                {
                    var webProxy = ClientFactory.ParseProxy(proxy);
                    var client = ClientFactory.CreateProxiedHttpClient(webProxy, true).AddHeaders(ClientFactory.DefaultHeaders);
                    _proxiedClients.Add(client);
                }
            }


            _proxylessClient = ClientFactory.CreateHttpClient(null, true).AddHeaders(ClientFactory.DefaultHeaders);

             Task.Factory.StartNew((Action)Monitor, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void Monitor()
        {
            while (true)
            {
                MonitorEpoch();
            }
        }


        private void MonitorEpoch()
        {
            foreach (var task in _registeredTasks)
            {
                if (DateTime.Now <= task.NextRun) continue;
                CompleteTaskAsync(task);
            }
        }

        private async Task CompleteTaskAsync(CollectionTask task)
        {
            if (AppSettings.Default.UseProxy && AppSettings.Default.Proxies.Count > 0)
            {
                var newTask = Task.Run
                (() =>
                    {
                        _proxiedClients.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).ForAll(
                            client =>
                            {
                                for (var i = 0; i < 5; i++)
                                    try
                                    {
                                        task.Func.Invoke(client, _cancellationTokenSource.Token);
                                        break;
                                    }
                                    catch
                                    {
                                        // ignored
                                    }
                            });
                    }
                );

                await newTask;
            }
            else
            {
                await Task.Run(() => task.Func.Invoke(_proxylessClient, CancellationToken.None));
            }
        }

        /// <summary>
        /// RegistersAction which runs between interval. Also wait for first run asynchonously
        /// </summary>
        /// <param name="uniqueName"> unique Name of action. Used for later removing</param>
        /// <param name="url">Url to collect cookies from</param>
        /// <param name="repeatInterval">Registered action repeat interval</param>
        /// <returns></returns>
        public async Task RegisterActionAsync(string uniqueName, Uri url, TimeSpan repeatInterval)
        {
            void Action(HttpClient client, CancellationToken token) => client.GetAsync(url).Wait();
            await RegisterActionAsync(uniqueName, (Action<HttpClient, CancellationToken>) Action, repeatInterval);
        }

        public async Task RegisterActionAsync(string uniqueName, Action<HttpClient, CancellationToken> collectFunc, TimeSpan repeatInterval)
        {

            var task = new CollectionTask()
            {
                Name = uniqueName,
                Func = collectFunc,
                Interval = repeatInterval,
                NextRun = DateTime.Now + repeatInterval
            };
            _registeredTasks.Add(task);

            await CompleteTaskAsync(task);
        }

        public HttpClient GetClient() =>
                AppSettings.Default.UseProxy && _proxiedClients.Count > 0 ?
                _proxiedClients[_rand.Next(_proxiedClients.Count - 1)] :
                _proxylessClient;


        public void RemoveAction(string uniqueName)
        {
            var findIndex = _registeredTasks.FindIndex(task => task.Name == uniqueName);
            if (findIndex == -1) throw new Exception("FinalActions with given name is not registered");
            _registeredTasks.RemoveAt(findIndex);
        }


        public void Dispose()
        {
            if (_diposed) return;
            _diposed = true;
            this._cancellationTokenSource.Cancel();
            _proxiedClients.ForEach(client => client.Dispose());
        }
    }
}
