using SpectrumAnalyzer.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumAnalyzer.SpectrumsDB
{
    //TODO:
    //0. check sql expressions;
    //1. add overloads;
    public class Database
    {
        public SQLiteConnection connection;
        private string dbName;

        public string Name
        {
            get
            {
                return dbName;
            }
        }

        public Database()
        {
            dbName = "LOCAL.db";
            Initialize(dbName);
        }

        public Database(string databaseName)
        {
            dbName = databaseName;
            Initialize(dbName);
        }

        private void Initialize(string dbName)
        {
            connection = new SQLiteConnection(String.Format("Data Source={0}", dbName));
            if (!File.Exists(String.Format("./AppData/{0}", dbName)))
            {
                SQLiteConnection.CreateFile(dbName);
            }
        }

        public void OpenConnection()
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }
        }

        public void CloseConnection()
        {
            if (connection.State != System.Data.ConnectionState.Closed)
            {
                connection.Close();
            }
        }

        public void LoadDataToDB(string table, string title, string data)
        {
            string query = String.Format("INSERT INTO {0} ('TITLE', 'DATA') VALUES (@title, @data)", table);
            SQLiteCommand command = new SQLiteCommand(query, connection);
            this.OpenConnection();
            command.Parameters.AddWithValue("@title", title);
            command.Parameters.AddWithValue("@data", data);
            command.ExecuteNonQuery();
            this.CloseConnection();
        }

        public KeyValuePair<string, string> UploadDataFromDB(string table, string title)
        {
            string query = String.Format("SELECT DATA FROM {0} WHERE TITLE == {1}", table, title);
            SQLiteCommand command = new SQLiteCommand(query, connection);
            this.OpenConnection();
            SQLiteDataReader result = command.ExecuteReader();
            this.CloseConnection();
            return new KeyValuePair<string, string>(result["DATA"].ToString(), title);
        }
    }
}
