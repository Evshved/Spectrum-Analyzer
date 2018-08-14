using System;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using SpectrumAnalyzer.ViewModels;
using SpectrumAnalyzer.Helpers;

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

        public enum StageType
        {
            Automatic = 0,
            CanBeManual = 1
        }

        public StageType Selection;

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
                PlotFrame.InvalidatePlot(true);
            }
        }

        private void Initialize()
        {
            PlotFrame = new PlotModel();
            PlotFrame.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
            PlotFrame.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Maximum = 10, Minimum = 0 });
        }

        public void Plot(Spectrum spectrum, Action<object, OxyMouseDownEventArgs> onSeriesClicked, SpectrumType spectrumType)
        {
            LineSeries sourceSeries;
            LineSeries optimizedSeries = BuildLineSeries(spectrum.Optimized, onSeriesClicked);
            var peakSeriesName = string.Empty;

            if (spectrumType == SpectrumType.Analyzed)
            {
                PlotFrame.Title = spectrum.Name;
                sourceSeries = BuildLineSeries(spectrum.Source, null);
                PlotFrame.Series.Add(sourceSeries);
                peakSeriesName = "Peaks";
            }
            else if (spectrumType == SpectrumType.Imported)
            {
                peakSeriesName = "Imported Peaks";
            }

            ScatterSeries peaksSeries = BuildScatterSeries(spectrum.Peaks, peakSeriesName);

            PlotFrame.Series.Add(optimizedSeries);
            PlotFrame.Series.Add(peaksSeries);

            RecountPlotAxes(spectrum.Optimized.Data); // TODO: REcount by all transition types, not only source.

            foreach (var item in spectrum.Peaks.Data)
            {
                this.Selection = Plotter.StageType.Automatic;
                this.MarkPeak(item.X, item.Y, peakSeriesName);
                this.Selection = Plotter.StageType.CanBeManual;
            }

            PlotFrame.InvalidatePlot(true);
        }

        private ScatterSeries BuildScatterSeries(SpectrumTransition peaks, string peakSeriesName)
        {
            ScatterSeries result;
            Series existingSeries = GetExistingSeries(peakSeriesName);
            if (existingSeries != null)
            {
                result = existingSeries as ScatterSeries;
            }
            else
            {
                result = CreatePeakSeries(peakSeriesName); // (string)Application.Current.Resources["str_plotter_PeakSeriesTitle"]
            }

            return result;
        }

        private LineSeries BuildLineSeries(SpectrumTransition transition, Action<object, OxyMouseDownEventArgs> onSeriesClicked)
        {
            var series = new LineSeries()
            {
                StrokeThickness = 1,
                Title = transition.Name
            };

            switch (transition.Name)
            {
                case "Source":
                    {
                        series.Color = OxyColors.Red;
                        series.MarkerType = MarkerType.Circle;
                        series.MarkerSize = 2;
                        series.MarkerFill = OxyColors.Red;
                        series.MarkerResolution = 50;
                        break;
                    }
                case "Optimized":
                    {
                        series.Color = OxyColors.Blue;
                        series.MarkerType = MarkerType.Square;
                        series.MarkerSize = 2;
                        series.MarkerFill = OxyColors.Blue;
                        series.MarkerResolution = 50;
                        if (onSeriesClicked != null)
                        {
                            series.MouseDown += (s, e) => { onSeriesClicked(s, e); };
                        }
                        break;
                    }
                case "Imported":
                    {
                        series.Color = OxyColors.Green;
                        series.MarkerType = MarkerType.Triangle;
                        series.MarkerSize = 3;
                        series.MarkerFill = OxyColors.Green;
                        series.MarkerResolution = 50;
                        break;
                    }
                default:
                    {
                        series.Color = OxyColors.Yellow;
                        break;
                    }
            }

            series.Points.AddRange(transition.Data.Select(bin => (DataPoint)bin));

            return series;
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

        internal void MarkPeak(double x, double y, string peakSeriesName)
        {
            if (this.Initialized)
            {
                ScatterSeries peakSeries = PlotFrame.Series.First(s => s.Title == peakSeriesName) as ScatterSeries;

                if (Selection == StageType.CanBeManual)
                {
                    var threshold = 50;
                    var nearestPoints = GetNearestPoints(peakSeries.Points, x, threshold);

                    if (nearestPoints == null || nearestPoints.Count == 0)
                    {
                        peakSeries.Points.Add(new ScatterPoint(x, y));
                    }
                    else if (nearestPoints.Count == 1)
                    {
                        peakSeries.Points.Remove(nearestPoints.First());
                    }
                    else if (nearestPoints.Count > 1)
                    {
                        ScatterPoint chosenPeak = AskForPeakToRemove(nearestPoints);
                        if (chosenPeak != null)
                        {
                            peakSeries.Points.Remove(chosenPeak);
                        }
                    }
                }
                else
                {
                    peakSeries.Points.Add(new ScatterPoint(x, y));
                }
                PlotFrame.InvalidatePlot(true);
            }
        }

        private ScatterPoint AskForPeakToRemove(List<ScatterPoint> nearestPoints)
        {
            IWindowManager manager = new WindowManager();
            var dialog = new PointToRemoveDialogViewModel(nearestPoints);
            bool? result = manager.ShowDialog(dialog, null, null);

            if (result == true)
            {
                return dialog.SelectedPoint.Point;
            }
            else
            {
                return null;
            }
        }

        private List<ScatterPoint> GetNearestPoints(List<ScatterPoint> points, double basePoint, int windowSize)
        {
            return points.Where(point => point.X > basePoint - windowSize && point.X < basePoint + windowSize).ToList();
        }

        private ScatterSeries CreatePeakSeries(string title)
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
                MarkerFill = title == "Peaks" ? OxyColors.DarkRed : OxyColors.LightBlue, // TODO: убрать этот костыль
                MarkerSize = 10,
                RenderInLegend = false,
                Title = title
            };

            return s;
        }

        public Series GetExistingSeries(string name)
        {
            return this.PlotFrame.Series.FirstOrDefault(s => s.Title == name);
        }

        private void RecountPlotAxes(List<Bin> dataBins)
        {
            double minX = dataBins.Min(x => x.X);
            double maxX = dataBins.Max(x => x.X);
            double minY = dataBins.Min(x => x.Y);
            double maxY = dataBins.Max(x => x.Y);

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

