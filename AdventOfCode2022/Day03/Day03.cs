using AdventOfCode.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2022
{
    [Solution(3)]
#if RELEASE
    [SolutionInput("Day03\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day03\\InputTest.txt", Enabled = true)]
#endif
    internal class Day03 : Solution
    {
        public Day03(Input input) : base(input)
        {
        }

        public int ItemPriority(char itemCode)
        {
            if (itemCode >= 'a' && itemCode <= 'z')
            {
                return (int)(itemCode - 'a') + 1;
            }
            else if (itemCode >= 'A' && itemCode <= 'Z')
            {
                return (int)(itemCode - 'A') + 27;
            }
            else throw new NotImplementedException();
        }

        protected override string? Problem1()
        {
            var ruckSacks = Input
                .Raw
                .Split("\r\n")
                .Select(i => (i.Take(i.Length / 2).ToHashSet(), new string(i.Skip(i.Length / 2).ToArray())))
                .ToList();

            int sum = 0;
            foreach (var ruckSack in ruckSacks)
            {
                var firstSack = ruckSack.Item1;
                var secondSack = ruckSack.Item2;
                foreach (var item in secondSack)
                {
                    if (firstSack.Contains(item))
                    {
                        sum += ItemPriority(item);
                        break;
                    }
                }
            }
            return sum.ToString();
        }

        protected override string? Problem2()
        {
            var ruckSacks = Input
                .Raw
                .Split("\r\n")
                .Select(i => i.ToHashSet())
                .ToList();

            int sum = 0;
            for (int i=0; i<ruckSacks.Count; i+=3)
            {
                var first = ruckSacks[i];
                var second = ruckSacks[i+1];
                var third = ruckSacks[i+2];

                foreach (var item in third)
                {
                    if (first.Contains(item) && second.Contains(item))
                    {
                        sum += ItemPriority(item);
                        break;
                    }
                }

            }
            return sum.ToString();
        }
    }
}
