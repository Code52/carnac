using System;
using System.ComponentModel.Composition;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public interface IKeyProvider : IObservable<KeyPress>
    {

    }
}