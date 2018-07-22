using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper;

namespace ScraperTest
{
    [TestClass]
    public class ChampsSportsTest
    {
        [TestMethod]
        public void test1()
        {
            
        }


        [TestInitialize]
        public void Init()
        {
            AppSettings.Init();
            if (!Directory.Exists(AppSettings.DataDir)) Directory.CreateDirectory(AppSettings.DataDir);
            AppSettings.Default = AppSettings.Load();
        }
    }
}