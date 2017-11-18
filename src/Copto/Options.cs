using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Copto
{

    public class ParserException : Exception
    {

        public ParserException(string message) : base(message)
        {

        }

    }

    public class ParserOptions
    {

        public bool CaseSensitive { get; set; } = true;

    }

    /// <summary>
    /// Represents a generic command-line argument
    /// </summary>
    public class Argument
    {

        public string Name { get; set; }

        public int Index { get; set; }

        public List<string> Values { get; set; } = new List<string>();

    }

    public class Options
    {

        static readonly char[] ValueSeparators = new char[] { '=', ':' };
        static readonly char[] ArgumentNamePrefix = new char[] { '-', '/' };

        public static Options Parse(string[] args, ParserOptions parsingOptions = null)
        {
            if (parsingOptions == null)
                parsingOptions = new ParserOptions();

            var options = new Options()
            {
                ParsingOptions = parsingOptions,
            };

            int argumentNum = 0;

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                // Separate argument's name and its value
                int valueSeparatorIndex = arg.IndexOfAny(ValueSeparators);
                bool hasValue = valueSeparatorIndex >= 0;

                // If the name of the argument starts with one of the prefixes, it might require a value
                bool mightRequireValue = arg.StartsWithAny(ArgumentNamePrefix);

                // Read argument name
                string argName = null;
                if (hasValue)
                    argName = arg.Substring(0, valueSeparatorIndex).TrimStart(ArgumentNamePrefix);
                else
                    argName = arg.TrimStart(ArgumentNamePrefix);

                // Now read its value
                string argValue = null;
                if (hasValue)
                    argValue = arg.Substring(valueSeparatorIndex + 1); // The +1 removes the value separator character
                else
                {
                    // Check if the next object in the array is the value of the current argument
                    // We only check if it's prefixed or not.
                    if (mightRequireValue && i != (args.Length - 1) && !args[i + 1].StartsWithAny(ArgumentNamePrefix))
                    {
                        // The next object in the array is the value for the current argument
                        hasValue = true;
                        argValue = args[i + 1];
                        i++; // Skip the next object
                    }
                }

                // Look for an existing argument with the same name
                var existingArgument = options.Arguments.FirstOrDefault(a => string.Compare(a.Name, argName, !options.ParsingOptions.CaseSensitive) == 0);

                // We can't use the "is" operator because it also returns true for instances of inherited types
                if (existingArgument != null && !hasValue)
                    throw new ParserException(string.Format("It's not possible to have arguments with the same name but only some with a value"));

                if (existingArgument == null)
                {
                    options.Arguments.Add(new Argument()
                    {
                        Name = argName,
                        Values = hasValue ? new List<string>() { argValue } : new List<string>(),
                        Index = argumentNum++
                    });
                }
                else
                    // Add the read value to the multivalue argument
                    (existingArgument).Values.Add(argValue);
            }

            return options;
        }

        void ApplyArgument(Argument arg, Delegate callback)
        {
            if (arg.Values.Count == 0)
                callback.DynamicInvoke(new object[] { null });
            else
            {
                foreach (var value in arg.Values)
                {
                    if (callback is Action<string>)
                        callback.DynamicInvoke(value);
                    else if (callback is Action<int?>)
                        callback.DynamicInvoke(int.Parse(value));
                    else if (callback is Action<float?>)
                        callback.DynamicInvoke(float.Parse(value));
                    else if (callback is Action<decimal?>)
                        callback.DynamicInvoke(decimal.Parse(value));
                    else if (callback is Action<double?>)
                        callback.DynamicInvoke(double.Parse(value));
                    else if (callback is Action<DateTime?>)
                        callback.DynamicInvoke(DateTime.Parse(value));
                    else if (callback is Action<bool?>)
                        callback.DynamicInvoke(bool.Parse(value));
                    else
                        throw new NotSupportedException("The parameter of the callback function has an unsupported type.");
                }
            }
        }

        public void Apply(RuleSet rules)
        {
            foreach (var rule in rules)
                ApplyRule(rule.Rule, rule.Callback, rule.Index);
        }

        void ApplyRule(string rule, Delegate callback, int? index = null)
        {
            // Get all the aliases. Prefixes and suffixes are ignored by the matching engine
            var argsNames = rule.TrimStart('-', '/').TrimEnd('=', ':').Split('|');

            // Get all the arguments that have a name contained in the aliases list
            // If an index is specified, only select the arguments with the specified index
            var matching = from arg in Arguments
                           from name in argsNames
                           where string.Compare(arg.Name, name, !ParsingOptions.CaseSensitive) == 0 && (!index.HasValue || arg.Index == index.Value)
                           select arg;

            foreach (var match in matching)
            {
                ApplyArgument(match, callback);
            }
        }

        public Argument this[string name]
        {
            get
            {
                return Arguments.FirstOrDefault(a => string.Compare(a.Name, name) == 0);
            }
        }

        public ParserOptions ParsingOptions { get; set; } = null;

        public List<Argument> Arguments { get; } = new List<Argument>();

    }
}
