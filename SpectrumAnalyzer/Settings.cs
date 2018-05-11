using System;
using System.Configuration;
using System.Globalization;

namespace SpectrumAnalyzer
{
    public static class Settings
    {
        private static string[] _ignoredPhrases;
        private static float _precision;

        static Settings()
        {
            _ignoredPhrases = ConfigurationManager.AppSettings["IgnoredPhrases"].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            _precision = float.Parse((ConfigurationManager.AppSettings["Precision"]).Replace(",", "."), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
        }

        public static string[] IgnoredPhrases => _ignoredPhrases;
        public static float Precision => _precision;
    }
}

