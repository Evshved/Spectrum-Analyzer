using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpectrumAnalyzer.Helpers
{
    internal class Spectrum
    {
        private string[] _contents;

        public List<Bin> Bins;
        public string SpectrumName;
        public List<Bin> QuantizedSpectrum;

        public Spectrum(string contents, string fileName)
        {
            _contents = contents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (_contents.Length > 2)
            {
                Bins = new List<Bin>();

                for (int i = 0; i < _contents.Length; i++)
                {
                    var str = _contents[i];
                    if (Settings.IgnoredPhrases.Any(str.Contains))
                    {
                        continue;
                    }
                    Bins.Add(ParseBin(str));
                }
            }
            SpectrumName = fileName;
        }

        private Bin ParseBin(string str)
        {
            string result = string.Empty;

            for (int i = 0; i < str.Length; i++)
            {
                if (Char.IsDigit(str[i]) || str[i] == '.' || str[i] == ',' || str[i] == '-')
                {
                    result += str[i];
                }
                else
                {
                    result += ";";
                }
            }
            string[] splitted = result.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            return new Bin(splitted[0], splitted[1]);
        }

        public void Quantize()
        {
            QuantizedSpectrum = new List<Bin>();
            var increment = CalculateIncrement() / 1000;
            
            QuantizedSpectrum.Add(Bins[0]);

            foreach (var bin in Bins.Skip(1))
            {
                var previous = QuantizedSpectrum.Last();
                var curX = previous.X + increment;
                while (curX < bin.X)
                {
                    var a = curX - previous.X;
                    var b = bin.X - curX;
                    var c = bin.Y - previous.Y;
                    var curY = a * c / (a + b) + previous.Y;
                    QuantizedSpectrum.Add(new Bin(curX, curY));
                    previous = QuantizedSpectrum.Last();
                    curX = previous.X + increment;
                }
            }
        }

        private float CalculateIncrement()
        {
            float result = 0;
            for (int i = 0; i < this.Bins.Count - 1; i++)
            {
                result += this.Bins[i + 1].X - this.Bins[i].X;
            }
            return result / (Bins.Count - 1);
        }

        public void SaveToFile()
        {
            if (Bins.Count > 0)
            {
                var result = string.Empty;
                for (int i = 0; i < Bins.Count; i++)
                {
                    result += $"{Bins[i].X};{Bins[i].Y}{Environment.NewLine}";
                }
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"\original_spectrum.csv", result);
            }
            if (QuantizedSpectrum.Count > 0)
            {
                var result = new string[QuantizedSpectrum.Count];
                for (int i = 0; i < QuantizedSpectrum.Count; i++)
                {
                    result[i] = $"{QuantizedSpectrum[i].X};{QuantizedSpectrum[i].Y}";
                }
                File.WriteAllLines(AppDomain.CurrentDomain.BaseDirectory + @"\quantized_spectrum.csv", result);
            }
        }

        public class Bin
        {
            public float X;
            public float Y;
            public Bin(string x, string y)
            {
                X = float.Parse(x);
                Y = float.Parse(y);
            }
            public Bin(float x, float y)
            {
                X = x;
                Y = y;
            }
        }
    }
}