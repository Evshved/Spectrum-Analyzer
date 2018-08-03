using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
