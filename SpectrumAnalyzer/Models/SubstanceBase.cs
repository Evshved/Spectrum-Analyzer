using SQLite.Net.Attributes;

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
