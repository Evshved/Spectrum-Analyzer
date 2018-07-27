using System;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;

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
        private Random rnd = new Random();
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

            // TODO: Spectrum checkouts

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
            const int NumberOfAngles = 4;
            var customMarkerOutline = new ScreenPoint[NumberOfAngles];
            for (int i = 0; i < NumberOfAngles; i++)
            {
                double th = Math.PI * (2.0 * i / (NumberOfAngles - 1) - 0.5);
                const double R = 1;
                customMarkerOutline[i] = new ScreenPoint(Math.Cos(th) * R, Math.Sin(th) * R);
            }

            ScatterSeries s = new ScatterSeries()
            {
                MarkerType = MarkerType.Custom,
                MarkerOutline = customMarkerOutline,
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

            for (int i = 0; i < PlotFrame.Series.Count; i++)
            {
                var serie = this.PlotFrame.Series[i] as LineSeries;
                var _minX = serie.Points.Min(x => x.X);
                var _maxX = serie.Points.Max(x => x.X);
                var _minY = serie.Points.Min(x => x.Y);
                var _maxY = serie.Points.Max(x => x.Y);
                if (_minX < minX)
                    minX = _minX;
                if (_maxX > maxX)
                    maxX = _maxX;
                if (_minY < minY)
                    minY = _minY;
                if (_maxY > maxY)
                    maxY = _maxY;
            }

            PlotFrame.Axes.Clear();
            PlotFrame.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                AbsoluteMinimum = minX,
                AbsoluteMaximum = maxX,
                Minimum = minX,
                Maximum = maxX,
                Title = "Δν, cm⁻¹",
                AxisTitleDistance = 10
            });
            PlotFrame.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                AbsoluteMinimum = minY,
                AbsoluteMaximum = maxY * 1.25,
                Minimum = minY,
                Maximum = maxY * 1.25,
                Title = "Интенсивность, о.е.",
                AxisTitleDistance = 10
            });
        }
    }
}

