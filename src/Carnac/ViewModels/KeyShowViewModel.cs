using System.Collections.ObjectModel;
using Caliburn.Micro;
using Message = Carnac.Models.Message;

namespace Carnac.ViewModels
{
    public class KeyShowViewModel: Screen
    {
        public KeyShowViewModel(ObservableCollection<Message> keys)
        {
            Keys = keys;
        }

        public ObservableCollection<Message> Keys { get; private set; } 
    }
}
