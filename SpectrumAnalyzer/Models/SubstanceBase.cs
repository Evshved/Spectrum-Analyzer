using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumAnalyzer.Models
{
    public partial class SubstanceBase
    {
        [PrimaryKey, AutoIncrement, Unique, NotNull]
        public long Id { get; set; }

        [NotNull]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
