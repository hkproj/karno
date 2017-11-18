using Copto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karno
{
    class Program
    {

        static async Task FastTestAsync()
        {
            var map = new KMap(4, new HashSet<long>() { 0, 1, 2, 4, 5, 9, 10, 11, 13, 15 }, new HashSet<long>() { });
            await map.PrintCoversAsync(true);
            await map.PrintTestResultsAsync();

            Console.WriteLine(new string('=', 100));

            map = new KMap(4, new HashSet<long>() { 0, 2, 4, 5, 10, 11, 13, 15 }, new HashSet<long>() { });
            await map.PrintCoversAsync(true);
            await map.PrintTestResultsAsync();

            Console.WriteLine(new string('=', 100));

            map = new KMap(8, new HashSet<long>() { 1, 5, 7, 9, 13, 17, 21, 23, 25, 29, 31, 33, 37, 39, 41, 45, 49, 53, 55, 57, 61, 65, 69, 71, 73, 77, 81, 85, 87, 89, 93, 95, 97, 101, 103, 105, 109, 113, 117, 119, 121, 125, 127, 129, 133, 135, 137, 141, 145, 149, 151, 153, 157, 159, 161, 165, 167, 169, 173, 177, 181, 183, 185, 189, 193, 197, 199, 201, 205, 209, 213, 215, 217, 221, 223, 225, 229, 231, 233, 237, 241, 245, 247, 249, 253 }, new HashSet<long>() { });
            await map.PrintCoversAsync(true);
            await map.PrintTestResultsAsync();

            Console.WriteLine(new string('=', 100));

            map = new KMap(3, new HashSet<long>() { 0, 1, 2, 3, 7 }, new HashSet<long>() { });
            await map.PrintCoversAsync(true);
            await map.PrintTestResultsAsync();

            Console.WriteLine(new string('=', 100));

            map = new KMap(4, new HashSet<long>() { 1, 11, 12, 13, 14, 15 }, new HashSet<long>() { 3, 4, 5, 9 });
            await map.PrintCoversAsync(true);
            await map.PrintTestResultsAsync();

            Console.WriteLine(new string('=', 100));

            map = new KMap(5, new HashSet<long>() { 2, 5, 7, 8, 10, 13, 15, 17, 19, 21, 23, 24, 29, 31 }, new HashSet<long>() { });
            await map.PrintCoversAsync(true);
            await map.PrintTestResultsAsync();
        }

        public static void Main(string[] args)
        {
            var opts = Options.Parse(args);

            int number_of_variables = 50;
            int number_of_tests = 10;
            int seed = 42;
            bool test_minimization_result = true;

            opts.Apply(new RuleSet()
            {
                { "vars", (v) => number_of_variables = v ?? 4 },
                { "tests", (t) => number_of_tests = t ?? 1000 },
                { "reseed", () => seed = (int)DateTime.Now.Ticks },
                { "test-minimization", (v) => test_minimization_result = v ?? true }
            });

            var benchmark = new KMapBenchmark(42, number_of_variables, number_of_tests);
            benchmark.RunAsync().Wait();
            Console.ReadLine();
        }
    }
}
