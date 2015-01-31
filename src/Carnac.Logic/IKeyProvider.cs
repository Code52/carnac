using System;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public interface IKeyProvider
    {
        IObservable<KeyPress> GetKeyStream();
    }
}