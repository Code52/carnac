using System.Collections.Generic;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public interface IShortcutProvider
    {
        bool IsChord(IEnumerable<KeyPress> keys, KeyPress newKeyPress);
    }
}