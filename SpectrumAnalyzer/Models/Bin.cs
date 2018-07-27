using OxyPlot;
using System.Globalization;

namespace SpectrumAnalyzer.Helpers
{
    public class Bin
    {
        public float X;

        public float Y;

        public Bin(string x, string y)
        {
            X = float.Parse(x.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture);
            Y = float.Parse(y.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture);
        }

        public Bin(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"X: {this.X}, Y: {this.Y}";
        }

        /// <summary>
        /// SpectrumAnalyzer 'Bin' implicit converter to OxyPlot 'DataPoint'
        /// </summary>
        /// <param name="bin"></param>
        public static implicit operator DataPoint(Bin bin)
        {
            return new DataPoint(bin.X, bin.Y);
        }
    }
}
