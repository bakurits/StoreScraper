using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EO.WinForm;
using ScraperTest.Controls;

namespace ScraperTest.MinorTests
{
    [TestClass]
    public class EOBrowserTests
    {

        [TestMethod]
        public void MainTest()
        {
            var form = new EOTestForm();
            form.Load += FormOnLoad;
            Application.Run();
        }

        private void FormOnLoad(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
