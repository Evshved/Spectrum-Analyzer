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
    public class DatabaseWindowViewModel : ViewModelBase
    {
        public ObservableCollection<Spectrums> Spectrums { get; set; }

        public DatabaseWindowViewModel()
        {
            Spectrums = new ObservableCollection<Spectrums>();
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
