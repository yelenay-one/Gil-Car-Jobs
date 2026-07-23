using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogManager
{
    class Logging_General
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
            string path = "d:\\Main_Interface_LOG\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            fileName = String.Format(path +"MainLog.log");
        }

    }
}
