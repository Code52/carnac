using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public interface IMessageMerger
    {
        Message MergeIfNeeded(Message acc, Message key);
    }
}