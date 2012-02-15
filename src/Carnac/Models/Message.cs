using System;
using Caliburn.Micro;
using NotifyPropertyWeaver;

namespace Carnac.Models
{
    public class Message: PropertyChangedBase
    {
        public string ProcessName { get; set; }

        public DateTime StartingTime { get; set; }
        public DateTime LastMessage { get; set; }

        [NotifyProperty(AlsoNotifyFor = new[] {"Count", "LastMessage"})]
        public string Text { get; set; }
        public int Count { get; set; }

        public bool IsDeleting { get; set; }
    }
}
