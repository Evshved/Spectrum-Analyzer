using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace SpectrumAnalyzer.Models
{
    public class Spectrum : INotifyPropertyChanged
    {
        private string[] _contents;
        private bool _isVisible;
        public string Name { get; set; }
        public bool IsVisible { get { return _isVisible; } set { _isVisible = value; OnPropertyChanged("IsVisible"); } }
        public List<Bin> Bins;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

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
            Name = fileName;
            IsVisible = true;
        }

        public Spectrum(List<Bin> data, string name)
        {
            this.Bins = new List<Bin>();
            this.Bins.AddRange(data);
            this.Name = name;
            this.IsVisible = true;
        }

        public override string ToString()
        {
            return this.Name;
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

        public Spectrum GetQuantized()
        {
            var result = new List<Bin>();
            var increment = CalculateIncrement() * Settings.Precision;

            result.Add(Bins[0]);

            foreach (var bin in Bins.Skip(1))
            {
                var previous = result.Last();
                var curX = previous.X + increment;
                while (curX < bin.X)
                {
                    var a = curX - previous.X;
                    var b = bin.X - curX;
                    var c = bin.Y - previous.Y;
                    var curY = a * c / (a + b) + previous.Y;
                    result.Add(new Bin(curX, curY));
                    previous = result.Last();
                    curX = previous.X + increment;
                }
            }
            return new Spectrum(result, "Quantized");
        }

        public List<Bin> GetSmoothed()
        {
            throw new NotImplementedException();
        }

        public List<Bin> GetDeconvoluted()
        {
            throw new NotImplementedException();
        }

        public Spectrum GetSearched()
        {
            int Iterations = 3;
            int AverageWindow = 3;

            double sigma = 2;
            double threshold = 0.05 * 100;
            int maxPeaks = 100;

            bool backgroundRemove = true;
            bool markov = true;

            double denoizeLevel = 10;
            List<string> totalPeaks = new List<string>();

            int size = this.Bins.Count;

            double[] dataX = new double[] { };
            Array.Resize(ref dataX, size);
            this.ToXArray().CopyTo(dataX, 0);

            double[] dataY = new double[] { };
            Array.Resize(ref dataY, size);
            this.ToYArray().CopyTo(dataY, 0);

            double[] resultDataY;
            double[] peakX;

            int peaksNumber = _search(dataY, out resultDataY, out peakX, size, Iterations, AverageWindow, sigma, threshold, maxPeaks, backgroundRemove, markov);

            List<Bin> resultDataYList = new List<Bin>();

            for (int i = 0; i < this.Bins.Count; i++)
            {
                resultDataYList.Add(new Bin(this.Bins[i].X, (float)resultDataY[i]));
            }
            return new Spectrum(resultDataYList, "Searched");
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
                var result = new string[Bins.Count];
                for (int i = 0; i < Bins.Count; i++)
                {
                    result[i] = Bins[i].X + ";" + Bins[i].Y;
                }
                File.WriteAllLines(AppDomain.CurrentDomain.BaseDirectory + $@"\{this.Name}.csv", result);
            }
        }

        public float[] ToXArray()
        {
            List<float> result = new List<float>();
            for (int i = 0; i < Bins.Count; i++)
            {
                result.Add(Bins[i].X);
            }
            return result.ToArray();
        }

        public float[] ToYArray()
        {
            List<float> result = new List<float>();
            for (int i = 0; i < Bins.Count; i++)
            {
                result.Add(Bins[i].Y);
            }
            return result.ToArray();
        }

        private int _search(double[] dataY, out double[] resultDataY, out double[] peakX, int size, int deconIterations, int averageWindow, double sigma, double threshold, int maxPeaks, bool backgroundRemove, bool markov)
        {
            peakX = new double[maxPeaks];
            int fNPeaks = 0;
            int PEAK_WINDOW = 1024;

            double[] _source = dataY;
            resultDataY = new double[size];

            int i, j, numberIterations = (int)(7 * sigma + 0.5);
            double a, b, c;
            int k, lindex, posit, imin, imax, jmin, jmax, lh_gold, priz;
            double lda, ldb, ldc, area, maximum, maximum_decon;
            int xmin, xmax, l, peak_index = 0, size_ext = size + 2 * numberIterations, shift = numberIterations, bw = 2, w;
            double maxch;
            double nom, nip, nim, sp, sm, plocha = 0;
            double m0low = 0, m1low = 0, m2low = 0, l0low = 0, l1low = 0, detlow, av, men;

            if (sigma < 1)
            {
                // Console.WriteLine("SearchHighRes", "Invalid sigma, must be greater than or equal to 1");
                return 0;
            }

            if (threshold <= 0 || threshold >= 100)
            {
                // Console.WriteLine("SearchHighRes", "Invalid threshold, must be positive and less than 100");
                return 0;
            }

            j = (int)(5.0 * sigma + 0.5);
            if (j >= PEAK_WINDOW / 2)
            {
                Console.WriteLine("SearchHighRes", "Too large sigma");
                return 0;
            }

            if (markov == true)
            {
                if (averageWindow <= 0)
                {
                    Console.WriteLine("SearchHighRes", "Averaging window must be positive");
                    return 0;
                }
            }

            if (backgroundRemove == true)
            {
                if (size < 2 * numberIterations + 1)
                {
                    Console.WriteLine("SearchHighRes", "Too large clipping window");
                    return 0;
                }
            }

            k = (int)(2 * sigma + 0.5);
            if (k >= 2)
            {
                for (i = 0; i < k; i++)
                {
                    a = i; b = _source[i];
                    m0low += 1;
                    m1low += a;
                    m2low += a * a;
                    l0low += b;
                    l1low += a * b;
                }
                detlow = m0low * m2low - m1low * m1low;
                if (detlow != 0)
                    l1low = (-l0low * m1low + l1low * m0low) / detlow;

                else
                    l1low = 0;
                if (l1low > 0)
                    l1low = 0;
            }

            else
            {
                l1low = 0;
            }

            i = (int)(7 * sigma + 0.5);
            i = 2 * i;
            double[] working_space = new double[7 * (size + i)];
            for (j = 0; j < 7 * (size + i); j++) working_space[j] = 0;
            for (i = 0; i < size_ext; i++)
            {
                if (i < shift)
                {
                    a = i - shift;
                    working_space[i + size_ext] = _source[0] + l1low * a;
                    if (working_space[i + size_ext] < 0)
                        working_space[i + size_ext] = 0;
                }

                else if (i >= size + shift)
                {
                    a = i - (size - 1 + shift);
                    working_space[i + size_ext] = _source[size - 1];
                    if (working_space[i + size_ext] < 0)
                        working_space[i + size_ext] = 0;
                }

                else
                    working_space[i + size_ext] = _source[i - shift];
            }

            // Background Remove ----------------------------
            if (backgroundRemove == true)
            {
                for (i = 1; i <= numberIterations; i++)
                {
                    for (j = i; j < size_ext - i; j++)
                    {
                        if (markov == false)
                        {
                            a = working_space[size_ext + j];
                            b = (working_space[size_ext + j - i] + working_space[size_ext + j + i]) / 2.0;
                            if (b < a)
                                a = b;

                            working_space[j] = a;
                        }

                        else
                        {
                            a = working_space[size_ext + j];
                            av = 0;
                            men = 0;
                            for (w = j - bw; w <= j + bw; w++)
                            {
                                if (w >= 0 && w < size_ext)
                                {
                                    av += working_space[size_ext + w];
                                    men += 1;
                                }
                            }
                            av = av / men;
                            b = 0;
                            men = 0;
                            for (w = j - i - bw; w <= j - i + bw; w++)
                            {
                                if (w >= 0 && w < size_ext)
                                {
                                    b += working_space[size_ext + w];
                                    men += 1;
                                }
                            }
                            b = b / men;
                            c = 0;
                            men = 0;
                            for (w = j + i - bw; w <= j + i + bw; w++)
                            {
                                if (w >= 0 && w < size_ext)
                                {
                                    c += working_space[size_ext + w];
                                    men += 1;
                                }
                            }
                            c = c / men;
                            b = (b + c) / 2;
                            if (b < a)
                                av = b;
                            working_space[j] = av;
                        }
                    }
                    for (j = i; j < size_ext - i; j++)
                        working_space[size_ext + j] = working_space[j];
                }
                for (j = 0; j < size_ext; j++)
                {
                    if (j < shift)
                    {
                        a = j - shift;
                        b = _source[0] + l1low * a;
                        if (b < 0) b = 0;
                        working_space[size_ext + j] = b - working_space[size_ext + j];
                    }

                    else if (j >= size + shift)
                    {
                        a = j - (size - 1 + shift);
                        b = _source[size - 1];
                        if (b < 0) b = 0;
                        working_space[size_ext + j] = b - working_space[size_ext + j];
                    }

                    else
                    {
                        working_space[size_ext + j] = _source[j - shift] - working_space[size_ext + j];
                    }
                }
                for (j = 0; j < size_ext; j++)
                {
                    if (working_space[size_ext + j] < 0) working_space[size_ext + j] = 0;
                }
            }

            // ----------------------------
            for (i = 0; i < size_ext; i++)
            {
                working_space[i + 6 * size_ext] = working_space[i + size_ext];
            }

            // Markov smoothing ----------------------------

            if (markov == true)
            {
                for (j = 0; j < size_ext; j++)
                    working_space[2 * size_ext + j] = working_space[size_ext + j];
                xmin = 0;
                xmax = size_ext - 1;
                for (i = 0, maxch = 0; i < size_ext; i++)
                {
                    working_space[i] = 0;
                    if (maxch < working_space[2 * size_ext + i])
                        maxch = working_space[2 * size_ext + i];
                    plocha += working_space[2 * size_ext + i];
                }
                if (maxch == 0)
                {
                    return 0;
                }

                nom = 1;
                working_space[xmin] = 1;
                for (i = xmin; i < xmax; i++)
                {
                    nip = working_space[2 * size_ext + i] / maxch;
                    nim = working_space[2 * size_ext + i + 1] / maxch;
                    sp = 0;
                    sm = 0;
                    for (l = 1; l <= averageWindow; l++)
                    {
                        if ((i + l) > xmax)
                            a = working_space[2 * size_ext + xmax] / maxch;

                        else
                            a = working_space[2 * size_ext + i + l] / maxch;

                        b = a - nip;
                        if (a + nip <= 0)
                            a = 1;

                        else
                            a = Math.Sqrt(a + nip);

                        b = b / a;
                        b = Math.Exp(b);
                        sp = sp + b;
                        if ((i - l + 1) < xmin)
                            a = working_space[2 * size_ext + xmin] / maxch;

                        else
                            a = working_space[2 * size_ext + i - l + 1] / maxch;

                        b = a - nim;
                        if (a + nim <= 0)
                            a = 1;

                        else
                            a = Math.Sqrt(a + nim);

                        b = b / a;
                        b = Math.Exp(b);
                        sm = sm + b;
                    }
                    a = sp / sm;
                    a = working_space[i + 1] = working_space[i] * a;
                    nom = nom + a;
                }
                for (i = xmin; i <= xmax; i++)
                {
                    working_space[i] = working_space[i] / nom;
                }
                for (j = 0; j < size_ext; j++)
                    working_space[size_ext + j] = working_space[j] * plocha;
                for (j = 0; j < size_ext; j++)
                {
                    working_space[2 * size_ext + j] = working_space[size_ext + j];
                }
                if (backgroundRemove == true)
                {
                    for (i = 1; i <= numberIterations; i++)
                    {
                        for (j = i; j < size_ext - i; j++)
                        {
                            a = working_space[size_ext + j];
                            b = (working_space[size_ext + j - i] + working_space[size_ext + j + i]) / 2.0;
                            if (b < a)
                                a = b;
                            working_space[j] = a;
                        }
                        for (j = i; j < size_ext - i; j++)
                            working_space[size_ext + j] = working_space[j];
                    }
                    for (j = 0; j < size_ext; j++)
                    {
                        working_space[size_ext + j] = working_space[2 * size_ext + j] - working_space[size_ext + j];
                    }
                }
            }

            // ----------------------------

            //deconvolution starts
            area = 0;
            lh_gold = -1;
            posit = 0;
            maximum = 0;
            //generate response vector
            for (i = 0; i < size_ext; i++)
            {
                lda = (double)(i - 3 * sigma);
                lda = lda * lda / (2 * sigma * sigma);
                j = (int)(1000 * Math.Exp(-lda));
                lda = j;
                if (lda != 0)
                    lh_gold = i + 1;

                working_space[i] = lda;
                area = area + lda;
                if (lda > maximum)
                {
                    maximum = lda;
                    posit = i;
                }
            }
            //read source vector
            for (i = 0; i < size_ext; i++)
                working_space[2 * size_ext + i] = Math.Abs(working_space[size_ext + i]);
            //create matrix at*a(vector b)
            i = lh_gold - 1;
            if (i > size_ext)
                i = size_ext;

            imin = -i;
            imax = i;
            for (i = imin; i <= imax; i++)
            {
                lda = 0;
                jmin = 0;
                if (i < 0)
                    jmin = -i;
                jmax = lh_gold - 1 - i;
                if (jmax > (lh_gold - 1))
                    jmax = lh_gold - 1;

                for (j = jmin; j <= jmax; j++)
                {
                    ldb = working_space[j];
                    ldc = working_space[i + j];
                    lda = lda + ldb * ldc;
                }
                working_space[size_ext + i - imin] = lda;
            }
            //create vector p
            i = lh_gold - 1;
            imin = -i;
            imax = size_ext + i - 1;
            for (i = imin; i <= imax; i++)
            {
                lda = 0;
                for (j = 0; j <= (lh_gold - 1); j++)
                {
                    ldb = working_space[j];
                    k = i + j;
                    if (k >= 0 && k < size_ext)
                    {
                        ldc = working_space[2 * size_ext + k];
                        lda = lda + ldb * ldc;
                    }

                }
                working_space[4 * size_ext + i - imin] = lda;
            }
            //move vector p
            for (i = imin; i <= imax; i++)
                working_space[2 * size_ext + i - imin] = working_space[4 * size_ext + i - imin];
            //initialization of resulting vector
            for (i = 0; i < size_ext; i++)
                working_space[i] = 1;
            //START OF ITERATIONS
            for (lindex = 0; lindex < deconIterations; lindex++)
            {
                for (i = 0; i < size_ext; i++)
                {
                    if (Math.Abs(working_space[2 * size_ext + i]) > 0.00001 && Math.Abs(working_space[i]) > 0.00001)
                    {
                        lda = 0;
                        jmin = lh_gold - 1;
                        if (jmin > i)
                            jmin = i;

                        jmin = -jmin;
                        jmax = lh_gold - 1;
                        if (jmax > (size_ext - 1 - i))
                            jmax = size_ext - 1 - i;

                        for (j = jmin; j <= jmax; j++)
                        {
                            ldb = working_space[j + lh_gold - 1 + size_ext];
                            ldc = working_space[i + j];
                            lda = lda + ldb * ldc;
                        }
                        ldb = working_space[2 * size_ext + i];
                        if (lda != 0)
                            lda = ldb / lda;

                        else
                            lda = 0;

                        ldb = working_space[i];
                        lda = lda * ldb;
                        working_space[3 * size_ext + i] = lda;
                    }
                }
                for (i = 0; i < size_ext; i++)
                {
                    working_space[i] = working_space[3 * size_ext + i];
                }
            }
            //shift resulting spectrum
            for (i = 0; i < size_ext; i++)
            {
                lda = working_space[i];
                j = i + posit;
                j = j % size_ext;
                working_space[size_ext + j] = lda;
            }
            //write back resulting spectrum
            maximum = 0;
            maximum_decon = 0;
            j = lh_gold - 1;
            for (i = 0; i < size_ext - j; i++)
            {
                if (i >= shift && i < size + shift)
                {
                    working_space[i] = area * working_space[size_ext + i + j];
                    if (maximum_decon < working_space[i])
                        maximum_decon = working_space[i];
                    if (maximum < working_space[6 * size_ext + i])
                        maximum = working_space[6 * size_ext + i];
                }

                else
                    working_space[i] = 0;
            }
            lda = 1;
            if (lda > threshold)
                lda = threshold;
            lda = lda / 100;

            // ----------------------------

            //searching for peaks in deconvolved spectrum
            for (i = 1; i < size_ext - 1; i++)
            {
                if (working_space[i] > working_space[i - 1] && working_space[i] > working_space[i + 1])
                {
                    if (i >= shift && i < size + shift)
                    {
                        if (working_space[i] > lda * maximum_decon && working_space[6 * size_ext + i] > threshold * maximum / 100.0)
                        {
                            for (j = i - 1, a = 0, b = 0; j <= i + 1; j++)
                            {
                                a += (double)(j - shift) * working_space[j];
                                b += working_space[j];
                            }
                            a = a / b;
                            if (a < 0)
                                a = 0;

                            if (a >= size)
                                a = size - 1;
                            if (peak_index == 0)
                            {
                                peakX[0] = a;
                                peak_index = 1;
                            }

                            else
                            {
                                for (j = 0, priz = 0; j < peak_index && priz == 0; j++)
                                {
                                    if (working_space[6 * size_ext + shift + (int)a] > working_space[6 * size_ext + shift + (int)peakX[j]])
                                        priz = 1;
                                }
                                if (priz == 0)
                                {
                                    if (j < maxPeaks)
                                    {
                                        peakX[j] = a;
                                    }
                                }

                                else
                                {
                                    for (k = peak_index; k >= j; k--)
                                    {
                                        if (k < maxPeaks)
                                        {
                                            peakX[k] = peakX[k - 1];
                                        }
                                    }
                                    peakX[j - 1] = a;
                                }
                                if (peak_index < maxPeaks)
                                    peak_index += 1;
                            }
                        }
                    }
                }
            }

            for (i = 0; i < size; i++)
            {
                resultDataY[i] = working_space[i + shift];
            }

            fNPeaks = peak_index;

            if (peak_index == maxPeaks)
                Console.WriteLine("SearchHighRes", "Peak buffer full");

            return fNPeaks;
        }
    }
}