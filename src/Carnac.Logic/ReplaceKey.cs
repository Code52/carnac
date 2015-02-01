using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Carnac.Logic
{
    public static class ReplaceKey
    {
        static readonly Dictionary<Keys, string> ShiftReplacements = new Dictionary<Keys, string>
        {
            {Keys.D0, ")"},
            {Keys.D1, "!"},
            {Keys.D2, "@"},
            {Keys.D3, "#"},
            {Keys.D4, "$"},
            {Keys.D5, "%"},
            {Keys.D6, "^"},
            {Keys.D7, "&"},
            {Keys.D8, "*"},
            {Keys.D9, "("},
            {Keys.OemOpenBrackets, "{"},
            {Keys.Oem6, "}"},
            {Keys.OemMinus, "_"},
            {Keys.Oemplus, "+"},
            {Keys.OemBackslash, "|"},
            {Keys.Oem5, "|"},
            {Keys.OemQuestion, "?"},
            {Keys.OemPeriod, ">"},
            {Keys.Oemcomma, "<"},
            {Keys.Oem1, ":"},
            {Keys.Oem7, "\""},
            {Keys.Oemtilde, "~"},
            {Keys.Insert, "ins"},
            {Keys.Delete, "del"}
        };

        static readonly Dictionary<Keys, string> Replacements = new Dictionary<Keys, string>
        {
            {Keys.Space, " "},
            {Keys.D0, "0"},
            {Keys.D1, "1"},
            {Keys.D2, "2"},
            {Keys.D3, "3"},
            {Keys.D4, "4"},
            {Keys.D5, "5"},
            {Keys.D6, "6"},
            {Keys.D7, "7"},
            {Keys.D8, "8"},
            {Keys.D9, "9"},
            {Keys.NumPad0, "0"},
            {Keys.NumPad1, "1"},
            {Keys.NumPad2, "2"},
            {Keys.NumPad3, "3"},
            {Keys.NumPad4, "4"},
            {Keys.NumPad5, "5"},
            {Keys.NumPad6, "6"},
            {Keys.NumPad7, "7"},
            {Keys.NumPad8, "8"},
            {Keys.NumPad9, "9"},
            {Keys.OemOpenBrackets, "["},
            {Keys.Oem6, "]"},
            {Keys.OemMinus, "-"},
            {Keys.Oemplus, "="},
            {Keys.Oem5, "\\"},
            {Keys.OemBackslash, "\\"},
            {Keys.OemQuestion, "/"},
            {Keys.OemPeriod, "."},
            {Keys.Oemcomma, ","},
            {Keys.Oem1, ";"},
            {Keys.Oem7, "'"},
            {Keys.Oemtilde, "`"},
            {Keys.Decimal, "."},
            {Keys.Divide, " / "},
            {Keys.Multiply, " * "},
            {Keys.Subtract, " - "},
            {Keys.Add, " + "},
            {Keys.LShiftKey, "Shift"},
            {Keys.RShiftKey, "Shift"},
            {Keys.LWin, "Win"},
            {Keys.RWin, "Win"},
        };

        public static Keys? ToKey(string keyText)
        {
            foreach (var shiftReplacement in ShiftReplacements)
            {
                if (shiftReplacement.Value.Equals(keyText, StringComparison.CurrentCultureIgnoreCase))
                    return shiftReplacement.Key;
            }
            Keys parsedKey;
            if (Enum.TryParse(keyText, true, out parsedKey))
                return parsedKey;

            foreach (var replacement in Replacements)
            {
                if (replacement.Value.Equals(keyText, StringComparison.CurrentCultureIgnoreCase))
                    return replacement.Key;
            }
            return null;
        }

        public static string Sanitise(this Keys key)
        {
            return Replacements.ContainsKey(key) ? Replacements[key] : string.Format(key.ToString());
        }

        public static bool SanitiseShift(this Keys key, out string sanitisedKeyInput)
        {
            if (ShiftReplacements.ContainsKey(key))
            {
                sanitisedKeyInput = ShiftReplacements[key];
                return true;
            }

            sanitisedKeyInput = key.Sanitise();
            return false;
        }
    }
}