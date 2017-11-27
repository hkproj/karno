using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karno
{
    public static class Utils
    {

        public static string ToMintermMap(this Group group)
        {
            if (group.Count == 0)
                throw new ArgumentException("Group must contain at least one term");

            // Take the first term and "mark" all variables that change value in the group
            var terms = group.ToList();
            var first_term_list = terms.First().ToList();

            for (int n = 1; n < terms.Count; n++)
            {
                for (int i = 0; i < first_term_list.Count; i++)
                {
                    if (terms[n][i] != first_term_list[i])
                        first_term_list[i] = '-';
                }
            }

            return string.Join("", first_term_list);
        }

        public static string ToSOPExpression(this Coverage coverage)
        {
            var result = new StringBuilder();
            var groups = coverage.OrderBy(g => g.Count).ToList();
            for (int g = 0; g < groups.Count; g++)
            {
                result.Append(groups[g].ToMintermExpression());
                if (g != (groups.Count - 1))
                    result.Append(" + ");
            }

            return result.ToString();
        }

        public static string ToMintermExpression(this Group group)
        {
            var map = group.ToMintermMap();

            var result = new StringBuilder(map.Length);
            for (int i = 0; i < map.Length; i++)
            {
                if (map[i] != '-')
                {
                    var letter = (char)('A' + i);
                    result.Append(letter);
                    result.Append(map[i] == '0' ? "\'" : "");
                }
            }

            return result.ToString();
        }

        public static void PrintCoverage(this Coverage coverage)
        {
            Console.WriteLine($"Coverage: {coverage.Cost.Value}");
            foreach (var g in coverage)
            {
                var terms = g.ToList();
                for (int i = 0; i < terms.Count; i++)
                {
                    if (i != terms.Count - 1)
                        Console.Write($"{terms[i]} - ");
                    else
                        Console.Write(terms[i]);
                }

                if (g.IsEssential.Value)
                    Console.WriteLine(" - Essential");
                else
                    Console.WriteLine("");
            }

            Console.WriteLine("SOP: " + coverage.ToSOPExpression());
        }

        public static void PrintCoverages(this KMap map, bool only_min = true)
        {
            var coverages = map.Minimize();
            if (coverages.Count == 0)
                return;
            var min_cost = coverages.Min(c => c.Cost.Value);
            foreach(var coverage in coverages)
            {
                if (only_min && coverage.Cost.Value > min_cost)
                    continue;
                coverage.PrintCoverage();
            }
        }

        public static void PrintTestResults(this KMap map, bool only_min = false)
        {
            var tester = new KMapTester(map);
            (var result, var n, var coverage) = tester.Test(only_min);
            if (result)
                Console.WriteLine("TEST: OK");
            else
            {
                Console.WriteLine($"TEST: FAILED - with the following coverage:");
                coverage.PrintCoverage();
            }
        }

        public static string ToBinaryString(this long num, int num_bits)
        {
            return Convert.ToString(num, 2).PadLeft(num_bits, '0');
        }

        public static int Hamming(string s1, string s2)
        {
            Debug.Assert(s1.Length == s2.Length);

            int d = 0;
            for (int i = 0; i < s1.Length; i++)
                if (s1[i] != s2[i])
                    d++;
            return d;
        }

    }
}
