using SQLite.Net;
using System;
using System.IO;

namespace SpectrumAnalyzer.Models
{
    public static class Database
    {
        private static string databaseFileName = @"AppData\LOCAL.db";

        private static SQLiteConnection localDbConnection;

        public static SQLiteConnection GetConnection()
        {
            if (localDbConnection == null)
            {
                if (!File.Exists(databaseFileName))
                {
                    databaseFileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Spectrum Analyzer\LOCAL.db";
                }
                localDbConnection = new SQLiteConnection(new SQLite.Net.Platform.Win32.SQLitePlatformWin32(), databaseFileName);
            }
            return localDbConnection;
        }
    }
}
