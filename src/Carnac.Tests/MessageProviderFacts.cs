using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;
using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Models;
using Microsoft.Win32;
using NSubstitute;
using Xunit;

namespace Carnac.Tests
{
    public class MessageProviderFacts
    {
        readonly IShortcutProvider shortcutProvider;

        public MessageProviderFacts()
        {
            shortcutProvider = Substitute.For<IShortcutProvider>();
            shortcutProvider.GetShortcutsStartingWith(Arg.Any<KeyPress>()).Returns(new List<KeyShortcut>());
            
        }

        MessageProvider CreateMessageProvider(IObservable<InterceptKeyEventArgs> keysStreamSource)
        {
            var source = Substitute.For<IInterceptKeys>();
            source.GetKeyStream().Returns(keysStreamSource);
            var desktopLockEventService = Substitute.For<IDesktopLockEventService>();
            desktopLockEventService.GetSessionSwitchStream().Returns(Observable.Never<SessionSwitchEventArgs>());
            var keyProvider = new KeyProvider(source, new PasswordModeService(), desktopLockEventService);
            return new MessageProvider(shortcutProvider, keyProvider, new PopupSettings(), new MessageMerger());
        }

        [Fact]
        public async void key_with_modifiers_raises_a_new_message()
        {
            // arrange
            var keySequence = KeyStreams.LetterL()
                .Concat(KeyStreams.CtrlShiftL())
                .ToObservable();
            var sut = CreateMessageProvider(keySequence);

            // act
            var messages = await sut.GetMessageStream().ToList();

            // assert
            Assert.Equal(2, messages.Count);
        }

        [Fact]
        public async void recognises_shortcuts()
        {
            // arrange
            var keySequence = KeyStreams.CtrlShiftL().ToObservable();
            var sut = CreateMessageProvider(keySequence);
            shortcutProvider.GetShortcutsStartingWith(Arg.Any<KeyPress>())
                .Returns(new List<KeyShortcut> { new KeyShortcut("MyShortcut", new KeyPressDefinition(Keys.L, shiftPressed: true, controlPressed: true)) });

            // act
            var messages = await sut.GetMessageStream().ToList();

            // assert
            Assert.Equal(1, messages.Count);
            Assert.Equal("MyShortcut", messages[0].ShortcutName);
        }

        [Fact]
        public async void does_not_show_key_press_on_partial_match()
        {
            // arrange
            var keySequence = KeyStreams.CtrlU().ToObservable();
            var sut = CreateMessageProvider(keySequence);
            shortcutProvider.GetShortcutsStartingWith(Arg.Any<KeyPress>())
                .Returns(new List<KeyShortcut> { new KeyShortcut("SomeShortcut",
                    new KeyPressDefinition(Keys.U, controlPressed: true),
                    new KeyPressDefinition(Keys.L)) });

            // act
            var messages = await sut.GetMessageStream().ToList();

            // assert
            Assert.Equal(0, messages.Count);
        }

        [Fact]
        public async void produces_two_messages_when_shortcut_is_broken()
        {
            // arrange
            var keySequence = KeyStreams.CtrlU()
                .Concat(KeyStreams.Number1())
                .ToObservable();
            var sut = CreateMessageProvider(keySequence);
            shortcutProvider.GetShortcutsStartingWith(Arg.Any<KeyPress>())
                .Returns(new List<KeyShortcut> { new KeyShortcut("SomeShortcut",
                    new KeyPressDefinition(Keys.U, controlPressed: true),
                    new KeyPressDefinition(Keys.L)) });

            // act
            var messages = await sut.GetMessageStream().ToList();

            // assert
            Assert.Equal(2, messages.Count);
            Assert.Equal("Ctrl + U", string.Join("", messages[0].Text));
            Assert.Equal("1", string.Join("", messages[1].Text));
        }

        [Fact]
        public async void does_show_shortcut_name_on_full_match()
        {
            // arrange
            var keySequence = KeyStreams.CtrlU()
                .Concat(KeyStreams.LetterL())
                .ToObservable();
            var sut = CreateMessageProvider(keySequence);
            shortcutProvider.GetShortcutsStartingWith(Arg.Any<KeyPress>())
                .Returns(new List<KeyShortcut> { new KeyShortcut("SomeShortcut",
                    new KeyPressDefinition(Keys.U, controlPressed: true),
                    new KeyPressDefinition(Keys.L)) });

            // act
            var messages = await sut.GetMessageStream().ToList();

            // assert
            Assert.Equal(1, messages.Count);
            Assert.Equal("SomeShortcut", messages[0].ShortcutName);
        }

        [Fact]
        public async void keeps_order_of_streams()
        {
            // arrange
            var keySequence = KeyStreams.CtrlU()
                .Concat(KeyStreams.LetterL())
                .Concat(KeyStreams.Number1())
                .Concat(KeyStreams.LetterL())
                .ToObservable();
            var sut = CreateMessageProvider(keySequence);
            shortcutProvider
                .GetShortcutsStartingWith(Arg.Any<KeyPress>())
                .Returns(new List<KeyShortcut> { new KeyShortcut("SomeShortcut",
                    new KeyPressDefinition(Keys.U, controlPressed: true),
                    new KeyPressDefinition(Keys.L)) });

            // act
            var messages = await sut.GetMessageStream().ToList();

            // assert
            Assert.Equal(3, messages.Count);
            Assert.Equal("Ctrl + U, l [SomeShortcut]", string.Join("", messages[0].Text));
            Assert.Equal("SomeShortcut", messages[0].ShortcutName);
            Assert.Equal("1", string.Join("", messages[1].Text));
            Assert.Equal("1l", string.Join("", messages[2].Text));
        }
    }
}