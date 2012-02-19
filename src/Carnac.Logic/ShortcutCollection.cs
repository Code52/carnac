using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public class ShortcutCollection : Collection<KeyShortcut>
    {
        public bool ContainsShortcut(IEnumerable<KeyPress> keys, KeyPress newKeyPress)
        {
            return this.Any(s => s.StartsWith(keys.Concat(new[] { newKeyPress })));
        }
    }
}