namespace SpectrumAnalyzer.Helpers
{
    public class ListBoxFileItem
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public ListBoxFileItem(string fileName, string filePath)
        {
            Name = fileName;
            Path = filePath;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

