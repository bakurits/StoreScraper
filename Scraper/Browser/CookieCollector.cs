using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using CheckOutBot.Factory;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Cookie = OpenQA.Selenium.Cookie;

namespace CheckOutBot.Browser
{

    class CookieCollector
    {

        public class CollectionTask
        {
            public Func<IWebDriver, IEnumerable<Cookie>> Func { get; set; }

            /// <summary>
            /// Interval in milliSeconds
            /// </summary>
            public int Interval { get; set; }

            /// <summary>
            /// Cookies after last update
            /// </summary>
            public List<Cookie> Cookies { get; set; }

            public DateTime NextRun { get; set; } = DateTime.Now;
        }

        public static CookieCollector Default { get; set; }


        private IWebDriver driver = ClientFactory.GetChromeDriver();
        private Dictionary<string, CollectionTask> _registeredTasks = new Dictionary<string, CollectionTask>();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public const int MonitorInterval = 5000;
        private bool _diposed = false;


        public CookieCollector()
        {
            Monitor();
        }

        private void Monitor()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Task.Run(() =>
                    {
                        foreach (var task in _registeredTasks)
                        {
                            if (task.Value.NextRun < DateTime.Now)
                            {
                                try
                                {
                                    var exeTask = Task.Run(() => task.Value.Func.Invoke(driver));
                                    exeTask.Wait(_cancellationTokenSource.Token);
                                    task.Value.Cookies = new List<Cookie>();
                                    foreach (var cookie in exeTask.Result)
                                    {
                                        task.Value.Cookies.Add(cookie);
                                    }
                                }
                                catch
                                {
                                    //ignored
                                }
                               
                            }

                            task.Value.NextRun += TimeSpan.FromMilliseconds(MonitorInterval);
                        }
                    }).Wait(_cancellationTokenSource.Token);

                    Task.Delay(5000, _cancellationTokenSource.Token);
                }
            });
           
        }

        public void RegisterAction(string uniqueName, Func<IWebDriver, IEnumerable<Cookie>> collectFunc, int repeatInMilliSeconds)
        {
            if (_registeredTasks.ContainsKey(uniqueName))
                throw new Exception("cookie collection action with this name already exist!");

            _registeredTasks.Add(uniqueName, new CollectionTask()
           {
               Func = collectFunc,
               Interval = repeatInMilliSeconds
           });
        }

        public void RemoveAction(string uniqueName)
        {
            if(!_registeredTasks.Remove(uniqueName)) throw new NoSuchElementException("Actions with given name is not registered");
        }

        public IEnumerable<Cookie> GetCookies(string uniqueName, CancellationToken token)
        {
            while (_registeredTasks[uniqueName].Cookies == null)
            {
               Task.Delay(100).Wait(token);
            }
            return _registeredTasks[uniqueName].Cookies;
        }

        public void Dispose()
        {   
            if(_diposed) return;
            _diposed = true;
            this._cancellationTokenSource.Cancel();
            this.driver.Close();
            this.driver.Quit();
        }
    }
}
