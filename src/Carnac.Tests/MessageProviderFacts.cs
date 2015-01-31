using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Forms;
using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Models;
using Microsoft.Win32;
using NSubstitute;
using SettingsProviderNet;
using Xunit;
using Message = Carnac.Logic.Models.Message;

namespace Carnac.Tests
{
    public class MessageProviderFacts
    {
        readonly Subject<InterceptKeyEventArgs> interceptKeysSource;
        readonly IShortcutProvider shortcutProvider;
        readonly MessageProvider messageProvider;
        readonly List<Message> messages = new List<Message>();

        public MessageProviderFacts()
        {
            var settingsProvider = Substitute.For<ISettingsProvider>();
            settingsProvider.GetSettings<PopupSettings>().Returns(new PopupSettings());
            shortcutProvider = Substitute.For<IShortcutProvider>();
            shortcutProvider.GetShortcutsStartingWith(Arg.Any<KeyPress>()).Returns(new List<KeyShortcut>());
            interceptKeysSource = new Subject<InterceptKeyEventArgs>();
            var source = Substitute.For<IInterceptKeys>();
            source.GetKeyStream().Returns(interceptKeysSource);
            var desktopLockEventService = Substitute.For<IDesktopLockEventService>();
            desktopLockEventService.GetSessionSwitchStream().Returns(Observable.Never<SessionSwitchEventArgs>());
            var keyProvider = new KeyProvider(source, new PasswordModeService(), desktopLockEventService);
            messageProvider = new MessageProvider(keyProvider, shortcutProvider, settingsProvider, new MessageMerger());
        }

        [Fact]
        public void key_with_modifiers_raises_a_new_message()
        {
            // arrange
            messageProvider.GetMessageStream().Subscribe(value => messages.Add(value));
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
            messageProvider.GetMessageStream().Subscribe(value => messages.Add(value));
            shortcutProvider.GetShortcutsStartingWith(Arg.Any<KeyPress>())
                .Returns(new List<KeyShortcut> { new KeyShortcut("MyShortcut", new KeyPressDefinition(Keys.L, shiftPressed: true, controlPressed: true)) });

            // act
            KeyStreams.CtrlShiftL().Play(interceptKeysSource);

            // assert
            Assert.Equal(1, messages.Count);
            Assert.Equal("MyShortcut", messages[0].ShortcutName);
        }

        [Fact]
        public void does_not_show_key_press_on_partial_match()
        {
            // arrange
            messageProvider.GetMessageStream().Subscribe(value => messages.Add(value));
            shortcutProvider.GetShortcutsStartingWith(Arg.Any<KeyPress>())
                .Returns(new List<KeyShortcut> { new KeyShortcut("SomeShortcut",
                    new KeyPressDefinition(Keys.U, controlPressed: true),
                    new KeyPressDefinition(Keys.L)) });

            // act
            KeyStreams.CtrlU().Play(interceptKeysSource);

            // assert
            Assert.Equal(0, messages.Count);
        }

        [Fact]
        public void produces_two_messages_when_shortcut_is_broken()
        {
            // arrange
            messageProvider.GetMessageStream().Subscribe(value => messages.Add(value));
            shortcutProvider.GetShortcutsStartingWith(Arg.Any<KeyPress>())
                .Returns(new List<KeyShortcut> { new KeyShortcut("SomeShortcut",
                    new KeyPressDefinition(Keys.U, controlPressed: true),
                    new KeyPressDefinition(Keys.L)) });

            // act
            KeyStreams.CtrlU().Play(interceptKeysSource);
            KeyStreams.Number1().Play(interceptKeysSource);

            // assert
            Assert.Equal(2, messages.Count);
            Assert.Equal("Ctrl + U", string.Join("", messages[0].Text));
            Assert.Equal("1", string.Join("", messages[1].Text));
        }

        [Fact]
        public void does_show_shortcut_name_on_full_match()
        {
            // arrange
            messageProvider.GetMessageStream().Subscribe(value => messages.Add(value));
            shortcutProvider.GetShortcutsStartingWith(Arg.Any<KeyPress>())
                .Returns(new List<KeyShortcut> { new KeyShortcut("SomeShortcut",
                    new KeyPressDefinition(Keys.U, controlPressed: true),
                    new KeyPressDefinition(Keys.L)) });

            // act
            KeyStreams.CtrlU().Play(interceptKeysSource);
            KeyStreams.LetterL().Play(interceptKeysSource);

            // assert
            Assert.Equal(1, messages.Count);
            Assert.Equal("SomeShortcut", messages[0].ShortcutName);
        }

        [Fact]
        public void keeps_order_of_streams()
        {
            // arrange
            messageProvider.GetMessageStream().Subscribe(value => messages.Add(value));
            shortcutProvider
                .GetShortcutsStartingWith(Arg.Any<KeyPress>())
                .Returns(new List<KeyShortcut> { new KeyShortcut("SomeShortcut",
                    new KeyPressDefinition(Keys.U, controlPressed: true),
                    new KeyPressDefinition(Keys.L)) });

            // act
            KeyStreams.CtrlU().Play(interceptKeysSource);
            KeyStreams.LetterL().Play(interceptKeysSource);
            KeyStreams.Number1().Play(interceptKeysSource);
            KeyStreams.LetterL().Play(interceptKeysSource);

            // assert
            Assert.Equal(2, messages.Count);
            Assert.Equal("Ctrl + U, l [SomeShortcut]", string.Join("", messages[0].Text));
            Assert.Equal("SomeShortcut", messages[0].ShortcutName);
            Assert.Equal("1l", string.Join("", messages[1].Text));
        }
    }
}