using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumAnalyzer
{
    public static class Settings
    {
        private static string[] _ignoredPhrases;

        static Settings()
        {
            _ignoredPhrases = ConfigurationManager.AppSettings["IgnoredPhrases"].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] IgnoredPhrases => _ignoredPhrases;
    }
}
