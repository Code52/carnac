using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public interface IMessageMerger
    {
        Message MergeIfNeeded(Message accumulatingMessage, Message newMessage);
    }
}