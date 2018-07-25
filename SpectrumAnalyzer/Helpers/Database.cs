using SQLite.Net;
using System;
using System.Collections.Generic;
using System.IO;

namespace SpectrumAnalyzer.Models
{
    public static class Database
    {
        private const string databaseFileName = @"AppData\LOCAL.db";
        private static SQLiteConnection localDbConnection;

        public static SQLiteConnection GetConnection()
        {
            if (localDbConnection == null)
            {
                localDbConnection = new SQLiteConnection(new SQLite.Net.Platform.Win32.SQLitePlatformWin32(), databaseFileName);
            }
            return localDbConnection;
        }

        public static void Put(Spectrums spec)
        {
            GetConnection().Insert(spec);
        }

        public static Spectrums Get(string title)
        {
            var spec = GetConnection().Table<Spectrums>().FirstOrDefault();
            return spec;
        }

        public static void Delete(string table, string title)
        {
        }
    }
}
