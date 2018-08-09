using OxyPlot.Series;
using System;

namespace SpectrumAnalyzer.Helpers
{
    public class ListViewTransitionItem
    {
        public string Name { get; set; }

        public bool IsVisible { get; set; } = true;

        public override string ToString()
        {
            return Name;
        }
    }
}
