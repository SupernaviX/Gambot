﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Gambot.Core
{
    public static class Variables
    {
        private static readonly Dictionary<string, Func<string>> magicVariables = new Dictionary<string, Func<string>>();
        private static readonly Regex variableRegex = new Regex(@"\$[a-z][a-z0-9_-]*", RegexOptions.IgnoreCase);

        public static void DefineMagicVariable(string name, Func<string> getter)
        {
            magicVariables.Add(name, getter);
        }

        public static string Substitute(string input)
        {
            return variableRegex.Replace(input, match =>
            {
                var var = match.Value.ToLower();
                if (magicVariables.ContainsKey(var))
                    return magicVariables[var]();

                // TODO: SQL variable lookup

                return match.Value; // ¯\_(ツ)_/¯
            });
        }
    }
}
