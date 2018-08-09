using SpectrumAnalyzer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpectrumAnalyzer.Models
{
    public class SpectrumTransition
    {
        public string Name { get; set; }
        public List<Bin> Data { get; set; } = new List<Bin>();

        public SpectrumTransition()
        {

        }

        internal static SpectrumTransition ParseFromString(string contents, string name)
        {
            var _contents = contents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var result = new SpectrumTransition();
            if (_contents.Length > 2)
            {
                result.Data = new List<Bin>();

                for (int i = 0; i < _contents.Length; i++)
                {
                    var str = _contents[i];
                    if (Settings.IgnoredPhrases.Any(str.Contains))
                    {
                        continue;
                    }
                    result.Data.Add(Bin.Parse(str));
                }
            }
            result.Name = name;
            return result;
        }

        internal double[] GetDataYArray()
        {
            double[] result = new double[this.Data.Count];
            for (int i = 0; i < this.Data.Count; i++)
            {
                result[i] = this.Data[i].Y;
            }
            return result;
        }
    }
}