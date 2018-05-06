using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumAnalyzer.Helpers
{
    public class SpectrumProfile
    {
        internal Spectrum OriginalData;
        internal Dictionary<string, Spectrum> Transitions = new Dictionary<string, Spectrum>();

        internal SpectrumProfile(Spectrum spectrum)
        {
            OriginalData = spectrum;
        }
    }
}
