using AdventOfCode.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2022
{
    [Solution(1)]
#if RELEASE
    [SolutionInput("Day01\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day01\\InputTest.txt", Enabled = true)]
#endif
    internal class Day01 : Solution
    {
        public Day01(Input input) : base(input)
        {
        }

        protected override string? Problem1()
        {
            var groups = Input.Raw.Split("\r\n\r\n");
            var convertedGroups = groups.Select(g => g.Split('\n').Select(x => int.Parse(x)).ToArray()).ToArray();
            int max = 0;
            int groupIndex = 0;
            for (int i = 0; i < convertedGroups.Length; i++)
            {
                var group = convertedGroups[i];
                var sum = group.Sum();
                if (sum > max)
                {
                    max = sum;
                    groupIndex = i;
                }
            }
            return max.ToString();
        }

        protected override string? Problem2()
        {
            var groups = Input.Raw.Split("\r\n\r\n");
            var convertedGroups = groups.Select(g => g.Split('\n').Select(x => int.Parse(x)).ToArray()).ToArray();
            int[] maxes = new int[3];
            int min = 0;
            int minIndex = 0;
            for (int i = 0; i < convertedGroups.Length; i++)
            {
                var group = convertedGroups[i];
                var sum = group.Sum();
                if (sum > min)
                {
                    maxes[minIndex] = sum;
                    min = maxes.Min();
                    minIndex = Array.IndexOf(maxes, min);
                }
            }
            return maxes.Sum().ToString();
        }
    }
}
