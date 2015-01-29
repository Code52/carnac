using System;
using System.ComponentModel.Composition;
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

        public IObservable<Message> GetMessageStream()
        {
            var chords = keyStream
                .Scan(default(KeyPressAccumulator), (acc, key) =>
                {
                    // If our accumulator is complete start a new one
                    if (acc == null || acc.IsComplete)
                    {
                        acc = new KeyPressAccumulator();
                        var possibleShortcuts = shortcutProvider.GetShortcutsStartingWith(key);
                        if (possibleShortcuts.Any())
                            acc.BeginShortcut(key, possibleShortcuts);
                        else
                            acc.Complete(key);

                        return acc;
                    }

                    acc.Add(key);
                    return acc;
                });
            return chords
                .Where(c => c.IsComplete)
                .SelectMany(c => c.GetMessages());
        }

        //private bool ShouldCreateNewMessage(KeyPress value)
        //{
        //    return
        //        CurrentMessage == null ||
        //        IsDifferentProcess(value) ||
        //        IsOlderThanOneSecond() ||
        //        LastKeyPressWasShortcut() ||
        //        value.IsShortcut;
        //}

        //private bool LastKeyPressWasShortcut()
        //{
        //    return CurrentMessage.Keys.Last().IsShortcut;
        //}

        //private bool IsOlderThanOneSecond()
        //{
        //    return CurrentMessage.LastMessage < DateTime.Now.AddSeconds(-1);
        //}

        //private bool IsDifferentProcess(KeyPress value)
        //{
        //    return CurrentMessage.ProcessName != value.Process.ProcessName;
        //}
    }
}