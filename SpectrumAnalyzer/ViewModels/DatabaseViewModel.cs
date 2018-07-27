using Caliburn.Micro;
using SpectrumAnalyzer.Helpers;
using SpectrumAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumAnalyzer.ViewModels
{
    public class DatabaseViewModel : Screen
    {
        public BindableCollection<Spectrums> Spectrums { get; set; } = new BindableCollection<Spectrums>();

        public DatabaseViewModel()
        {
            LoadAllData();
        }

        internal void LoadAllData()
        {
            var spectrums = Database.GetConnection().Table<Spectrums>().ToList();
            foreach (var spectrum in spectrums)
            {
                Spectrums.Add(spectrum);
            }
        }
    }
}
