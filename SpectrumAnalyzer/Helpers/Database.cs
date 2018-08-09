using SpectrumAnalyzer.Helpers;
using SQLite.Net;
using System;
using System.Collections.Generic;
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

        internal static List<Bin> ParseSpectrum(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException("Peak data can't be null.");
            }
            var s = data.Split(';');
            var result = new List<Bin>();
            foreach (var x in s)
            {
                var arr = x.Split(':');
                result.Add(new Bin(arr[0], arr[1]));
            }
            return result;
        }
    }
}
