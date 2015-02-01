namespace Carnac.UI
{
    public partial class PreferencesView
    {
        public PreferencesView(PreferencesViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}