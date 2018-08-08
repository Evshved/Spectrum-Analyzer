using Caliburn.Micro;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Series;
using SpectrumAnalyzer.Helpers;
using SpectrumAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SpectrumAnalyzer.ViewModels
{
    public class MainViewModel : Screen
    {
        public Plotter Plotter { get; set; } = new Plotter();

        public BindableCollection<ListBoxFileItem> Files { get; set; } = new BindableCollection<ListBoxFileItem>();

        public BindableCollection<Spectrum> Transitions { get; set; } = new BindableCollection<Spectrum>();

        public BindableCollection<SpectrumBase> ImportedFromDatabase { get; set; } = new BindableCollection<SpectrumBase>();

        public ListBoxFileItem SelectedFile { get; set; }

        public void Files_SelectionChanged(SelectionChangedEventArgs args)
        {
            Transitions.Clear();
            Plotter.Clear();

            SelectedFile = args.AddedItems.Cast<object>().Where(x => x is ListBoxFileItem).Count() == 1
                ? args.AddedItems.Cast<ListBoxFileItem>().First()
                : null;

            NotifyOfPropertyChange(() => CanDetectPeaks);
            NotifyOfPropertyChange(() => CanAddToDatabase);
        }

        #region Triggers
        public bool CanAddToDatabase
        {
            get
            {
                return Transitions.Count > 1 ? true : false;
            }
        }

        public bool CanSaveImage
        {
            get
            {
                return Transitions.Count > 1 ? true : false;
            }
        }

        public bool CanSaveSpectrum
        {
            get
            {
                return Transitions.Any(x => x.Name == "Searched") ? true : false;
            }
        }

        public bool CanDetectPeaks
        {
            get
            {
                return SelectedFile == null ? false : true;
            }
        }

        public bool CanClearImportedSpectrums
        {
            get
            {
                return ImportedFromDatabase.Count > 0 ? true : false;
            }
        }
        #endregion

        #region Actions
        public void ImportFiles(object parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                Multiselect = true
            };


            if (dialog.ShowDialog() == true)
            {
                if (dialog.FileNames.Any())
                {
                    AddToQueue(dialog.FileNames);
                }
            }
        }

        public void SaveSpectrum()
        {
            SaveSearchedSpectrum();
        }

        public void ClearImportedSpectrums()
        {
            ImportedFromDatabase.Clear();
            NotifyOfPropertyChange(() => CanClearImportedSpectrums);
        }

        public void DetectPeaks()
        {
            Transitions.Clear();
            Plotter.Clear();

            string contents = ReadFromFile(SelectedFile.Path);

            if (!string.IsNullOrEmpty(contents))
            {
                ProcessFile(contents);
            }

            NotifyOfPropertyChange(() => CanAddToDatabase);
            NotifyOfPropertyChange(() => CanSaveImage);
            NotifyOfPropertyChange(() => CanSaveSpectrum);
        }

        public void AddToDatabase(object parameter)
        {
            IWindowManager manager = new WindowManager();
            var vm = new AddToDatabaseDialogViewModel(Plotter.PlotFrame.Title);
            var result = manager.ShowDialog(vm, null, null);
            if (result == true)
            {
                SpectrumBase spectrumBase = new SpectrumBase()
                {
                    Name = vm.SpectrumName,
                    Data = string.Join(";", Transitions.First(x => x.Name == "Searched").Bins.Select(x => x.X + ":" + x.Y).ToArray()),
                    Peaks = string.Join(";", (Plotter.PlotFrame.Series.First(x => x.TrackerKey == "peaks") as ScatterSeries).Points.Select(x => x.X + ":" + x.Y).ToArray())
                };

                Database.GetConnection().Insert(spectrumBase);
            }
        }

        public void OpenDatabaseView()
        {
            IWindowManager manager = new WindowManager();
            var vm = new DatabaseViewModel();
            manager.ShowDialog(vm, null, null);
            if (vm.PreparedForImport != null && vm.PreparedForImport.Count > 0)
            {
                ImportedFromDatabase.AddRange(vm.PreparedForImport);
            }
            NotifyOfPropertyChange(() => CanClearImportedSpectrums);
        }

        public void SaveImage(object parameter)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                FileName = Plotter.PlotFrame.Title + ".png",
                Filter = "PNG (*.png)|*.png",
                FilterIndex = 1
            };

            if (dialog.ShowDialog() == true)
            {
                IO.SaveImage(Plotter, dialog.FileName, Dispatcher.CurrentDispatcher);
            }
        }
        #endregion

        private void SaveSearchedSpectrum()
        {
            var spectrum = Transitions.First(x => x.Name == "Searched");
            var contents = "X,Y" + Environment.NewLine;
            contents += string.Join(Environment.NewLine, spectrum.Bins.Select(x => x.X + "," + x.Y).ToArray());

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = spectrum.FileName;
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text file (*.txt)|*.txt|Comma Separated (*.csv)|*.csv";

            if (dlg.ShowDialog() == true)
            {
                IO.SaveTextFile(dlg.FileName, contents, Dispatcher.CurrentDispatcher);
            }
        }

        public void ProcessFile(string contents)
        {
            var originalSpectrum = new Spectrum(contents, "Original");
            originalSpectrum.FileName = SelectedFile.Name;
            Plotter.Plot(originalSpectrum, null);
            Transitions.Add(originalSpectrum);
            // var quantized = originalSpectrum.GetQuantized();
            // Transitions.Add(quantized);
            var searched = originalSpectrum.GetSearched();
            Transitions.Add(searched);
            Plotter.Plot(Transitions.FirstOrDefault(t => t.Name == "Searched"), OnSeriesClicked);
            foreach (var item in searched.PeakX)
            {
                Plotter.Selection = Plotter.StageType.Automatic;
                Plotter.MarkPeak(item.X, item.Y);
                Plotter.Selection = Plotter.StageType.CanBeManual;
            }
            Transitions.Add(new Spectrum(new List<Bin>(), "Peaks")); // Empty Spectrum : the main reason of this is to display a 'Peaks' checkbox in Transitions list
            NotifyOfPropertyChange(() => Transitions);
        }

        public void TransitionCheckBoxChanged(object sender, RoutedEventArgs args)
        {
            CheckBox checkBox = sender as CheckBox;
            var seriesName = checkBox.Content.ToString();
            Plotter.GetExistingSeries(seriesName.ToLower()).IsVisible = checkBox.IsChecked ?? false;
            Plotter.PlotFrame.InvalidatePlot(false);
        }

        private void AddToQueue(string[] fileNames)
        {
            foreach (var path in fileNames)
            {
                var fileName = Path.GetFileName(path);
                var extension = Path.GetExtension(fileName);
                if (extension == ".txt")
                {
                    var file = new ListBoxFileItem(fileName, path);
                    Files.Add(file);
                }
            }
        }

        private void OnSeriesClicked(object s, OxyMouseDownEventArgs e)
        {
            var series = s as LineSeries;
            var x = series.InverseTransform(e.Position).X;
            var y = series.InverseTransform(e.Position).Y;
            Plotter.MarkPeak(x, y);
        }

        private string ReadFromFile(string filePath)
        {
            string result = string.Empty;
            if (File.Exists(filePath))
            {
                result = File.ReadAllText(filePath);
            }
            return result;
        }
    }
}
