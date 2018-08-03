using SQLite.Net;

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
    }
}
