using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace SpectrumAnalyzer.Models
{
    //rewrite using Dict<string, string> everywhere
    //add SQL UPDATE feature
    public class DBConnection
    {
        private LocalDbConnection localDbConnection;
        
        public DBConnection()
        {
            //need this weird path for testing
            localDbConnection = new LocalDbConnection();
        }

        public void Put(Spectrums spec)
        {
            localDbConnection.Spectrums.Add(spec);
            localDbConnection.SaveChanges();
        }
        
        public Spectrums Get(string title)
        {
            var spec = localDbConnection.Spectrums.Find(1);
            
            return spec;
        }

        public void Delete(string table, string title)
        {
        }
    }
}
