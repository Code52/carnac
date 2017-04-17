using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Models;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Shouldly;
using Xunit;
using Message = Carnac.Logic.Models.Message;
using SettingsProviderNet;
using Carnac.Tests.ViewModels;

namespace Carnac.Tests
{
    public class KeysControllerFacts
    {
        const int MessageAOnNextTick = 100;

        readonly ObservableCollection<Message> messages = new ObservableCollection<Message>();
        readonly TestScheduler testScheduler;
        readonly Message messageA = new Message(A);

        public KeysControllerFacts()
        {
            testScheduler = new TestScheduler();
        }

        KeysController CreateKeysController(IObservable<Message> messageStream)
        {
            var messageProvider = Substitute.For<IMessageProvider>();
            messageProvider.GetMessageStream().Returns(_ => messageStream);
            var concurrencyService = Substitute.For<IConcurrencyService>();
            concurrencyService.MainThreadScheduler.Returns(testScheduler);
            concurrencyService.Default.Returns(testScheduler);

            var settingsService = Substitute.For<ISettingsProvider>();
            var popupSettings = new PopupSettings();
            popupSettings.ItemFadeDelay = GetDefaultFadeDelay(popupSettings);
            settingsService.GetSettings<PopupSettings>().Returns(popupSettings);

            return new KeysController(messages, messageProvider, concurrencyService, settingsService);
        }

        [Fact]
        public void MessagesAreAddedIntoKeysColletion()
        {
            var sut = CreateKeysController(SingleMessageAt100Ticks());
            sut.Start();
            testScheduler.AdvanceTo(MessageAOnNextTick + 1);  //+1 for the cost of the ObserveOn scheduling

            messages.ShouldContain(messageA);
            messageA.IsDeleting.ShouldBe(false);
        }

        [Fact]
        public void MessagesAreFlaggedAsDeletingAfter5Seconds()
        {
            var sut = CreateKeysController(SingleMessageAt100Ticks());
            sut.Start();
            testScheduler.AdvanceBy(MessageAOnNextTick + 1);
            testScheduler.AdvanceBy(5.Seconds());

            Assert.Single(messages);
            messages.Single().ShouldBe(messageA.FadeOut());
        }

        [Fact]
        public void MessagesIsRemovedAfter6Seconds()
        {
            var sut = CreateKeysController(SingleMessageAt100Ticks());
            sut.Start();
            testScheduler.AdvanceBy(MessageAOnNextTick + 2);
            testScheduler.AdvanceBy(6.Seconds());

            messages.ShouldBeEmpty();
        }

        [Fact]
        public void MessageTimeoutIsStartedAgainIfMessageIsUpdated()
        {
            var expected = messageA.Merge(new Message(A));
            var messageSequence = testScheduler.CreateColdObservable(
                ReactiveTest.OnNext(MessageAOnNextTick, messageA),
                ReactiveTest.OnNext(3.Seconds(), expected)
                );

            var sut = CreateKeysController(messageSequence);

            sut.Start();
            testScheduler.AdvanceBy(6.Seconds());

            messages.Single().IsDeleting.ShouldBe(false);
            messages.Single().ShouldBe(expected);
        }

        [Fact]
        public void MultiMerge()
        {
            var message1 = new Message(Down);
            var message2 = message1.Merge(new Message(Down));
            var message3 = message2.Merge(new Message(Down));

            var expected = message3;
            var messageSequence = testScheduler.CreateColdObservable(
                ReactiveTest.OnNext(0.1.Seconds(), message1),
                ReactiveTest.OnNext(0.2.Seconds(), message2),
                ReactiveTest.OnNext(0.3.Seconds(), message3)
                );

            var sut = CreateKeysController(messageSequence);

            sut.Start();
            testScheduler.AdvanceBy(1.Seconds());

            messages.Single().IsDeleting.ShouldBe(false);
            messages.Single().ShouldBe(expected);
        }

        static KeyPress A
        {
            get
            {
                return new KeyPress(new ProcessInfo("foo"),
                    new InterceptKeyEventArgs(Keys.A, KeyDirection.Down, false, false, false), false, new[] { "a" });
            }
        }

        static KeyPress Down
        {
            get
            {
                return new KeyPress(new ProcessInfo("foo"),
                    new InterceptKeyEventArgs(Keys.Down, KeyDirection.Down, false, false, false), false, new[] { "Down" });
            }
        }

        IObservable<Message> SingleMessageAt100Ticks()
        {
            return testScheduler.CreateColdObservable(
               ReactiveTest.OnNext(MessageAOnNextTick, messageA)
               );
        }

        double GetDefaultFadeDelay(PopupSettings settings)
        {
            AttributeCollection attributes =
                TypeDescriptor.GetProperties(settings)["ItemFadeDelay"].Attributes;
            DefaultValueAttribute myAttribute =
                (DefaultValueAttribute)attributes[typeof(DefaultValueAttribute)];

            double fadeDelay;
            double.TryParse(myAttribute.Value.ToString(), out fadeDelay);
            return fadeDelay;
        }

    }
}
