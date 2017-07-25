using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace Carnac.Logic.Models
{
    public sealed class Message
    {
        readonly ReadOnlyCollection<string> textCollection;
        readonly ReadOnlyCollection<KeyPress> keys;
        readonly string processName;
        readonly ImageSource processIcon;
        readonly string shortcutName;
        readonly bool canBeMerged;
        readonly bool isShortcut;
        readonly bool isModifier;
        readonly bool isDeleting;
        readonly DateTime lastMessage;
        readonly Message previous;

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
            isModifier = key.HasModifierPressed;

            keys = new ReadOnlyCollection<KeyPress>(new[] { key });
            textCollection = new ReadOnlyCollection<string>(CreateTextSequence(key).ToArray());
        }

        public Message(IEnumerable<KeyPress> keys, KeyShortcut shortcut, Boolean isShortcut = false)
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
            this.isShortcut = isShortcut;
            this.isModifier = allKeys.Any(k => k.HasModifierPressed);
            canBeMerged = false;

            this.keys = new ReadOnlyCollection<KeyPress>(allKeys);

            var textSeq = CreateTextSequence(allKeys).ToList();
            if (!string.IsNullOrEmpty(shortcutName))
                textSeq.Add(string.Format(" [{0}]", shortcutName));
            textCollection = new ReadOnlyCollection<string>(textSeq);
        }

        private Message(Message initial, Message appended)
            : this(initial.keys.Concat(appended.keys), new KeyShortcut(initial.ShortcutName))
        {
            previous = initial;
            canBeMerged = true;
        }

        private Message(Message initial, bool isDeleting)
            : this(initial.keys, new KeyShortcut(initial.ShortcutName))
        {
            this.isDeleting = isDeleting;
            previous = initial;
            lastMessage = initial.lastMessage;
        }

        public string ProcessName { get { return processName; } }

        public ImageSource ProcessIcon { get { return processIcon; } }

        public string ShortcutName { get { return shortcutName; } }

        public bool CanBeMerged { get { return canBeMerged; } }

        public bool IsShortcut { get { return isShortcut; } }

        public Message Previous { get { return previous; } }

        public ReadOnlyCollection<string> Text { get { return textCollection; } }

        public DateTime LastMessage { get { return lastMessage; } }

        public bool IsDeleting { get { return isDeleting; } }

        public bool IsModifier { get { return isModifier; } }

        public Message Merge(Message other)
        {
            return new Message(this, other);
        }

        static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

        public static Message MergeIfNeeded(Message previousMessage, Message newMessage)
        {
            return ShouldCreateNewMessage(previousMessage, newMessage)
                ? newMessage
                : previousMessage.Merge(newMessage);
        }

        static bool ShouldCreateNewMessage(Message previous, Message current)
        {
            return previous.ProcessName != current.ProcessName ||
                   current.LastMessage.Subtract(previous.LastMessage) > OneSecond ||
                   !previous.CanBeMerged ||
                   !current.CanBeMerged;
        }

        public Message FadeOut()
        {
            return new Message(this, true);
        }

        static IEnumerable<string> CreateTextSequence(KeyPress key)
        {
            return CreateTextSequence(new[] {key});
        }

        static IEnumerable<string> CreateTextSequence(IEnumerable<KeyPress> keys)
        {
            return keys.Aggregate(new List<RepeatedKeyPress>(),
              (acc, curr) =>
              {
                  if (acc.Any())
                  {
                      var last = acc.Last();
                      if (last.IsRepeatedBy(curr))
                      {
                          last.IncrementRepeat();
                      }
                      else
                      {
                          acc.Add(new RepeatedKeyPress(curr, last.NextRequiresSeperator));
                      }
                  }
                  else
                  {
                      acc.Add(new RepeatedKeyPress(curr));
                  }
                  return acc;
              })
              .SelectMany(rkp => rkp.GetTextParts());
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", ProcessName, string.Join(string.Empty, Text), ShortcutName);
        }

        private sealed class RepeatedKeyPress
        {
            readonly bool requiresPrefix;
            readonly bool nextRequiresSeperator;
            readonly string[] textParts;
            int repeatCount;

            public RepeatedKeyPress(KeyPress keyPress, bool requiresPrefix = false)
            {
                nextRequiresSeperator = keyPress.HasModifierPressed;
                textParts = keyPress.GetTextParts().ToArray();
                this.requiresPrefix = requiresPrefix;
                repeatCount = 1;
            }

            public bool NextRequiresSeperator { get { return nextRequiresSeperator; } }

            public void IncrementRepeat()
            {
                repeatCount++;
            }

            public bool IsRepeatedBy(KeyPress nextKeyPress)
            {
                return textParts.SequenceEqual(nextKeyPress.GetTextParts());
            }

            public IEnumerable<string> GetTextParts()
            {
                if (requiresPrefix)
                    yield return ", ";
                foreach (var textPart in textParts)
                {
                    yield return textPart;
                }
                if (repeatCount > 1)
                    yield return string.Format(" x {0} ", repeatCount);
            }
        }

        #region Equality overrides

        bool Equals(Message other)
        {
            return textCollection.SequenceEqual(other.textCollection)
                && keys.SequenceEqual(other.keys)
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
            if (obj.GetType() != GetType()) return false;
            return Equals((Message)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (textCollection != null ? textCollection.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (keys != null ? keys.GetHashCode() : 0);
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
