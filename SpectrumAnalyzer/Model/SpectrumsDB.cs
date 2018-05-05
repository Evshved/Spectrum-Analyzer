using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumAnalyzer.SpectrumsDB
{
    public class Spectrum : INotifyPropertyChanged
    {
        private int id;
        private string title;
        private string data;

        public int ID
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged("id");
            }
        }

        public string TITLE
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged("title");
            }
        }
        public string DATA
        {
            get { return data; }
            set
            {
                data = value;
                OnPropertyChanged("Data");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }

    public class ApplicationContext : DbContext
    {
        public ApplicationContext() : base("DefaultConnection")
        {
        }
        public DbSet<Spectrum> Spectrums { get; set; }
    }
}
