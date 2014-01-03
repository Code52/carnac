using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Windows.Forms;
using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Models;
using NSubstitute;
using SettingsProviderNet;
using Xunit;
using Message = Carnac.Logic.Models.Message;

namespace Carnac.Tests
{
    public class MessageProviderFacts : IObserver<Message>
    {
        readonly Subject<InterceptKeyEventArgs> interceptKeysSource;
        readonly IShortcutProvider shortcutProvider;
        readonly MessageProvider messageProvider;
        readonly List<Message> messages = new List<Message>();
        readonly ISettingsProvider settingsProvider;

        public MessageProviderFacts()
        {
            settingsProvider = Substitute.For<ISettingsProvider>();
            settingsProvider.GetSettings<PopupSettings>().Returns(new PopupSettings());
            shortcutProvider = Substitute.For<IShortcutProvider>();
            interceptKeysSource = new Subject<InterceptKeyEventArgs>();
            var keyProvider = new KeyProvider(interceptKeysSource, new PasswordModeService());
            messageProvider = new MessageProvider(keyProvider, shortcutProvider, settingsProvider);
        }

        [Fact]
        public void key_with_modifiers_raises_a_new_message()
        {
            // arrange
            messageProvider.Subscribe(this);
            KeyStreams.LetterL().Play(interceptKeysSource);

            // act
            KeyStreams.CtrlShiftL().Play(interceptKeysSource);

            // assert
            Assert.Equal(2, messages.Count);
        }

        [Fact]
        public void recognises_shortcuts()
        {
            // arrange
            messageProvider.Subscribe(this);
            shortcutProvider.GetShortcutsMatching(Arg.Any<IEnumerable<KeyPress>>())
                .Returns(new []{new KeyShortcut("MyShortcut", new KeyPressDefinition(Keys.L, shiftPressed:true, controlPressed:true))});

            // act
            KeyStreams.CtrlShiftL().Play(interceptKeysSource);

            // assert
            Assert.Equal(1, messages.Count);
            Assert.Equal("MyShortcut", messages[0].ShortcutName);
        }

        [Fact]
        public void does_not_show_shortcut_name_on_partial_match()
        {
            // arrange
            messageProvider.Subscribe(this);
            shortcutProvider.GetShortcutsMatching(Arg.Any<IEnumerable<KeyPress>>())
                .Returns(new[] { new KeyShortcut("SomeShortcut",
                    new KeyPressDefinition(Keys.U, controlPressed: true),
                    new KeyPressDefinition(Keys.L)) });

            // act
            KeyStreams.CtrlU().Play(interceptKeysSource);

            // assert
            Assert.Equal(1, messages.Count);
            Assert.NotEqual("SomeShortcut", messages[0].ShortcutName);
        }

        [Fact]
        public void does_show_shortcut_name_on_full_match()
        {
            // arrange
            messageProvider.Subscribe(this);
            shortcutProvider.GetShortcutsMatching(Arg.Any<IEnumerable<KeyPress>>())
                .Returns(new[] { new KeyShortcut("SomeShortcut",
                    new KeyPressDefinition(Keys.U, controlPressed: true),
                    new KeyPressDefinition(Keys.L)) });

            // act
            KeyStreams.CtrlU().Play(interceptKeysSource);
            KeyStreams.LetterL().Play(interceptKeysSource);

            // assert
            Assert.Equal(1, messages.Count);
            Assert.Equal("SomeShortcut", messages[0].ShortcutName);
        }

        public void OnNext(Message value) { messages.Add(value); }
        public void OnError(Exception error) { }
        public void OnCompleted() { }
    }
}