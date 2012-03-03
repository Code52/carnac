using System.Collections.Generic;
using System.Linq;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public class KeyShortcut
    {
        private readonly KeyPressDefinition[] keyCombinations;

        public KeyShortcut(string name, params KeyPressDefinition[] keyCombinations)
        {
            Name = name;
            this.keyCombinations = keyCombinations;
        }

        public string Name { get; private set; }

        public bool StartsWith(IEnumerable<KeyPressDefinition> keyPresses)
        {
            var index = 0;
            return keyPresses.All(keyPress => keyCombinations.Length > index && keyCombinations[index++].Equals(keyPress));
        }

        public bool EndsWith(IEnumerable<KeyPressDefinition> keyPresses)
        {
            var index = keyCombinations.Length - keyPresses.Count();
            bool result = keyPresses.All(keyPress => keyCombinations.Length > index && keyCombinations[index++].Equals(keyPress));
            return result;
        }

        public bool IsMatch(IEnumerable<KeyPress> keyPresses)
        {
            var index = 0;
            return keyPresses.All(keyPress => keyCombinations.Length > index && keyCombinations[index++].Equals(keyPress)) && index == keyCombinations.Length;
       }
    }
}