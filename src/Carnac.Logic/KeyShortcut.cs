using System.Collections.Generic;
using System.Linq;

namespace Carnac.Logic
{
    public class KeyShortcut
    {
        private readonly KeyPressDefinition[] keyCombinations;

        public KeyShortcut(params KeyPressDefinition[] keyCombinations)
        {
            this.keyCombinations = keyCombinations;
        }

        public bool StartsWith(IEnumerable<KeyPressDefinition> chord)
        {
            var index = 0;
            return chord.All(keyPress => keyCombinations.Length > index && keyCombinations[index++].Equals(keyPress));
        }
    }
}