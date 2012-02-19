using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Xunit;

namespace Carnac.Tests
{
    public class ExampleTest
    {
        [Fact]
        public void ctrlshiftl_is_processed_correctly()
        {
            // arrange
            var player = CtrlShiftL();
            var provider = new KeyProvider(player);

            // act
            var processedKeys = ToEnumerable(provider, player);

            // assert
            Assert.Equal(new[]{"Ctrl", "Shift", "L"}, processedKeys.Single().Input);
        }

        [Fact]
        public void shift_is_not_outputted_when_is_being_used_as_a_modifier_key()
        {
            // arrange
            var player = ShiftL();
            var provider = new KeyProvider(player);

            // act
            var processedKeys = ToEnumerable(provider, player);

            // assert
            Assert.Equal(new[] { "L" }, processedKeys.Single().Input);
        }

        [Fact]
        public void key_without_shift_is_lowercase()
        {
            // arrange
            var player = LetterL();
            var provider = new KeyProvider(player);

            // act
            var processedKeys = ToEnumerable(provider, player);

            // assert
            Assert.Equal(new[] { "l" }, processedKeys.Single().Input);
        }

        [Fact]
        public void verify_number()
        {
            // arrange
            var player = Number1();
            var provider = new KeyProvider(player);

            // act
            var processedKeys = ToEnumerable(provider, player);

            // assert
            Assert.Equal(new[] { "1" }, processedKeys.Single().Input);
        }

        [Fact]
        public void verify_shift_number()
        {
            // arrange
            var player = ExclaimationMark();
            var provider = new KeyProvider(player);

            // act
            var processedKeys = ToEnumerable(provider, player);

            // assert
            Assert.Equal(new[] { "!" }, processedKeys.Single().Input);
        }

        private IEnumerable<KeyPress> ToEnumerable(KeyProvider provider, KeyPlayer player)
        {
            var keys = new List<KeyPress>();

            provider.Subscribe(keys.Add);
            player.Play();

            return keys;
        }

        private static KeyPlayer ExclaimationMark()
        {
            return new KeyPlayer
                       {
                           new InterceptKeyEventArgs(Keys.LShiftKey, KeyDirection.Down, false, false, false),
                           new InterceptKeyEventArgs(Keys.D1, KeyDirection.Down, false, false, true),
                           new InterceptKeyEventArgs(Keys.D1, KeyDirection.Up, false, false, true),
                           new InterceptKeyEventArgs(Keys.LShiftKey, KeyDirection.Up, false, false, true),
                       };
        }

        private static KeyPlayer Number1()
        {
            return new KeyPlayer
                       {
                           new InterceptKeyEventArgs(Keys.D1, KeyDirection.Down, false, false, false),
                           new InterceptKeyEventArgs(Keys.D1, KeyDirection.Up, false, false, false)
                       };
        }

        private KeyPlayer LetterL()
        {
            return new KeyPlayer
                           {
                               new InterceptKeyEventArgs(Keys.L, KeyDirection.Down, false, false, false),
                               new InterceptKeyEventArgs(Keys.L, KeyDirection.Up, false, false, false),
                           };
        }

        private KeyPlayer ShiftL()
        {
            return new KeyPlayer
                           {
                               new InterceptKeyEventArgs(Keys.LShiftKey, KeyDirection.Down, false, false, false),
                               new InterceptKeyEventArgs(Keys.L, KeyDirection.Down, false, false, true),
                               new InterceptKeyEventArgs(Keys.L, KeyDirection.Up, false, false, true),
                               new InterceptKeyEventArgs(Keys.LShiftKey, KeyDirection.Up, false, false, true),
                           };
        }

        private KeyPlayer CtrlShiftL()
        {
            return new KeyPlayer
                           {
                               new InterceptKeyEventArgs(Keys.LControlKey, KeyDirection.Down, false, false, false),
                               new InterceptKeyEventArgs(Keys.LShiftKey, KeyDirection.Down, false, true, false),
                               new InterceptKeyEventArgs(Keys.L, KeyDirection.Down, false, true, true),
                               new InterceptKeyEventArgs(Keys.L, KeyDirection.Up, false, true, true),
                               new InterceptKeyEventArgs(Keys.LShiftKey, KeyDirection.Up, false, true, true),
                               new InterceptKeyEventArgs(Keys.LControlKey, KeyDirection.Up, false, true, false)
                           };
        }
    }
}