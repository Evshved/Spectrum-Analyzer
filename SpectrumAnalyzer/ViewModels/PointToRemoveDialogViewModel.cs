using Caliburn.Micro;
using OxyPlot.Series;
using SpectrumAnalyzer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Controls;

namespace SpectrumAnalyzer.ViewModels
{
    public class PointToRemoveDialogViewModel : Screen
    {
        public BindableCollection<ListBoxScatterPointItem> Points { get; set; } = new BindableCollection<ListBoxScatterPointItem>();

        public ListBoxScatterPointItem SelectedPoint { get; set; }

        public PointToRemoveDialogViewModel(List<ScatterPoint> nearestPoints)
        {
            nearestPoints = nearestPoints.OrderBy(p => p.X).ToList();
            foreach (ScatterPoint point in nearestPoints)
            {
                Points.Add(new ListBoxScatterPointItem(point));
            }
            NotifyOfPropertyChange(() => Points);
        }

        public void Points_SelectionChanged(SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count == 0 || args.AddedItems.Cast<object>().Where(x => x is ListBoxScatterPointItem).Count() != 1)
            {
                return;
            }

            SelectedPoint = args.AddedItems.Cast<ListBoxScatterPointItem>().First();
            NotifyOfPropertyChange(() => CanSubmit);
        }

        public void Cancel()
        {
            TryClose(false);
        }

        public bool CanSubmit
        {
            get { return SelectedPoint == null ? false : true; }
        }

        public void Submit()
        {
            TryClose(true);
        }
    }
}
