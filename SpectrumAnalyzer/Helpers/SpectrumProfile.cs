using System.Collections.Generic;

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
