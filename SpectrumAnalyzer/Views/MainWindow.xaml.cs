using System;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using System.Windows.Controls;
using SpectrumAnalyzer.Helpers;
using static SpectrumAnalyzer.Helpers.Plotter;
using System.Linq;
using SpectrumAnalyzer.SpectrumsDB;
using OxyPlot.Series;
using OxyPlot;

namespace SpectrumAnalyzer.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var a = ListBox_Files_Queue.Items;
        }

        private void Menu_Exit_Click(object sender, RoutedEventArgs e)
        {
            // TODO:
            // SaveData();
            Environment.Exit(0);
        }

        private void Menu_Open_Click(object sender, RoutedEventArgs e)
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
                    ListBox_Files_Queue.Items.Add(new ListBoxFileItem() { fileName = fileName, filePath = path });
                }
            }
        }

        private void Button_AddToDB_Click(object sender, RoutedEventArgs e)
        {
            var database = new Database();
            var title = ((Plotter)DataContext).PlotFrame.Title;
            var series = ((Plotter)DataContext).PlotFrame.Series[0] as LineSeries;
            String data = System.String.Empty;
            foreach(DataPoint point in series.Points)
            {
                data += String.Format("({0};{1})", point.X, point.Y);
            }
            database.Put("Spectrums", title, data);
            MessageBox.Show("Добавлено в БД!");
        }

        private void ListBox_Files_Queue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxFileItem fni = ((sender as ListBox).SelectedItem as ListBoxFileItem);
            string contents = ReadFromFile(fni.filePath);
            if (!string.IsNullOrEmpty(contents))
            {
                var spectrum = new Spectrum(contents, fni.fileName);
                SpectrumProfile profile = new SpectrumProfile(spectrum);
                profile.Transitions.Add("quantize", spectrum.GetQuantized());

                ((Plotter)DataContext).Plot(profile.OriginalData, PlotMethod.Replace);

                profile.SearchHH();
                ((Plotter)DataContext).Plot(profile.Transitions["searchHH"], PlotMethod.Combine);
            }
        }

        private string ReadFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            else
            {
                MessageBox.Show($"File {filePath} is not found.", "Error");
                return string.Empty;
            }
        }

        private void Button_SaveImage_Click(object sender, RoutedEventArgs e)
        {
            var plotModel = DataContext as Plotter;

            SaveFileDialog dialog = new SaveFileDialog
            {
                FileName = plotModel.PlotFrame.Title + ".png",
                Filter = "PNG (*.png)|*.png",
                FilterIndex = 1
            };

            if (dialog.ShowDialog() == true)
            {
                IO.SaveImage(plotModel, dialog.FileName, Dispatcher);
            }
        }
    }
}
