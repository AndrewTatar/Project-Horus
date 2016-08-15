using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Horus_Config
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static bool INSTALLATION = false;
        public static string EMAIL_ADDRESS = "";
        public static string ANDROID_APK = "https://drive.google.com/file/d/0B-hJtziIY0dlYk43WDdqY3NxZ1U/view?usp=sharing";

        public static void WriteMessage(string message)
        {
            Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + " - " + message);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Count() > 0)
            {
                foreach(var a in e.Args)
                {
                    if (a.IndexOf(":") == -1)
                    {
                        //Flag
                        switch (a.ToLower())
                        {
                            case "-install":
                                INSTALLATION = true;
                                break;
                        }
                    }
                    else
                    {
                        //Value
                        string header = a.Substring(0, a.IndexOf(":"));

                        switch (header.ToLower())
                        {
                            case "email":
                                EMAIL_ADDRESS = a.Substring(a.IndexOf(":") + 1);
                                break;
                        }
                    }
                }
            }
        }
    }
}
