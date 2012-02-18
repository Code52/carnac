using System.Collections.ObjectModel;
using Caliburn.Micro;
using Carnac.Models;
using Message = Carnac.Models.Message;

namespace Carnac.ViewModels
{
    public class KeyShowViewModel: Screen
    {
        public KeyShowViewModel(ObservableCollection<Message> keys, Settings settings)
        {
            Keys = keys;
            Settings = settings;
        }

        public ObservableCollection<Message> Keys { get; private set; }

        public Settings Settings { get; set; }
    }
}
