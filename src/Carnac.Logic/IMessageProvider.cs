using System;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public interface IMessageProvider
    {
        IObservable<Message> GetMessageStream();
    }
}