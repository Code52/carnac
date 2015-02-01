using System.Windows;
using System.Windows.Controls;
using Carnac.Logic.Native;
using Carnac.UI;

namespace Carnac
{
    public partial class PositionOnMonitorSelector
    {
        public PositionOnMonitorSelector()
        {
            InitializeComponent();
        }

        private void RadioChecked(object sender, RoutedEventArgs e)
        {
            var dc = DataContext as PreferencesViewModel;
            if (dc == null)
                return;

            var rb = sender as RadioButton;
            if (rb == null)
                return;

            var tag = rb.Tag as DetailedScreen;
            if (tag == null)
                return;

            dc.SelectedScreen = tag;
        }
    }
}