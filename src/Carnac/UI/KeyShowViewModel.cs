using System.Collections.ObjectModel;
using Carnac.Logic;
using Carnac.Logic.Models;
using Message = Carnac.Logic.Models.Message;

namespace Carnac.UI
{
    public class KeyShowViewModel: NotifyPropertyChanged
    {
        public KeyShowViewModel(ObservableCollection<Message> keys, PopupSettings popupSettings)
        {
            Keys = keys;
            Settings = popupSettings;
        }

        public ObservableCollection<Message> Keys { get; private set; }

        public PopupSettings Settings { get; set; }
    }
}
