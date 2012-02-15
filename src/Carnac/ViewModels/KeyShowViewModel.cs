using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Caliburn.Micro;

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
