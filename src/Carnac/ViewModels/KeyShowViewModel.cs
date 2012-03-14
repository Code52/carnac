using System.Collections.ObjectModel;
using Caliburn.Micro;
using Carnac.Logic.Models;
using Message = Carnac.Logic.Models.Message;

namespace Carnac.ViewModels
{
    public class KeyShowViewModel: Screen
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
