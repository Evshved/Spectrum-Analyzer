using System;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Series;
using System.Windows.Media;
using System.Text;
using SpectrumAnalyzer.Helpers;
using static SpectrumAnalyzer.Helpers.Plotter;

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
            Stream myStream = null;
            OpenFileDialog openFileDialogElement = new OpenFileDialog();

            openFileDialogElement.InitialDirectory = Environment.CurrentDirectory;
            openFileDialogElement.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialogElement.FilterIndex = 2;
            openFileDialogElement.RestoreDirectory = true;
            openFileDialogElement.Multiselect = true;

            if (openFileDialogElement.ShowDialog() == true)
            {
                foreach (String filePath in openFileDialogElement.FileNames)
                {
                    try
                    {

                        var fileName = Path.GetFileName(filePath);
                        Console.WriteLine(fileName);
                        Console.WriteLine(filePath);

                        if (Path.GetExtension(fileName) == ".txt")
                        {
                            using (StreamReader reader = new StreamReader(filePath))
                            {
                                // Read for base API
                                String line = reader.ReadToEnd();
                                if (!string.IsNullOrEmpty(line))
                                {
                                    ListBox_Files_Queue.Items.Add(new ListBoxFileItem() { fileName = fileName, filePath = filePath });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Охуенная кнопка!");
        }

        private void Button_AddToDB_Click(object sender, RoutedEventArgs e)
        {
            //List<string> data = PrepareForDB();
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
                profile.Transitions.Add("quantize", spectrum.Quantize());

                ((Plotter)DataContext).Plot(profile.OriginalData, PlotMethod.Replace);
                ((Plotter)DataContext).Plot(profile.Transitions["quantize"], PlotMethod.Combine);
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
    }
}
