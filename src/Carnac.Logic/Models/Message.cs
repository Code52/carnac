using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Media;

namespace Carnac.Logic.Models
{
    public class Message : NotifyPropertyChanged
    {
        static readonly string[] RepeatDetectionText = { "Back", "Left", "Right", "Down", "Up" };
        readonly ObservableCollection<string> textCollection;
        readonly ObservableCollection<KeyPress> keyCollection;
        readonly ReadOnlyObservableCollection<string> roTextCollection;
        readonly ReadOnlyObservableCollection<KeyPress> roKeysCollection;
        readonly string processName;
        readonly ImageSource processIcon;
        readonly string shortcutName;
        readonly bool canBeMerged;
        readonly bool isShortcut;
        int lastTextRepeatCount = 1;
        string lastText;
        KeyPress lastKeyPress;


        public Message()
        {
            textCollection = new ObservableCollection<string>();
            keyCollection = new ObservableCollection<KeyPress>();
            roTextCollection = new ReadOnlyObservableCollection<string>(textCollection);
            roKeysCollection = new ReadOnlyObservableCollection<KeyPress>(keyCollection);
            Updated = Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    handler => (sender, e) => handler(e),
                    add => PropertyChanged += add,
                    remove => PropertyChanged -= remove)
                .Select(_ => Unit.Default);
        }

        public Message(KeyPress key)
            : this()
        {
            processName = key.Process.ProcessName;
            processIcon = key.Process.ProcessIcon;
            AddKey(key);
            canBeMerged = !key.HasModifierPressed;
        }

        public Message(IEnumerable<KeyPress> keys, KeyShortcut shortcut)
            : this()
        {
            var allKeys = keys.ToArray();
            var distinctProcessName = allKeys.Select(k => k.Process.ProcessName)
                .Distinct()
                .ToArray();
            if (distinctProcessName.Count() != 1)
                throw new InvalidOperationException("Keys are from different processes");

            processName = distinctProcessName.Single();
            processIcon = allKeys.First().Process.ProcessIcon;

            foreach (var keyPress in allKeys)
            {
                AddKey(keyPress);
            }
            shortcutName = shortcut.Name;
            if (!string.IsNullOrEmpty(shortcutName))
                AddText(string.Format(" [{0}]", shortcutName));
            isShortcut = true;
            canBeMerged = false;
        }

        public string ProcessName { get { return processName; } }

        public ImageSource ProcessIcon { get { return processIcon; } }

        public string ShortcutName { get { return shortcutName; } }

        public bool CanBeMerged { get { return canBeMerged; } }

        public bool IsShortcut { get { return isShortcut; } }

        public ReadOnlyObservableCollection<string> Text { get { return roTextCollection; } }

        public ReadOnlyObservableCollection<KeyPress> Keys { get { return roKeysCollection; } }


        public DateTime LastMessage { get; private set; } //AddKey

        public int Count { get; private set; }            //AddKey

        public bool IsDeleting { get; set; }

        public IObservable<Unit> Updated { get; private set; }

        public Message Merge(Message key)
        {
            foreach (var keyPress in key.Keys)
            {
                AddKey(keyPress);
            }

            return this;
        }

        //ctor(KeyPress key)
        //ctor(IEnumerable<KeyPress> keys, KeyShortcut shortcut)
        //Merge(Message key)
        void AddKey(KeyPress keyPress)
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

        //AddKey(KeyPress keyPress)
        void AddText(string text)
        {
            var formattedText = Format(text, lastKeyPress.HasModifierPressed);

            if (lastText == formattedText && RepeatDetectionText.Contains(text) && Text.Any())
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

        static string Format(string text, bool isShortcut)
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

        static string GetString(int decimalValue)
        {
            return new string(new[] { (char)decimalValue });
        }
    }
}
