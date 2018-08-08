using SQLite.Net.Attributes;

namespace SpectrumAnalyzer.Models
{
    public partial class SpectrumBase
    {
        [PrimaryKey, AutoIncrement, Unique, NotNull]
        public long Id { get; set; }

        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public string Data { get; set; }

        public string Peaks { get; set; }

        [Indexed]
        public int? SubstanceId { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
