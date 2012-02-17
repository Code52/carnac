using System;
using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Carnac.Models
{
    public class Message: PropertyChangedBase
    {
        public Message()
        {
            Text = new ObservableCollection<string>();
        }
        public string ProcessName { get; set; }

        public DateTime StartingTime { get; set; }
        public DateTime LastMessage { get; set; }
        public ObservableCollection<string> Text { get; set; }
        public int Count { get; set; }

        public bool IsDeleting { get; set; }
    }
}
