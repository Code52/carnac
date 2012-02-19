using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;

namespace Carnac.Models
{
    public class Message: PropertyChangedBase
    {
        private string lastText;
        private int lastTextRepeatCount = 1;

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

        public void AddText(string input)
        {
            if (input == "Back" && lastText == input)
            {
                var repeatText = string.Format(" x {0} ", ++lastTextRepeatCount);
                if (Text.Last() == lastText)
                    Text.Add(repeatText);
                else
                    Text[Text.Count - 1] = repeatText;
            }
            else
            {
                Text.Add(input);
                lastText = input;
                lastTextRepeatCount = 1;
            }
        }
    }
}
