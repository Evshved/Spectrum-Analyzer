﻿using System;
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
        private static float _precision;

        static Settings()
        {
            _ignoredPhrases = ConfigurationManager.AppSettings["IgnoredPhrases"].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            _precision = float.Parse(ConfigurationManager.AppSettings["Precision"]);
        }

        public static string[] IgnoredPhrases => _ignoredPhrases;
        public static float Precision => _precision;
    }
}