using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using Carnac.Logic.Models;
using SettingsProviderNet;

namespace Carnac.Logic
{
    [Export(typeof(IMessageProvider))]
    public class MessageProvider : IMessageProvider
    {
        private readonly IShortcutProvider shortcutProvider;
        private readonly IObservable<KeyPress> keyStream;
        private readonly PopupSettings settings;

        [ImportingConstructor]
        public MessageProvider(IKeyProvider keyProvider, IShortcutProvider shortcutProvider, ISettingsProvider settingsProvider)
        {
            this.shortcutProvider = shortcutProvider;
            settings = settingsProvider.GetSettings<PopupSettings>();
            keyStream = keyProvider.GetKeyStream();
        }

        public Message CurrentMessage { get; private set; }

        public IObservable<Message> GetMessageStream()
        {
            return keyStream.Select(OnNext).DistinctUntilChanged();
        }

        public Message OnNext(KeyPress value)
        {
            Message message;

            var currentKeyPress = new[] {value};
            var keyPresses = CurrentMessage == null ? currentKeyPress : CurrentMessage.Keys.Concat(currentKeyPress).ToArray();
            var possibleShortcuts = GetPossibleShortcuts(keyPresses).ToList();
            if (possibleShortcuts.Any())
            {
                var shortcut = possibleShortcuts.FirstOrDefault(s => s.IsMatch(keyPresses));
                if (shortcut != null)
                {
                    message = CurrentMessage ?? CreateNewMessage(value);
                    message.AddKey(value);
                    message.ShortcutName = shortcut.Name;
                    message.LastMessage = DateTime.Now;
                    message.Count++;
                    //Have duplicated as it was easier for now, this should be cleaned up
                    return CurrentMessage;
                }
            }

            // Haven't matched a Chord, try just the last keypress
            var keyShortcuts = GetPossibleShortcuts(currentKeyPress).ToList();
            if (keyShortcuts.Any())
            {
                var shortcut = keyShortcuts.FirstOrDefault(s => s.IsMatch(currentKeyPress));
                if (shortcut != null)
                {
                    //For matching last keypress, we want a new message
                    message = CreateNewMessage(value);
                    message.AddKey(value);
                    message.ShortcutName = shortcut.Name;
                    message.LastMessage = DateTime.Now;
                    message.Count++;
                    //Have duplicated as it was easier for now, this should be cleaned up
                    return CurrentMessage;
                }
            }

            if (!value.IsShortcut && settings.DetectShortcutsOnly)
                return CurrentMessage;
            
            if (ShouldCreateNewMessage(value))
            {
                message = CreateNewMessage(value);
            }
            else
                message = CurrentMessage ?? CreateNewMessage(value);

            message.AddKey(value);
            message.LastMessage = DateTime.Now;
            message.Count++;

            return CurrentMessage;
        }

        private Message CreateNewMessage(KeyPress value)
        {
            var message = new Message
                                  {
                                      StartingTime = DateTime.Now,
                                      ProcessName = value.Process.ProcessName
                                  };

            CurrentMessage = message;
            return message;
        }

        private bool ShouldCreateNewMessage(KeyPress value)
        {
            return
                CurrentMessage == null ||
                IsDifferentProcess(value) ||
                IsOlderThanOneSecond() ||
                LastKeyPressWasShortcut() ||
                value.IsShortcut;
        }

        private bool LastKeyPressWasShortcut()
        {
            return CurrentMessage.Keys.Last().IsShortcut;
        }

        private IEnumerable<KeyShortcut> GetPossibleShortcuts(IEnumerable<KeyPress> keyPresses)
        {
            return shortcutProvider.GetShortcutsMatching(keyPresses);
        }

        private bool IsOlderThanOneSecond()
        {
            return CurrentMessage.LastMessage < DateTime.Now.AddSeconds(-1);
        }

        private bool IsDifferentProcess(KeyPress value)
        {
            return CurrentMessage.ProcessName != value.Process.ProcessName;
        }
    }
}