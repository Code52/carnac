using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Carnac.Logic.Models
{
    public class Message : NotifyPropertyChanged
    {
        readonly string[] repeatDetectionText = { "Back", "Left", "Right", "Down", "Up" };
        private readonly ObservableCollection<string> textCollection;
        private readonly ObservableCollection<KeyPress> keyCollection;
        private int lastTextRepeatCount = 1;
        private string lastText;
        private KeyPress lastKeyPress;
        private string shortcutName;

        public Message()
        {
            textCollection = new ObservableCollection<string>();
            keyCollection = new ObservableCollection<KeyPress>();
            Text = new ReadOnlyObservableCollection<string>(textCollection);
            Keys = new ReadOnlyObservableCollection<KeyPress>(keyCollection);
            Updated = Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    handler => (sender, e) => handler(e),
                    add => PropertyChanged += add,
                    remove => PropertyChanged -= remove)
                .Select(_ => Unit.Default);
        }

        public Message(KeyPress key) : this()
        {
            ProcessName = key.Process.ProcessName;
            AddKey(key);
            CanBeMerged = !key.HasModifierPressed;
        }

        public Message(IEnumerable<KeyPress> keys, KeyShortcut shortcut)
            : this()
        {
            var distinctProcessName = keys.Select(k => k.Process.ProcessName).Distinct();
            if (distinctProcessName.Count() != 1)
                throw new InvalidOperationException("Keys are from different processes");

            ProcessName = distinctProcessName.Single();
            foreach (var keyPress in keys)
            {
                AddKey(keyPress);
            }
            ShortcutName = shortcut.Name;
            IsShortcut = true;
            CanBeMerged = false;
        }

        public string ProcessName { get; private set; }

        public DateTime LastMessage { get; private set; }

        public ReadOnlyObservableCollection<string> Text { get; private set; }

        public ReadOnlyObservableCollection<KeyPress> Keys { get; private set; }

        public int Count { get; private set; }

        public bool IsDeleting { get; set; }

        public bool CanBeMerged { get; private set; }

        public string ShortcutName
        {
            get { return shortcutName; }
            private set
            {
                shortcutName = value;
                if (!string.IsNullOrEmpty(shortcutName))
                    AddText(string.Format(" [{0}]", value));
            }
        }

        public bool IsShortcut { get; private set; }

        public IObservable<Unit> Updated { get; private set; }

        private void AddKey(KeyPress keyPress)
        {
            keyCollection.Add(keyPress);
            if (lastKeyPress != null && lastKeyPress.HasModifierPressed)
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
            Count++;
            LastMessage = DateTime.Now;
        }

        private void AddText(string text)
        {
            var formattedText = Format(text, lastKeyPress.HasModifierPressed);

            if (lastText == formattedText && repeatDetectionText.Contains(text) && Text.Any())
            {
                var repeatText = string.Format(" x {0} ", ++lastTextRepeatCount);
                if (Text.Last() == lastText)
                    textCollection.Add(repeatText);
                else
                    textCollection[Text.Count - 1] = repeatText;
            }
            else
            {
                textCollection.Add(formattedText);
                lastText = formattedText;
                lastTextRepeatCount = 1;
            }
        }

        private static string Format(string text, bool isShortcut)
        {
            if (text == "Left")
                return GetString(8592);
            if (text == "Up")
                return GetString(8593);
            if (text == "Right")
                return GetString(8594);
            if (text == "Down")
                return GetString(8595);

            // If the space is part of a shortcut sequence
            // present it as a primitive key. E.g. Ctrl+Spc.
            // Otherwise we want to preserve a space as part of
            // what is probably a sentence.
            if (text == " " && isShortcut)
                return "Space";

            return text;
        }

        private static string GetString(int decimalValue)
        {
            return new string(new[] { (char)decimalValue });
        }

        public Message Merge(Message key)
        {
            foreach (var keyPress in key.Keys)
            {
                AddKey(keyPress);
            }

            return this;
        }
    }
}
