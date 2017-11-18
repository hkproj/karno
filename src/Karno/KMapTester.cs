using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karno
{
    public class KMapTester
    {

        public KMap Map { get; private set; }

        public KMapTester(KMap map)
        {
            Map = map;
        }

        IEnumerable<string> CompileCoverage(Coverage coverage)
        {
            foreach(var group in coverage)
                yield return group.ToMintermMap();
        }

        /// <summary>
        /// Returns true if the minterm evaluates to true for the given input
        /// </summary>
        bool TestInputOnMinterm(string input_string, string minterm_map)
        {
            Debug.Assert(input_string.Length == minterm_map.Length);

            for(int i = 0; i< input_string.Length; i++)
            {
                if (minterm_map[i] == '-')
                    continue;
                else if (input_string[i] != minterm_map[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if at least one minterm in the coverage evaluates to true for the given input
        /// </summary>
        bool TestInputOnCoverage(string input_string, IEnumerable<string> compiled_coverage)
        {
            foreach(var minterm_map in compiled_coverage)
            {
                if (TestInputOnMinterm(input_string, minterm_map))
                    return true;
            }

            return false;
        }

        public async Task<(bool, long?, Coverage)> TestAsync(bool only_min = true)
        {
            var covers = await Map.Minimize();

            if (covers.Count == 0)
                throw new InvalidOperationException("No valid covers in minimization");

            var min_cost = covers.Min(c => c.Cost);
            foreach(var coverage in covers)
            {
                if (only_min && coverage.Cost > min_cost)
                    continue;
                // Cache compiled minterms
                var compiled_coverage = CompileCoverage(coverage);
                // Generate all possible inputs (from 0 to 2^N - 1, where N is the number of variables).
                var upper_limit = (long)Math.Pow(2, Map.NumberOfVariables);

                for (long n = 0; n < upper_limit; n++)
                {
                    // we don't care about the result for this particular input, as it's in the don't care set.
                    if (Map.DCSet.Contains(n))
                        continue;

                    var input_string = n.ToBinaryString(Map.NumberOfVariables);
                    var result = TestInputOnCoverage(input_string, compiled_coverage);
                    // Result should be 'true' only if the input is part of the on_set
                    if ((!result && Map.ONSet.Contains(n)) || (result && !Map.ONSet.Contains(n)))
                        return (false, n, coverage);
                }
            }

            return (true, null, null);
        }

    }
}
