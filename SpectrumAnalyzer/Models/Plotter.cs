using System;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Windows;

namespace SpectrumAnalyzer.Models
{
    public class Plotter
    {
        public bool Initialized
        {
            get
            {
                return PlotFrame == null ? false : true;
            }
        }

        public PlotModel PlotFrame { get; private set; }

        public Plotter()
        {
            Initialize();
        }

        public void Clear()
        {
            if (this.Initialized)
            {
                PlotFrame.Title = string.Empty;
                PlotFrame.Series.Clear();
            }
        }

        private void Initialize()
        {
            PlotFrame = new PlotModel();
            PlotFrame.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
            PlotFrame.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Maximum = 10, Minimum = 0 });
        }

        public void Plot(Spectrum spectrum, Action<object, OxyMouseDownEventArgs> onSeriesClicked)
        {
            if (spectrum == null)
            {
                return;
            }

            PlotFrame.Title = spectrum.FileName;

            var series1 = new LineSeries
            {
                StrokeThickness = 1,
                Color = this.GetColor(),
                TrackerKey = spectrum.Name.ToLower(),
                Title = spectrum.Name
            };

            series1.Points.AddRange(spectrum.Bins.Select(x => (DataPoint)x));

            if (onSeriesClicked != null)
            {
                series1.MouseDown += (s, e) => { onSeriesClicked(s, e); };
            }

            PlotFrame.Series.Add(series1);
            RecountPlotAxes(spectrum);
            PlotFrame.InvalidatePlot(true);
        }

        private OxyColor GetColor()
        {
            var colors = new List<OxyColor> {
                OxyColors.Red,
                OxyColors.DarkBlue,
                OxyColors.DarkGreen,
                OxyColors.Orange,
                OxyColors.DarkGray
            };

            return colors[this.PlotFrame.Series.Count(serie => serie.TrackerKey != "peaks")];
        }

        public void RemoveSeries(string key)
        {
            var existingSeries = this.PlotFrame.Series.FirstOrDefault(s => s.TrackerKey == key.ToLower());
            if (!string.IsNullOrEmpty(key) && existingSeries != null)
            {
                PlotFrame.Series.Remove(existingSeries);
            }
            PlotFrame.InvalidatePlot(true);
        }

        internal void MarkPeak(double x, double y)
        {
            if (this.Initialized)
            {
                ScatterSeries peakSeries = InstantinatePeakMarkSeries();
                var existingPoint = peakSeries.Points.FirstOrDefault(p => p.X == x);
                if (existingPoint != null)
                {
                    peakSeries.Points.Remove(existingPoint);
                }
                else
                {
                    peakSeries.Points.Add(new ScatterPoint(x, y));
                }
                PlotFrame.InvalidatePlot(true);
            }
        }

        private ScatterSeries InstantinatePeakMarkSeries()
        {
            Series existingSeries = GetExistingSeries("peaks");
            if (existingSeries != null)
            {
                return existingSeries as ScatterSeries;
            }
            else
            {
                var s = CreatePeakSeries();
                this.PlotFrame.Series.Add(s);
                return GetExistingSeries("peaks") as ScatterSeries;
            }
        }

        private ScatterSeries CreatePeakSeries()
        {
            var customMarker = new List<ScreenPoint>()
            {
                new ScreenPoint(1, -2),
                new ScreenPoint(0.3, -1.3),
                new ScreenPoint(0, -0.5),
                new ScreenPoint(-0.3, -1.3),
                new ScreenPoint(-1, -2)
            };

            ScatterSeries s = new ScatterSeries()
            {
                MarkerType = MarkerType.Custom,
                MarkerOutline = customMarker.ToArray(),
                MarkerFill = OxyColors.DarkRed,
                MarkerSize = 10,
                TrackerKey = "peaks"
            };

            return s;
        }

        public Series GetExistingSeries(string key)
        {
            return this.PlotFrame.Series.FirstOrDefault(s => s.TrackerKey == key);
        }

        private void RecountPlotAxes(Spectrum spectrum)
        {
            double minX = spectrum.Bins.Min(x => x.X);
            double maxX = spectrum.Bins.Max(x => x.X);
            double minY = spectrum.Bins.Min(x => x.Y);
            double maxY = spectrum.Bins.Max(x => x.Y);

            foreach (var series in PlotFrame.Series.OfType<LineSeries>())
            {
                var _minX = series.Points.Min(x => x.X);
                var _maxX = series.Points.Max(x => x.X);
                var _minY = series.Points.Min(x => x.Y);
                var _maxY = series.Points.Max(x => x.Y);
                if (_minX < minX) minX = _minX;
                if (_maxX > maxX) maxX = _maxX;
                if (_minY < minY) minY = _minY;
                if (_maxY > maxY) maxY = _maxY;
            }

            PlotFrame.Axes.Clear();
            PlotFrame.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                AbsoluteMinimum = minX,
                AbsoluteMaximum = maxX,
                Minimum = minX,
                Maximum = maxX,
                Title = (string)Application.Current.Resources["str_plotter_xaxis"],
                AxisTitleDistance = 10
            });
            PlotFrame.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                AbsoluteMinimum = minY,
                AbsoluteMaximum = maxY * 1.25,
                Minimum = minY,
                Maximum = maxY * 1.25,
                Title = (string)Application.Current.Resources["str_plotter_yaxis"],
                AxisTitleDistance = 10
            });
        }
    }
}

