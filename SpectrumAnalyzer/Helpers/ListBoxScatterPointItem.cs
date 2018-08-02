using OxyPlot.Series;
using System;

namespace SpectrumAnalyzer.Helpers
{
    public class ListBoxScatterPointItem
    {
        public ScatterPoint Point { get; set; }

        public ListBoxScatterPointItem(ScatterPoint point)
        {
            Point = point;
        }

        public override string ToString()
        {
            return $"X: {Math.Round(Point.X, 4)} Y: {Math.Round(Point.Y, 4)}";
        }
    }
}
