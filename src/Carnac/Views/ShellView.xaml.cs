using System.Windows.Input;

namespace Carnac.Views
{
    public partial class ShellView
    {
        public ShellView()
        {
            InitializeComponent();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}