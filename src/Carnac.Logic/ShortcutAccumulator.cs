using System;
using System.Collections.Generic;
using System.Linq;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public class ShortcutAccumulator
    {
        List<KeyShortcut> possibleKeyShortcuts;
        Message[] messages;
        readonly List<KeyPress> keys;
        DateTime shortcutStartedAt;

        public ShortcutAccumulator()
        {
            keys = new List<KeyPress>();
        }

        public IEnumerable<KeyPress> Keys
        {
            get { return keys; }
        }

        public bool HasCompletedValue { get; private set; }

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
            if (HasCompletedValue)
                throw new InvalidOperationException();

            messages = new[] { new Message(shortcutStartedAt, Keys, shortcut) };
            HasCompletedValue = true;
        }

        public Message[] GetMessages()
        {
            if (!HasCompletedValue)
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
            if (HasCompletedValue)
                throw new InvalidOperationException();

            HasCompletedValue = true;
            messages = new[] { new Message(key) };
        }

        void NoMatchingShortcut()
        {
            if (HasCompletedValue)
                throw new InvalidOperationException();

            // When we have no matching shortcut just break all key presses into individual messages
            HasCompletedValue = true;
            messages = keys.Select(k => new Message(k)).ToArray();
        }

        public ShortcutAccumulator ProcessKey(IShortcutProvider shortcutProvider, KeyPress key)
        {
            if (!keys.Any() || HasCompletedValue)
            {
                var possibleShortcuts = shortcutProvider.GetShortcutsStartingWith(key);
                if (possibleShortcuts.Any())
                    BeginShortcut(key, possibleShortcuts);
                else
                    Complete(key);

                return this;
            }

            Add(key);
            return this;
        }
    }
}