using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace UserManagementOnSQLLite.Database
{
    public class DatabaseInitializer
    {
        private static string dbFile = ConfigurationManager.AppSettings["SQLLiteDBFilePath"];
        private static string connectionString = $"Data Source={nameof(dbFile)};Version=3;";
        public static bool Initialize()
        {
            if (!File.Exists(dbFile))
            {
                return false;
            }
            return true;
        }
    }

        
}
