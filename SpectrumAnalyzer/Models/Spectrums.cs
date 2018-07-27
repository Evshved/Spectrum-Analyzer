using SQLite.Net.Attributes;

namespace SpectrumAnalyzer.Models
{
    public partial class Spectrums
    {
        [PrimaryKey, AutoIncrement, Unique, NotNull]
        public long ID { get; set; }

        public string TITLE { get; set; }

        public string DATA { get; set; }

        public string PEAKS { get; set; }
    }
}
