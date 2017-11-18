using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Karno
{
    public class KMap
    {

        SortedSet<string> on_set_binary;
        SortedSet<string> dc_set_binary;

        public KMap(int number_of_variables, HashSet<long> on_set, HashSet<long> dc_set)
        {
            if (on_set.Intersect(dc_set).Count() > 0)
                throw new ArgumentException("ON set and DC set must be disjoint");

            if (on_set.Count == 0)
                throw new ArgumentException("ON set can't be empty");

            NumberOfVariables = number_of_variables;
            ONSet = on_set;
            DCSet = dc_set;

            on_set_binary = new SortedSet<string>(ONSet.Select(e => e.ToBinaryString(number_of_variables)));
            dc_set_binary = new SortedSet<string>(DCSet.Select(e => e.ToBinaryString(number_of_variables)));
        }

        public int NumberOfVariables { get; private set; }

        public HashSet<long> ONSet { get; set; }

        public HashSet<long> DCSet { get; set; }

        public HashSet<Coverage> Minimize()
        {
            var groups = new Coverage();

            // Generate initial groups (of cardinality 1)
            foreach (var one in on_set_binary)
                groups.Add(new Group() { one });

            // onsider the don't cares as 'ones', but don't consider them essential
            foreach (var dc in dc_set_binary)
                groups.Add(new Group() { dc });

            while (true)
            {
                // Merge groups of cardinality 'n' to create groups of double cardinality
                var merged_groups = MergeGroups(groups);
                // Clean up groups that are strict subsets of other groups
                merged_groups = RemoveSubsets(merged_groups);

                // If nothing was merged and nothing removed, then we can't optimize anymore
                if (merged_groups.Equals(groups))
                    break;
                else
                    groups = merged_groups;
            }

            // Detect essential groups
            groups = MarkEssential(groups);

            // Remove completely redundant groups
            // (groups that cover ones already covered by other ESSENTIAL groups)
            groups = RemoveRedundant(groups);

            return GetCovers(groups);
        }

        Coverage MergeGroups(Coverage groups)
        {
            var merged = new Coverage();
            foreach(var g1 in groups)
            {
                foreach(var g2 in groups)
                {
                    // Two groups can only merge if they are adjacent and disjoint
                    if (g1.Intersect(g2).Count() == 0 && AreGroupsAdjacent(g1, g2))
                    {
                        var new_group = new Group(g1.Union(g2));
                        merged.Add(new_group);
                    }
                }
            }

            return new Coverage(groups.Union(merged));
        }

        bool AreGroupsAdjacent(Group group1, Group group2)
        {
            // In order to be adjacent, they need to have the same cardinality
            if (group1.Count != group2.Count)
                return false;

            // For each one in the first group, there should be one 'pairing' one in the other
            // such that they have a hamming distance of 1.
            var matched_in_group2 = new Group();
            foreach(var term1 in group1)
            {
                var matched = false;
                foreach(var term2 in group2)
                {

                    if (matched_in_group2.Contains(term2))
                        // This one has already been matched previously, skip it
                        continue;
                    else if (Utils.Hamming(term1, term2) == 1)
                    {
                        matched = true;
                        matched_in_group2.Add(term2);
                        break;
                    }
                }

                if (!matched)
                    // If there is even one "one" in the first group that doesn't
                    // have a matching pair, then the two groups cannot be adjacent
                    return false;
            }

            return true;
        }

        HashSet<Coverage> GetCovers(Coverage groups)
        {
            // Navigate the graph of (possible) solutions, each including or excluding one particular group
            // Each if each solution is valid (i.e. it covers all 'ones')
            var essential = new Coverage(groups.Where(g => g.IsEssential.Value));
            var available_groups_list = groups.Except(essential).OrderBy(g => g.Count);
            return NavigateCovers(essential, available_groups_list);
        }

        HashSet<Coverage> NavigateCovers(Coverage selected_groups, IEnumerable<Group> available_groups_list)
        {
            if (!available_groups_list.Any())
            {
                // No other covers possible, return a coverage including only the selected so far
                Coverage coverage;
                if (IsValidCoverage(selected_groups, out coverage))
                    return new HashSet<Coverage>() { coverage };
                else
                    return new HashSet<Coverage>();
            }

            var next_available = available_groups_list.First();
            var result = new HashSet<Coverage>();

            // Create two covers: the first contains the selected group, the other doesn't.
            var groups_including_next = new Coverage(selected_groups) { next_available };
            if (IsValidCoverage(groups_including_next, out Coverage coverage_including_next))
                result.Add(coverage_including_next);

            var groups_excluding_next = new Coverage(selected_groups);
            if (IsValidCoverage(groups_excluding_next, out Coverage coverage_excluding_next))
                result.Add(coverage_excluding_next);

            var next_available_groups_list = available_groups_list.Skip(1);
            var covers_including_next = NavigateCovers(groups_including_next, next_available_groups_list);
            var covers_excluding_next = NavigateCovers(groups_excluding_next, next_available_groups_list);

            return new HashSet<Coverage>(result.Union(covers_including_next).Union(covers_excluding_next));
        }

        bool IsValidCoverage(Coverage selected_groups, out Coverage coverage)
        {
            coverage = null;
            var on_set = new SortedSet<string>(on_set_binary);

            foreach (var g in selected_groups)
                on_set.ExceptWith(g);

            // A coverage is valid if and only if it covers all the 'ones' of the function
            if (on_set.Any())
                return false;
            else
            {
                coverage = new Coverage(selected_groups) { Cost = CoverageCost(selected_groups) };
                return true;
            }
        }

        private long CoverageCost(Coverage selected_groups)
        {
            // The number of literals in the minterm produced by a group of cardinality 2 ^ n is N - n
            // where N is the number of variables of the boolean function to minimize
            long cost = 0;
            foreach(var g in selected_groups)
            {
                var n = (long)Math.Log(g.Count, 2);
                cost += (NumberOfVariables - n);
            }
            return cost;
        }

        Coverage RemoveSubsets(Coverage groups)
        {
            var not_subsets = new Coverage();
            foreach(var candidate_subset in groups)
            {
                if (!groups.Any(candidate_superset => candidate_subset.IsProperSubsetOf(candidate_superset)))
                    not_subsets.Add(candidate_subset);
            }

            return not_subsets;
        }

        Coverage MarkEssential(Coverage groups)
        {
            // if there is any "one" not covered by any other group
            // then "g1" is an essential group
            foreach(var g1 in groups)
            {
                // Compare this group's terms with all the other groups
                // and remove those covered by the other groups
                var remaining_terms = new Group(g1.Except(dc_set_binary));
                foreach(var g2 in groups)
                {
                    if (!g1.Equals(g2))
                        remaining_terms.ExceptWith(g2);
                }

                // If there is at least one term covered by ONLY this group
                // then it is essential
                if (remaining_terms.Count > 0)
                    g1.IsEssential = true;
                else
                    g1.IsEssential = false;
            }

            return groups;
        }

        Coverage RemoveRedundant(Coverage groups)
        {
            // A group is redundant if it covers "ones" already covered by other
            // essential groups
            var redundant = new Coverage();
            var essential = new Coverage(groups.Where(g => g.IsEssential.Value));

            foreach(var g in groups)
            {
                if (g.IsEssential.Value)
                    continue;

                var remaining_terms = new Group(g);
                foreach (var e in essential)
                    remaining_terms.ExceptWith(e);
                if (remaining_terms.Count == 0)
                    redundant.Add(g);
            }

            return new Coverage(groups.Except(redundant));
        }

    }
}
