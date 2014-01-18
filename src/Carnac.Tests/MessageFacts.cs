using System.Windows.Forms;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Models;
using Xunit;
using Message = Carnac.Logic.Models.Message;

namespace Carnac.Tests
{
    public class MessageFacts
    {
        [Fact]
        public void message_does_not_group_different_letters()
        {
            // arrange
            var message = new Message();
            message.AddKey(new KeyPress(null, new InterceptKeyEventArgs(Keys.Back, KeyDirection.Down, false, false, false), false, new[] { "a" }));

            // act
            message.AddKey(new KeyPress(null, new InterceptKeyEventArgs(Keys.Back, KeyDirection.Down, false, false, false), false, new[] { "b" }));

            // assert
            Assert.Equal("ab", string.Join(string.Empty, message.Text));
        }

        [Fact]
        public void message_does_not_group_letter_and_backspace()
        {
            // arrange
            var message = new Message();
            message.AddKey(new KeyPress(null, new InterceptKeyEventArgs(Keys.Back, KeyDirection.Down, false, false, false), false, new[] { "a" }));

            // act
            message.AddKey(new KeyPress(null, new InterceptKeyEventArgs(Keys.Back, KeyDirection.Down, false, false, false), false, new[] { "Back" }));

            // assert
            Assert.Equal("aBack", string.Join(string.Empty, message.Text));
        }

        [Fact]
        public void message_groups_multiple_backspace_key_presses_together()
        {
            // arrange
            var message = new Message();
            message.AddKey(new KeyPress(null, new InterceptKeyEventArgs(Keys.Back, KeyDirection.Down, false, false, false), false, new[]{"Back"}));

            // act
            message.AddKey(new KeyPress(null, new InterceptKeyEventArgs(Keys.Back, KeyDirection.Down, false, false, false), false, new[] { "Back" }));

            // assert
            Assert.Equal("Back x 2 ", string.Join(string.Empty, message.Text));
        }

        [Fact]
        public void message_does_not_group_different_arrow_key_presses_together()
        {
            // arrange
            var message = new Message();
            message.AddKey(new KeyPress(null, new InterceptKeyEventArgs(Keys.Up, KeyDirection.Down, false, false, false), false, new[] { "Up" }));

            // act
            message.AddKey(new KeyPress(null, new InterceptKeyEventArgs(Keys.Down, KeyDirection.Down, false, false, false), false, new[] { "Down" }));

            // assert
            Assert.Equal("↑↓", string.Join(string.Empty, message.Text));
        }

        [Fact]
        public void message_groups_multiple_equal_arrow_key_presses_together()
        {
            // arrange
            var message = new Message();
            message.AddKey(new KeyPress(null, new InterceptKeyEventArgs(Keys.Down, KeyDirection.Down, false, false, false), false, new[] { "Down" }));

            // act
            message.AddKey(new KeyPress(null, new InterceptKeyEventArgs(Keys.Down, KeyDirection.Down, false, false, false), false, new[] { "Down" }));

            // assert
            Assert.Equal("↓ x 2 ", string.Join(string.Empty, message.Text));
        }

        [Fact]
        public void multiple_shortcuts_have_comma_inserted_between_input()
        {
            // arrange
            var message = new Message();
            message.AddKey(new KeyPress(null, new InterceptKeyEventArgs(Keys.R, KeyDirection.Down, false, true, false), false, new[] { "Control", "R" }));

            // act
            message.AddKey(new KeyPress(null, new InterceptKeyEventArgs(Keys.T, KeyDirection.Down, false, true, false), false, new[] { "Control", "T" }));

            // assert
            Assert.Equal("Control + R, Control + T", string.Join(string.Empty, message.Text));
        }
    }
}