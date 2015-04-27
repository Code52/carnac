using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Windows.Media;

namespace Carnac.Logic.Models
{
    public class Message
    {
        static readonly string[] RepeatDetectionText = { "Back", "Left", "Right", "Down", "Up" };
        readonly ReadOnlyCollection<string> textCollection;
        readonly ReadOnlyCollection<KeyPress> keyCollection;
        readonly string processName;
        readonly ImageSource processIcon;
        readonly string shortcutName;
        readonly bool canBeMerged;
        readonly bool isShortcut;
        readonly bool isDeleting;
        readonly DateTime lastMessage;

        public Message()
        {
            lastMessage = DateTime.Now;
        }

        public Message(KeyPress key)
            : this()
        {
            processName = key.Process.ProcessName;
            processIcon = key.Process.ProcessIcon;
            canBeMerged = !key.HasModifierPressed;

            keyCollection = new ReadOnlyCollection<KeyPress>(new[] { key });
            textCollection = new ReadOnlyCollection<string>(CreateTextSequence(new[] { key }).ToArray());
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
            shortcutName = shortcut.Name;
            isShortcut = true;
            canBeMerged = false;

            keyCollection = new ReadOnlyCollection<KeyPress>(allKeys);

            var textSeq = CreateTextSequence(allKeys).ToList();
            if (!string.IsNullOrEmpty(shortcutName))
                textSeq.Add(string.Format(" [{0}]", shortcutName));
            textCollection = new ReadOnlyCollection<string>(textSeq.ToArray());
        }

        private Message(Message initial, Message appended)
            : this(initial.Keys.Concat(appended.Keys), new KeyShortcut(initial.ShortcutName))
        {
        }

        private Message(Message initial, bool isDeleting)
            : this(initial.Keys, new KeyShortcut(initial.ShortcutName))
        {
            this.isDeleting = isDeleting;
            lastMessage = initial.lastMessage;
        }

        public string ProcessName { get { return processName; } }

        public ImageSource ProcessIcon { get { return processIcon; } }

        public string ShortcutName { get { return shortcutName; } }

        public bool CanBeMerged { get { return canBeMerged; } }

        public bool IsShortcut { get { return isShortcut; } }

        public ReadOnlyCollection<string> Text { get { return textCollection; } }

        public ReadOnlyCollection<KeyPress> Keys { get { return keyCollection; } }

        //TODO: Rename to timestamp - LC
        public DateTime LastMessage { get { return lastMessage; } }

        public bool IsDeleting { get { return isDeleting; } }

        public Message Merge(Message other)
        {
            return new Message(this, other);
        }

        public Message FadeOut()
        {
            return new Message(this, true);
        }


        static IEnumerable<string> CreateTextSequence(IList<KeyPress> keys)
        {
            return keys.Scan(Tuple.Create<KeyPress, KeyPress>(null, null), (acc, cur) => Tuple.Create(acc.Item2, cur))
                .SelectMany(pair =>
                {
                    var previous = pair.Item1;
                    var current = pair.Item2;
                    var collatedText = Collate(current.Input);
                    var joined = Join(collatedText, "+");
                    var fomatted = joined.Select(txt => Format(txt, current.HasModifierPressed));
                    //if previous has Modifier key pressed, then add ',' to text
                    if (pair.Item1 != null && previous.HasModifierPressed)
                        return fomatted.StartWith(",");
                    return fomatted;
                });
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

        static IEnumerable<string> Collate(IEnumerable<string> textEntries)
        {
            return textEntries.Scan(new List<RepeatedText>(),
                (acc, curr) =>
                {
                    if (acc.Any() && acc.Last().IsRepeatedBy(curr))
                    {
                        acc.Last().IncrementRepeat();
                    }
                    else
                    {
                        acc.Add(new RepeatedText(curr));
                    }
                    return acc;
                })
                .Select(rt => rt.ToString());
        }

        static IEnumerable<T> Join<T>(IEnumerable<T> source, T separator)
        {
            var srcArr = source.ToArray();
            for (int i = 0; i < srcArr.Length; i++)
            {
                yield return srcArr[i];
                if (i < srcArr.Length - 1)
                {
                    yield return separator;
                }
            }
        }

        private sealed class RepeatedText
        {
            readonly string text;
            readonly bool canRepeat;
            int repeatCount;

            public RepeatedText(string text)
            {
                this.text = text;
                canRepeat = RepeatDetectionText.Contains(text);
                repeatCount = 1;
            }

            public void IncrementRepeat()
            {
                repeatCount++;
            }

            public bool IsRepeatedBy(string otherText)
            {
                return canRepeat && text == otherText;
            }

            public override string ToString()
            {
                if (repeatCount == 1) return text;
                return string.Format("{0} x {1} ", text, repeatCount);
            }
        }

        #region Equality overrides
        protected bool Equals(Message other)
        {

            return textCollection.SequenceEqual(other.textCollection) 
                && keyCollection.SequenceEqual(other.keyCollection)
                && string.Equals(processName, other.processName) 
                && Equals(processIcon, other.processIcon) 
                && string.Equals(shortcutName, other.shortcutName) 
                && canBeMerged.Equals(other.canBeMerged) 
                && isShortcut.Equals(other.isShortcut) 
                && isDeleting.Equals(other.isDeleting) 
                && lastMessage.Equals(other.lastMessage);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Message)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (textCollection != null ? textCollection.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (keyCollection != null ? keyCollection.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (processName != null ? processName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (processIcon != null ? processIcon.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (shortcutName != null ? shortcutName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ canBeMerged.GetHashCode();
                hashCode = (hashCode * 397) ^ isShortcut.GetHashCode();
                hashCode = (hashCode * 397) ^ isDeleting.GetHashCode();
                hashCode = (hashCode * 397) ^ lastMessage.GetHashCode();
                return hashCode;
            }
        }
        #endregion
    }
}
