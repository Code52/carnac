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
        private KeyPress lastKeyPress;

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

        public void AddKey(KeyPress keyPress)
        {
            if (lastKeyPress != null && lastKeyPress.IsShortcut)
                textCollection.Add(", ");
            lastKeyPress = keyPress;
            var first = true;
            foreach (var text in keyPress.Input)
            {
                if (!first)
                    AddText(" + ");
                AddText(text);
                first = false;
            }
        }

        private void AddText(string text)
        {
            if (text == "Back" && lastText == text)
            {
                var repeatText = string.Format(" x {0} ", ++lastTextRepeatCount);
                if (Text.Last() == lastText)
                    textCollection.Add(repeatText);
                else
                    textCollection[Text.Count - 1] = repeatText;
            }
            else
            {
                textCollection.Add(text);
                lastText = text;
                lastTextRepeatCount = 1;
            }
        }
    }
}
