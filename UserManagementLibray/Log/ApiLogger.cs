using System;
using System.IO;

namespace UserManagementlibrary.Log
{
    public static class ApiLogger
    {
        private static readonly string BaseLogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        public static void Log(string apiName, string message)
        {
            try
            {
                string dateFolder = Path.Combine(BaseLogFolder, DateTime.Now.ToString("yyyy-MM-dd"));
                if (!Directory.Exists(dateFolder))
                {
                    Directory.CreateDirectory(dateFolder);
                }

                string logFilePath = Path.Combine(dateFolder, "ApiCallLog.txt");
                string logEntry = $"{DateTime.Now:HH:mm:ss} | API: {apiName}";
                if (!string.IsNullOrEmpty(message))
                {
                    logEntry += $" | Message: {message}";
                }

                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine(logEntry);
                }
            }
            catch
            {
            }
        }
    }
}
