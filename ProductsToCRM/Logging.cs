using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Configuration;

namespace LogManager
{
    class Logging
    {
        public static string fileName;

        public static void WriteToLog(string Message)
        {
            try
            {
                GenerateDefaultLogFileName();
                using (StreamWriter sw = File.AppendText(fileName))
                {
                    sw.WriteLine(String.Format("[{0}]\t{1}", DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss"), Message));
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Logging class error", ex.Message);
            }
        }

        private static void GenerateDefaultLogFileName()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"\logs";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            fileName = String.Format(@"{0}\{1}.log",
                path, DateTime.Now.ToString("yyyy.MM.dd"));
        }

    }
}