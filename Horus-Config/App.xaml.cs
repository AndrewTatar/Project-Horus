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

        public static void WriteMessage(string message)
        {
            Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + " - " + message);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string email = "";

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
                                email = a.Substring(a.IndexOf(":") + 1);
                                break;
                        }
                    }
                }
            }

            if (INSTALLATION)
            {
                //Initial Application Install Configuration

                //Email User Link to Android APK
                if (email != "")
                    Emailing.EmailNewUser(email);
            }
        }
    }
}
