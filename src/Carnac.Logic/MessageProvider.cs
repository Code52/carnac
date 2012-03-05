﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Subjects;
using Carnac.Logic.Models;
using Message = Carnac.Logic.Models.Message;

namespace Carnac.Logic
{
    [Export(typeof(IMessageProvider))]
    public class MessageProvider : IMessageProvider, IObserver<KeyPress>
    {
        readonly Subject<Message> subject = new Subject<Message>();
        private readonly IShortcutProvider shortcutProvider;
        private IDisposable keyStream;

        [ImportingConstructor]
        public MessageProvider(IKeyProvider keyProvider, IShortcutProvider shortcutProvider)
        {
            this.shortcutProvider = shortcutProvider;
            keyStream = keyProvider.Subscribe(this);
        }

        public Message CurrentMessage { get; private set; }

        public IDisposable Subscribe(IObserver<Message> observer)
        {
            return subject.Subscribe(observer);
        }

        public void OnNext(KeyPress value)
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
                    return;
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
                    return;
                }
            }

            if (ShouldCreateNewMessage(value))
            {
                message = CreateNewMessage(value);
            }
            else
                message = CurrentMessage ?? CreateNewMessage(value);

            message.AddKey(value);
            message.LastMessage = DateTime.Now;
            message.Count++;
        }

        private Message CreateNewMessage(KeyPress value)
        {
            var message = new Message
                                  {
                                      StartingTime = DateTime.Now,
                                      ProcessName = value.Process.ProcessName
                                  };

            CurrentMessage = message;
            subject.OnNext(message);
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

        public void OnError(Exception error)
        {

        }

        public void OnCompleted()
        {

        }
    }
}