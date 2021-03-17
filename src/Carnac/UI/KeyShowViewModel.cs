using System.Collections.ObjectModel;
using System.Windows;
using Carnac.Logic;
using Carnac.Logic.Models;

namespace Carnac.UI
{
    public class KeyShowViewModel: NotifyPropertyChanged
    {
        public KeyShowViewModel(PopupSettings popupSettings)
        {
            Messages = new ObservableCollection<Message>();
            Settings = popupSettings;
        }

        public ObservableCollection<Message> Messages { get; private set; }

        public PopupSettings Settings { get; set; }

        public Point CursorPosition { get; set; }

        public Thickness CursorMargins
        {
            get { return new Thickness(CursorPosition.X - 10, CursorPosition.Y - 10 , 0, 0); }
        }
    }
}
