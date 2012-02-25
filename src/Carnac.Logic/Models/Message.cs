using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Carnac.Logic.Models
{
    public class Message : NotifyPropertyChanged
    {
        // The keys enum has duplicate values in some instances
        // so we need to normalise those values.
        private static readonly Dictionary<string, string> duplicateKeys = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            {"Capital", "Caps Lock"},
            {"Next", "Page Down"},
            {"Prior", "Page Up"},
            {"Enter", "Return"}
        };

        private readonly ObservableCollection<string> textCollection;
        private readonly ObservableCollection<KeyPress> keyCollection;
        private int lastTextRepeatCount = 1;
        private string lastText;
        private KeyPress lastKeyPress;

        public Message()
        {
            textCollection = new ObservableCollection<string>();
            keyCollection = new ObservableCollection<KeyPress>();
            Text = new ReadOnlyObservableCollection<string>(textCollection);
            Keys = new ReadOnlyObservableCollection<KeyPress>(keyCollection);
        }

        public string ProcessName { get; set; }

        public DateTime StartingTime { get; set; }
        public DateTime LastMessage { get; set; }
        public ReadOnlyObservableCollection<string> Text { get; private set; }
        public ReadOnlyObservableCollection<KeyPress> Keys { get; private set; }
        public int Count { get; set; }
        public bool IsDeleting { get; set; }

        private string shortcutName;
        public string ShortcutName
        {
            get { return shortcutName; }
            set
            {
                shortcutName = value;
                if (!string.IsNullOrEmpty(shortcutName))
                    AddText(string.Format(" [{0}]", value));
            }
        }

        public void AddKey(KeyPress keyPress)
        {
            keyCollection.Add(keyPress);
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
            // lastKeyPress here actually means the current key press. See AddKey.
            var formattedText = Format(text, lastKeyPress.IsShortcut);

            if (formattedText == "Back" && lastText == formattedText)
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

            string overridenKey;
            if (duplicateKeys.TryGetValue(text, out overridenKey))
                return overridenKey;

            return BreakCamelCase(text);
        }

        private static string BreakCamelCase(string source)
        {
            if (string.IsNullOrEmpty(source))
                return source;
            var sb = new StringBuilder(source.Length);
            var lastWasCaps = true; // Don't prefix space on first character.
            for (var i = 0; i < source.Length; i++)
            {
                var c = source[i];
                var isCaps = char.IsUpper(c);
                if (isCaps && !lastWasCaps && !char.IsDigit(c))
                    sb.Append(" ");
                sb.Append(c);
                lastWasCaps = isCaps;
            }

            lock (duplicateKeys)
            {
                // Don't do this every time - just cache the result.
                if (!duplicateKeys.ContainsKey(source))
                    duplicateKeys.Add(source, sb.ToString());
            }

            return sb.ToString();
        }

        private static string GetString(int decimalValue)
        {
            return new string(new[] { (char)decimalValue });
        }
    }
}
