using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EMV_Card_Browser
{
    public class Logger
    {
        private readonly string logPath;

        //public Logger()
        //{
        //    string exePath = Assembly.GetExecutingAssembly().Location;
        //    string appRoot = Path.GetDirectoryName(exePath);

        //    // If you want to place logs in a sub-directory:
        //    string logDirectory = Path.Combine(appRoot, "EMV_CB_log");
        //    Directory.CreateDirectory(logDirectory);  // Ensure the directory exists

        //    logPath = Path.Combine(logDirectory, "emvcard_log.txt");
        //}
        public Logger()
        {
            // Getting the path to the Public Documents folder
            string publicDocsPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);

            // If you want to place logs in a sub-directory:
            string logDirectory = Path.Combine(publicDocsPath, "EMV_CB_log");
            Directory.CreateDirectory(logDirectory);  // Ensure the directory exists

            logPath = Path.Combine(logDirectory, "emvcard_log.txt");
        }

        public void WriteLog(string message)
        {
            try
            {
                File.AppendAllText(logPath, $"{DateTime.Now}: {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during writing
                // For now, let's just print them out.
                Console.WriteLine($"Failed to write log: {ex.Message}");
            }
        }
    }
}