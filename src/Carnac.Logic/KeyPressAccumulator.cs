using System;
using System.Collections.Generic;
using System.Linq;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public class KeyPressAccumulator
    {
        List<KeyShortcut> possibleKeyShortcuts;
        Message[] messages;
        readonly List<KeyPress> keys;
        DateTime shortcutStartedAt;

        public KeyPressAccumulator()
        {
            keys = new List<KeyPress>();
        }

        public IEnumerable<KeyPress> Keys
        {
            get { return keys; }
        }

        public bool IsComplete { get; private set; }

        public void Add(KeyPress key)
        {
            var isFirstKey = keys.Count == 0;

            if (!isFirstKey && keys[0].Process.ProcessName != key.Process.ProcessName)
            {
                NoMatchingShortcut();
                return;
            }

            keys.Add(key);
            var newPossibleShortcuts = possibleKeyShortcuts.Where(s => s.StartsWith(keys)).ToList();

            if (!newPossibleShortcuts.Any())
                NoMatchingShortcut();
            else if (newPossibleShortcuts.Any(s => s.IsMatch(keys)))
                ShortcutCompleted(newPossibleShortcuts.First(s => s.IsMatch(keys)));
            else
                possibleKeyShortcuts = newPossibleShortcuts;
        }

        public void ShortcutCompleted(KeyShortcut shortcut)
        {
            if (IsComplete)
                throw new InvalidOperationException();

            messages = new[] { new Message(shortcutStartedAt, Keys, shortcut) };
            IsComplete = true;
        }

        public Message[] GetMessages()
        {
            if (!IsComplete)
                throw new InvalidOperationException();

            return messages;
        }

        public void BeginShortcut(KeyPress key, List<KeyShortcut> possibleShortcuts)
        {
            possibleKeyShortcuts = possibleShortcuts;
            keys.Add(key);
            shortcutStartedAt = DateTime.Now;
        }

        /// <summary>
        /// Is not the start of a possbile chord
        /// </summary>
        /// <param name="key"></param>
        public void Complete(KeyPress key)
        {
            if (IsComplete)
                throw new InvalidOperationException();

            IsComplete = true;
            messages = new[] { new Message(key) };
        }

        void NoMatchingShortcut()
        {
            if (IsComplete)
                throw new InvalidOperationException();

            // When we have no matching shortcut just break all key presses into individual messages
            IsComplete = true;
            messages = keys.Select(k => new Message(k)).ToArray();
        }
    }
}