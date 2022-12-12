using AdventOfCode.Framework;
using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2022.Day01
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
            int max = 0;
            foreach (var group in Input.Raw.Split("\r\n\r\n"))
            {
                int sum = 0;
                foreach (var groupItem in group.Split("\r\n"))
                {
                    sum += int.Parse(groupItem);
                }
                if (sum > max)
                {
                    max = sum;
                }
            }
            return max.ToString();
        }

        protected override string? Problem2()
        {
            int[] maxes = new int[3];
            int min = 0;
            int minIndex = 0;
            int sum = 0;
            foreach (var group in Input.Raw.SplitFast("\r\n"))
            {
                if (group.Length == 0)
                {
                    if (sum > min)
                    {
                        maxes[minIndex] = sum;
                        min = maxes.Min();
                        minIndex = Array.IndexOf(maxes, min);
                    }
                    sum = 0;
                }
                else
                {
                    sum += int.Parse(group);
                }
            }
            return maxes.Sum().ToString();
        }
    }
}
