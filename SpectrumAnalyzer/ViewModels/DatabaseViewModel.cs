using Caliburn.Micro;
using SpectrumAnalyzer.Helpers;
using SpectrumAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SpectrumAnalyzer.ViewModels
{
    public class DatabaseViewModel : Screen
    {
        public BindableCollection<Spectrums> Spectrums { get; set; } = new BindableCollection<Spectrums>();
        public List<Spectrums> SelectedSpectrums { get; set; } = new List<Spectrums>();

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

        public void DeleteSpectrums()
        {
            var spectrumsToRemove = SelectedSpectrums;
            foreach (var spectrum in spectrumsToRemove)
            {
                Database.GetConnection().Delete<Spectrum>(spectrum.ID);
                SelectedSpectrums.Remove(spectrum);
                Spectrums.Remove(spectrum);
            }
        }

        public bool CanDeleteSpectrums
        {
            get { return SelectedSpectrums.Count > 0 ? true : false; }
        }

        public void Spectrums_SelectionChanged(SelectionChangedEventArgs args)
        {
            SelectedSpectrums.AddRange(args.AddedItems.Cast<object>().Where(x => x is Spectrums).Cast<Spectrums>());
            args.RemovedItems.Cast<object>().Where(x => x is Spectrums).Cast<Spectrums>().ToList().ForEach(x => SelectedSpectrums.Remove(x));
            NotifyOfPropertyChange(() => CanDeleteSpectrums);
        }
    }
}
