using OxyPlot;
using OxyPlot.Wpf;
using SpectrumAnalyzer.Models;
using System.IO;
using System.Threading;
using System.Windows.Threading;

namespace SpectrumAnalyzer.Helpers
{
    public static class IO
    {
        public static void SaveImage(Plotter plotModel, string fileName, Dispatcher dispatcher)
        {
            var thread = new Thread(() =>
            {
                dispatcher.Invoke(() => _saveImage(plotModel, fileName));
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private static void _saveImage(Plotter plotModel, string fileName)
        {
            PngExporter.Export((plotModel.PlotFrame), fileName, 960, 720, OxyColors.White);
        }

        public static void SaveTextFile(string filename, string contents, Dispatcher dispatcher)
        {
            var thread = new Thread(() =>
            {
                dispatcher.Invoke(() => _saveTextFile(filename, contents));
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private static void _saveTextFile(string filename, string contents)
        {
            File.WriteAllText(filename, contents);
        }
    }
}

