using Caliburn.Micro;
using SpectrumAnalyzer.ViewModels;
using System.Windows;

namespace SpectrumAnalyzer
{
    public class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainViewModel>();
        }
    }
}
