using System;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace SpectrumAnalyzer.Models
{
    public class Plotter
    {
        public bool Initialized
        {
            get
            {
                if (PlotFrame != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
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
                PlotFrame.Series.Clear();
            }
        }

        private void Initialize()
        {
            PlotFrame = new PlotModel();
            PlotFrame.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
            PlotFrame.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Maximum = 10, Minimum = 0 });
        }

        public void Plot(Spectrum spectrum, PlotMethod plotMethod, Action<object, OxyMouseDownEventArgs> onClickCallback)
        {
            if (plotMethod == PlotMethod.Replace)
            {
                this.Clear();
            }

            // TODO: Spectrum checkouts

            PlotFrame.Title = spectrum.FileName;

            var series1 = new LineSeries
            {
                StrokeThickness = 1,
                Color = OxyColors.Red
            };
            if (plotMethod == PlotMethod.Combine)
            {
                series1.Color = OxyColor.FromRgb(Convert.ToByte(rnd.Next(1, 200)), Convert.ToByte(rnd.Next(1, 200)), Convert.ToByte(rnd.Next(1, 200)));
            }
            for (int i = 0; i < spectrum.Bins.Count; i++)
            {
                series1.Points.Add(new DataPoint(spectrum.Bins[i].X, spectrum.Bins[i].Y));
            }

            if (onClickCallback != null)
            {
                series1.MouseDown += (s, e) => { onClickCallback(s, e); };
            }
            series1.TrackerKey = spectrum.Name.ToLower();
            series1.Title = spectrum.Name;

            RecountPlotAxes(spectrum);
            PlotFrame.Series.Add(series1);
            PlotFrame.InvalidatePlot(true);
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
            var minX = spectrum.Bins.Min(x => x.X);
            var maxX = spectrum.Bins.Max(x => x.X);
            var minY = spectrum.Bins.Min(x => x.Y);
            var maxY = spectrum.Bins.Max(x => x.Y);

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
                AbsoluteMaximum = maxY,
                Minimum = minY,
                Maximum = maxY,
                Title = "Интенсивность, о.е.",
                AxisTitleDistance = 10
            });
        }
    }
}

