using System.Windows;
using System.Windows.Controls;
using Carnac.Logic.Native;

namespace Carnac.UI
{
    public partial class PreferencesView
    {
        public PreferencesView(PreferencesViewModel viewModel)
        {
            DataContext = viewModel;
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