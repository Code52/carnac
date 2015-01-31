using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using Carnac.Logic.Native;
using Carnac.Utilities;

namespace Carnac.UI
{
    public partial class PreferencesView
    {
        public PreferencesView(PreferencesViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            base.OnClosing(e);
        }

        private void RadioChecked(object sender, RoutedEventArgs e)
        {
            var dc = DataContext as PreferencesViewModel;
            if (dc == null)
                return;

            var rb = sender as System.Windows.Controls.RadioButton;
            if (rb == null) 
                return;

            var tag = rb.Tag as DetailedScreen;
            if (tag == null) 
                return;

            dc.SelectedScreen = tag;
        }
    }
}