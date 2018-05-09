using System.Linq;
using OxyPlot;
using OxyPlot.Axes;

namespace SpectrumAnalyzer.Helpers
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

        public PlotModel PlotFrame { get; private set; }

        public enum PlotMethod
        {
            Replace = 1,
            Combine = 2
        }

        public Plotter()
        {
            Initialize();
            ShowTestPlot();
        }

        private void ShowTestPlot()
        {
            PlotFrame.Title = "Test Plot";

            var series1 = new OxyPlot.Series.LineSeries
            {
                StrokeThickness = 1,
                Color = OxyColors.Red
            };

            series1.Points.Add(new DataPoint(0, 6));
            series1.Points.Add(new DataPoint(1, 2));
            series1.Points.Add(new DataPoint(2, 4));
            series1.Points.Add(new DataPoint(3, 2));
            series1.Points.Add(new DataPoint(4, 7));
            series1.Points.Add(new DataPoint(6, 6));
            series1.Points.Add(new DataPoint(8, 8));
            PlotFrame.Series.Add(series1);

            var series2 = new OxyPlot.Series.LineSeries
            {
                StrokeThickness = 1,
                Color = OxyColors.Tan
            };
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

        public void Plot(Spectrum spectrum, PlotMethod plotMethod)
        {
            if (plotMethod == PlotMethod.Replace)
            {
                this.Clear();
            }

            // TODO: Spectrum checkouts

            PlotFrame.Title = spectrum.SpectrumName;

            var series1 = new OxyPlot.Series.LineSeries
            {
                StrokeThickness = 1,
                Color = OxyColors.Red
            };
            if (plotMethod == PlotMethod.Combine)
            {
                series1.Color = OxyColors.Blue;
            }
            for (int i = 0; i < spectrum.Bins.Count; i++)
            {
                series1.Points.Add(new DataPoint(spectrum.Bins[i].X, spectrum.Bins[i].Y));
            }
            RecountPlotAxes(spectrum);
            PlotFrame.Series.Add(series1);
            PlotFrame.InvalidatePlot(true);
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
                Maximum = maxX
            });
            PlotFrame.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                AbsoluteMinimum = minY,
                AbsoluteMaximum = maxY,
                Minimum = minY,
                Maximum = maxY
            });
        }
    }
}

