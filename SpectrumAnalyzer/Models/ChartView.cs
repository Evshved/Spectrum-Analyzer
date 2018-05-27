using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace SpectrumAnalyzer.Models
{
    public class ChartView : INotifyPropertyChanged
    {
        private PlotModel plotModelMain;
        public PlotModel PlotModelMain
        {
            get { return PlotModelMain; }
            set { plotModelMain = value; OnPropertyChanged("ProtModelMain"); }
        }

        private DateTime lastUpdate = DateTime.Now;

        public ChartView()
        {
            PlotModelMain = new PlotModel();
            SetUpModel();
            LoadData();
        }

        private readonly List<OxyColor> colors = new List<OxyColor>
                                            {
                                                OxyColors.Green,
                                                OxyColors.IndianRed,
                                                OxyColors.Coral,
                                                OxyColors.Chartreuse,
                                                OxyColors.Azure
                                            };

        private readonly List<MarkerType> markerTypes = new List<MarkerType>
                                                   {
                                                       MarkerType.Plus,
                                                       MarkerType.Star,
                                                       MarkerType.Diamond,
                                                       MarkerType.Triangle,
                                                       MarkerType.Cross
                                                   };


        private void SetUpModel()
        {
            PlotModelMain.LegendTitle = "Legend";
            PlotModelMain.LegendOrientation = LegendOrientation.Horizontal;
            PlotModelMain.LegendPlacement = LegendPlacement.Outside;
            PlotModelMain.LegendPosition = LegendPosition.TopRight;
            PlotModelMain.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            PlotModelMain.LegendBorder = OxyColors.Black;

            var dateAxis = new DateTimeAxis() { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, IntervalLength = 80 };
            PlotModelMain.Axes.Add(dateAxis);
            var valueAxis = new LinearAxis() { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = "Value" };
            PlotModelMain.Axes.Add(valueAxis);

        }

        private void LoadData()
        {
            List<Measurement> measurements = Data.GetData();

            var dataPerDetector = measurements.GroupBy(m => m.DetectorId).OrderBy(m => m.Key).ToList();

            foreach (var data in dataPerDetector)
            {
                var lineSerie = new LineSeries
                {
                    StrokeThickness = 2,
                    MarkerSize = 3,
                    MarkerStroke = colors[data.Key],
                    MarkerType = markerTypes[data.Key],
                    CanTrackerInterpolatePoints = false,
                    Title = string.Format("Detector {0}", data.Key),
                    Smooth = false,
                };

                data.ToList().ForEach(d => lineSerie.Points.Add(new DataPoint(DateTimeAxis.ToDouble(d.DateTime), d.Value)));
                PlotModelMain.Series.Add(lineSerie);
            }
            lastUpdate = DateTime.Now;
        }

        public void UpdateModel()
        {
            List<Measurement> measurements = Data.GetUpdateData(lastUpdate);
            var dataPerDetector = measurements.GroupBy(m => m.DetectorId).OrderBy(m => m.Key).ToList();

            foreach (var data in dataPerDetector)
            {
                var lineSerie = PlotModelMain.Series[data.Key] as LineSeries;
                if (lineSerie != null)
                {
                    data.ToList()
                        .ForEach(d => lineSerie.Points.Add(new DataPoint(DateTimeAxis.ToDouble(d.DateTime), d.Value)));
                }
            }
            lastUpdate = DateTime.Now;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
