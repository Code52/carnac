using Carnac.Models;
using Xunit;

namespace Carnac.Tests
{
    public class MessageFacts
    {
        [Fact]
        public void message_groups_multiple_backspace_key_presses_together()
        {
            // arrange
            var message = new Message();
            message.AddText("Back");

            // act
            message.AddText("Back");

            // assert
            Assert.Equal("Back x 2 ", string.Join(string.Empty, message.Text));
        }
    }
}