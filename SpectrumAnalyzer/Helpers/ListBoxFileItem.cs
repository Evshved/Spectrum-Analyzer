using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumAnalyzer.Helpers
{
    class ListBoxFileItem
    {
        public string fileName;
        public string filePath;

        public override string ToString()
        {
            return fileName;
        }
    }
}
