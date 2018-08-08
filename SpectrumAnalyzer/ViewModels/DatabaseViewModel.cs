using Caliburn.Micro;
using SpectrumAnalyzer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace SpectrumAnalyzer.ViewModels
{
    public class DatabaseViewModel : Screen
    {
        public BindableCollection<SpectrumBase> Spectrums { get; set; } = new BindableCollection<SpectrumBase>();

        public List<SpectrumBase> SelectedSpectrums { get; set; } = new List<SpectrumBase>();

        public List<SpectrumBase> PreparedForImport { get; set; } = new List<SpectrumBase>();

        public DatabaseViewModel()
        {
            LoadAllData();
        }

        internal void LoadAllData()
        {
            var spectrums = Database.GetConnection().Table<SpectrumBase>().ToList();
            foreach (var spectrum in spectrums)
            {
                Spectrums.Add(spectrum);
            }
        }

        public void DeleteSpectrums()
        {
            // .ToList() conversion is important here, as the code itself will modify 
            // the elements of the original collection (ToList() instatinates a copy of the collection)
            foreach (var selectedSpectrum in SelectedSpectrums.ToList())
            {
                Database.GetConnection().Delete(selectedSpectrum);
                SelectedSpectrums.Remove(selectedSpectrum);
                PreparedForImport.Remove(selectedSpectrum);
                Spectrums.Remove(selectedSpectrum);
            }
        }

        public void ImportToWorkspace()
        {
            PreparedForImport.AddRange(SelectedSpectrums);
            SelectedSpectrums.Clear();
            NotifyOfPropertyChange(() => CanImportToWorkspace);
        }

        public bool CanImportToWorkspace
        {
            get { return SelectedSpectrums.Count > 0 ? true : false; }
        }

        public bool CanDeleteSpectrums
        {
            get { return SelectedSpectrums.Count > 0 ? true : false; }
        }

        public void Spectrums_SelectionChanged(SelectionChangedEventArgs args)
        {
            SelectedSpectrums.AddRange(args.AddedItems.Cast<object>().Where(x => x is SpectrumBase).Cast<SpectrumBase>());
            args.RemovedItems.Cast<object>().Where(x => x is SpectrumBase).Cast<SpectrumBase>().ToList().ForEach(x => SelectedSpectrums.Remove(x));
            NotifyOfPropertyChange(() => CanDeleteSpectrums);
            NotifyOfPropertyChange(() => CanImportToWorkspace);
        }
    }
}
