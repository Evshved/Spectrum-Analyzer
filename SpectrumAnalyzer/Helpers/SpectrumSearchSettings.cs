using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumAnalyzer.Helpers
{
    public class SpectrumSearchSettings
    {
        public int DeconvolutionIterations { get; set; } = 3;
        public int AverageWindow { get; set; } = 3;
        public double Sigma { get; set; } = 2;
        public double Threshold { get; set; } = 0.05 * 100;
        public int MaxPeaks { get; set; } = 100;
        public bool BackgroundRemove { get; set; } = true;
        public bool Markov { get; set; } = true;
    }
}
