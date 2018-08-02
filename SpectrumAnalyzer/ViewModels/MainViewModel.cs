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

        public ListBoxFileItem SelectedFile { get; set; }

        public void Files_SelectionChanged(SelectionChangedEventArgs args)
        {
            Transitions.Clear();
            Plotter.Clear();

            if (args.AddedItems.Count == 0 || args.AddedItems.Cast<object>().Where(x => x is ListBoxFileItem).Count() != 1)
            {
                return;
            }

            SelectedFile = args.AddedItems.Cast<ListBoxFileItem>().FirstOrDefault();
            string contents = ReadFromFile(SelectedFile.Path);

            if (!string.IsNullOrEmpty(contents))
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
            }
        }

        public void TransitionCheckBoxChanged(object sender, RoutedEventArgs args)
        {
            CheckBox checkBox = sender as CheckBox;
            var transitionName = checkBox.Content.ToString();
            if (checkBox.IsChecked == true)
            {
                var existingSeries = Plotter.GetExistingSeries(transitionName.ToLower());
                if (existingSeries == null)
                {
                    Plotter.Plot(Transitions.First(t => t.Name == transitionName), null);
                }
            }
            else if (checkBox.IsChecked == false)
            {
                var existingSeries = Plotter.GetExistingSeries(transitionName.ToLower());
                if (existingSeries != null)
                {
                    Plotter.RemoveSeries(transitionName);
                }
            }
        }

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

        public void AddToDatabase(object parameter)
        {
            var title = Plotter.PlotFrame.Title;
            var series = Plotter.PlotFrame.Series[0] as LineSeries;
            string data = string.Empty;
            foreach (DataPoint point in series.Points)
            {
                data += string.Format("({0};{1})", point.X, point.Y);
            }
            Database.Put(new SpectrumBase() { Peaks = data, Name = title });
        }

        public void OpenDatabaseView()
        {
            IWindowManager manager = new WindowManager();
            manager.ShowWindow(new DatabaseViewModel(), null, null);
        }
    }
}
