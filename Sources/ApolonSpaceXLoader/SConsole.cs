namespace ApolonSpaceXLoader
{
    using System;
    using System.Diagnostics;
    using System.IO;

    class SConsole
    {
        private static readonly string LogFilePath = Path.Combine(Path.GetTempPath(), "");

        public static bool CVoid(string Command)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {PathReplacer(Command)}", // /C закрывает cmd после выполнения
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit(5000); // Ожидание до 5 секунд

                    if (process.HasExited)
                    {
                        if (!string.IsNullOrEmpty(output))
                            if (!string.IsNullOrEmpty(error))
                                return false;
                        return process.ExitCode == 0;
                    }
                    else
                    {
                        process.Kill();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static string PathReplacer(string Str)
        {
            try
            {
                string result = string.Empty;
                switch (Str)
                {
                    case "{AppData}":
                        result = Str.Replace("{AppData}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                        break;
                    case "{UserProfile}":
                        result = Str.Replace("{UserProfile}", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                        break;
                    case "{Documents}":
                        result = Str.Replace("{Documents}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                        break;
                    case "{ProgramFiles}":
                        result = Str.Replace("{ProgramFiles}", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
                        break;
                    case "{Startup}":
                        result = Str.Replace("{Startup}", Environment.GetFolderPath(Environment.SpecialFolder.Startup));
                        break;
                    default:
                        result = Str;
                        break;
                }
                return result;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}