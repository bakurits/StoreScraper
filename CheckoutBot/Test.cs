using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Anticaptcha;
using System.Threading;

namespace TestConsole
{
    public class Test
    {
        /// <summary>
        /// Automatic CAPTCHA recognition example
        /// </summary>
        /// <param name="key">service key. You can get it free here - http://pixodrom.com</param>
        public static void Recognize(string key)
        {
            anticaptcha ac = new anticaptcha(key);

            Console.WriteLine("getting the CAPTCHA...");

            string res = "";
            res = ac.GetBalance();
            Console.WriteLine("Balance: {0}", res);

            res = ac.UploadURL("http://www.russianpost.ru/tracking20/Code/Code.png.ashx");
            Console.WriteLine("{0}", res);

            if (res.StartsWith("OK|"))
            {
                string sid = res.Split(new char[] { '|' })[1];
                int id = Int32.Parse(sid);
                int tries = 0;

                res = anticaptcha.NOT_READY;
                Console.WriteLine("waiting for answer...");
                while (tries < 30 && (res == anticaptcha.NOT_READY))
                {
                    res = ac.GetResult(id);
                    if (res != anticaptcha.NOT_READY)
                        Console.WriteLine("(tries={2}) {0} : {1}", id, res, tries);
                    Thread.Sleep(1000);
                    tries++;
                }
            }

            Console.ReadLine();
        }
    }
}
