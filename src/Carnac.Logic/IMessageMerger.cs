using Carnac.Logic.Models;

namespace Carnac.Logic
{
    //TODO: Just push logic into Message itself. -LC
    public interface IMessageMerger
    {
        Message MergeIfNeeded(Message previousMessage, Message newMessage);
    }
}