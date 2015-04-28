using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Models;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Shouldly;
using Xunit;
using Message = Carnac.Logic.Models.Message;

namespace Carnac.Tests
{
    //TODO Create a known sequence of events and test against that. -LC

    public class KeysControllerFacts
    {
        const int MessageAOnNextTick = 100;

        readonly ObservableCollection<Message> keysCollection = new ObservableCollection<Message>();
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
            return new KeysController(keysCollection, messageProvider, concurrencyService);
        }

        [Fact]
        public void MessagesAreAddedIntoKeysColletion()
        {
            var sut = CreateKeysController(SingleMessageAt100Ticks());
            sut.Start();
            testScheduler.AdvanceTo(MessageAOnNextTick + 1);  //+1 for the cost of the ObserveOn scheduling

            keysCollection.ShouldContain(messageA);
            messageA.IsDeleting.ShouldBe(false);
        }

        [Fact]
        public void MessagesAreFlaggedAsDeletingAfter5Seconds()
        {
            var sut = CreateKeysController(SingleMessageAt100Ticks());
            sut.Start();
            testScheduler.AdvanceBy(MessageAOnNextTick + 1);
            testScheduler.AdvanceBy(5.Seconds());

            Assert.Single(keysCollection);
            keysCollection.Single().ShouldBe(messageA.FadeOut());
        }

        [Fact]
        public void MessagesIsRemovedAfter6Seconds()
        {
            var sut = CreateKeysController(SingleMessageAt100Ticks());
            sut.Start();
            testScheduler.AdvanceBy(MessageAOnNextTick + 2);
            testScheduler.AdvanceBy(6.Seconds());

            keysCollection.ShouldBeEmpty();
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

            keysCollection.Single().IsDeleting.ShouldBe(false);
            keysCollection.Single().ShouldBe(expected);
        }

        static KeyPress A
        {
            get
            {
                return new KeyPress(new ProcessInfo("foo"),
                    new InterceptKeyEventArgs(Keys.A, KeyDirection.Down, false, false, false), false, new[] { "a" });
            }
        }

        IObservable<Message> SingleMessageAt100Ticks()
        {
            return testScheduler.CreateColdObservable(
               ReactiveTest.OnNext(MessageAOnNextTick, messageA)
               );
        }
    }
}
