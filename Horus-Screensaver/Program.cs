using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace Horus_Screensaver
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                if (args.Length > 0)
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Horus");

                    if (key != null)
                    {
                        string firstarg = args[0].ToString();

                        if (firstarg.Length > 2)
                            firstarg = firstarg.Substring(0, 2);
                        
                        string appPath = (string)key.GetValue("AppPath");

                        if (firstarg == "/c" || firstarg == "/s")
                        {
                            //Config or Full Screen Mode
                            if (appPath != null)
                            {
                                if (firstarg == "/c")
                                {
                                    //Start configuration Tool
                                    Process process = new Process();
                                    process.StartInfo.FileName = Path.Combine(appPath, "Horus-Config.exe");
                                    process.StartInfo.UseShellExecute = true;
                                    process.StartInfo.WorkingDirectory = appPath;
                                    process.Start();
                                }
                                else
                                {
                                    Process process = new Process();
                                    process.StartInfo.FileName = Path.Combine(appPath, "Horus.exe");
                                    process.StartInfo.WorkingDirectory = appPath;
                                    process.StartInfo.UseShellExecute = true;
                                    process.StartInfo.Arguments = firstarg;
                                    process.Start();
                                }

                                //Application.Exit();
                            }
                        }
                        else if (firstarg == "/p")
                        {
                            //Preview Mode - Handle
                            if (args.Count() >= 2)
                            {
                                IntPtr previewWndHandle = new IntPtr(long.Parse(args[1].ToString()));
                                Application.Run(new ScreenSaverPreview(previewWndHandle));
                            }
                        }
                    }
                }
                else
                {
                    //No Arguments
                }
            }
            catch (Exception)
            {
            }            
        }
    }
}
