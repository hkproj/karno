using System;
using System.Collections.Generic;

namespace Karno
{
    public class KMapBenchmark
    {

        readonly Random random;

        const double DC_PROBABILITY = 0.15;
        const double ON_PROBABILITY = 0.3;

        public int NumberOfVariables { get; private set; }
        public int NumberOfTests { get; private set; }

        public KMapBenchmark(int seed, int number_of_variables, int number_of_tests)
        {
            random = new Random(seed);
            NumberOfVariables = number_of_variables;
            NumberOfTests = number_of_tests;
        }

        KMap NewRandomMap()
        {
            var on_set = new HashSet<long>();
            var dc_set = new HashSet<long>();

            var upper_limit = Math.Pow(2, NumberOfVariables);

            for (int n = 0; n < upper_limit; n++)
            {
                var magic = random.NextDouble();
                if (magic < (DC_PROBABILITY + ON_PROBABILITY))
                {
                    if (magic < ON_PROBABILITY)
                        on_set.Add(n);
                    else if (magic < (DC_PROBABILITY + ON_PROBABILITY))
                        dc_set.Add(n);
                }
            }

            if (on_set.Count == 0)
                return NewRandomMap();
            else
                return new KMap(NumberOfVariables, on_set, dc_set);
        }

        public void Run(bool print_results = true, bool test_minimization_result = true)
        {
            double total_time = 0;
            if (print_results)
                Console.WriteLine($"Performing {NumberOfTests} tests with {NumberOfVariables} variables");

            int correct = 0;

            for (int i = 0; i < NumberOfTests; i++)
            {
                if (print_results)
                {
                    Console.WriteLine($"Performing test #{(i + 1)}");
                    Console.WriteLine("\tGenerating map...");
                }
                var map = NewRandomMap();

                if (print_results)
                    Console.WriteLine("\tStarting test...");

                var start_time = DateTime.UtcNow;

                bool test_passed = false;
                if (test_minimization_result)
                {
                    var tester = new KMapTester(map);
                    (test_passed, _, _) = tester.Test(false);
                }
                else
                    map.Minimize();

                var end_time = DateTime.UtcNow;
                var duration = (end_time - start_time).TotalSeconds;
                total_time += duration;

                if (print_results)
                {
                    Console.WriteLine($"\tTook {duration}s");
                }

                if (!print_results || !test_minimization_result || test_passed)
                    correct++;
            }

            if (print_results)
            {
                Console.WriteLine($"Total time: {total_time}s");
                Console.WriteLine($"AVG. time per test: {(total_time / NumberOfTests)}s");
                Console.WriteLine($"Correct {correct} out of {NumberOfTests} tests");
            }
        }

    }
}
