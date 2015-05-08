using System.Windows.Media;
using Carnac.Logic;

namespace Carnac.UI
{
    public class AvailableColor : NotifyPropertyChanged
    {
        public AvailableColor(string name, Color color)
        {
            Name = name;
            Brush = new SolidColorBrush(color);
        }

        public string Name { get; private set; }

        public SolidColorBrush Brush { get; private set; }
    }
}