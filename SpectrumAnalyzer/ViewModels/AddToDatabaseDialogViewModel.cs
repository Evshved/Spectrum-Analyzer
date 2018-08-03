using Caliburn.Micro;

namespace SpectrumAnalyzer.ViewModels
{
    public class AddToDatabaseDialogViewModel : Screen
    {
        public string SpectrumName { get; set; }

        public AddToDatabaseDialogViewModel(string name)
        {
            SpectrumName = name;
        }

        public void Cancel()
        {
            TryClose(false);
        }

        public bool CanSubmit
        {
            get { return string.IsNullOrEmpty(SpectrumName) ? false : true; }
        }

        public void Submit()
        {
            TryClose(true);
        }
    }
}
