using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Carnac.Logic.Models
{
    public class Message : NotifyPropertyChanged
    {
        private readonly ObservableCollection<string> textCollection;
        private int lastTextRepeatCount = 1;
        private string lastText;

        public Message()
        {
            textCollection = new ObservableCollection<string>();
            Text = new ReadOnlyObservableCollection<string>(textCollection);
        }

        public string ProcessName { get; set; }

        public DateTime StartingTime { get; set; }
        public DateTime LastMessage { get; set; }
        public ReadOnlyObservableCollection<string> Text { get; private set; }
        public int Count { get; set; }
        public bool IsDeleting { get; set; }

        public void AddText(string input)
        {
            if (input == "Back" && lastText == input)
            {
                var repeatText = string.Format(" x {0} ", ++lastTextRepeatCount);
                if (Text.Last() == lastText)
                    textCollection.Add(repeatText);
                else
                    textCollection[Text.Count - 1] = repeatText;
            }
            else
            {
                textCollection.Add(input);
                lastText = input;
                lastTextRepeatCount = 1;
            }
        }
    }
}
