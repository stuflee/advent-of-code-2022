using AdventOfCode.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2022.Day04
{
    [Solution(4)]
#if RELEASE
    [SolutionInput("Day04\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day04\\InputTest.txt", Enabled = true)]
#endif
    internal class Day04 : Solution
    {
        public Day04(Input input) : base(input)
        {
        }

        protected override string? Problem1()
        {
            var pairs = Input
                .Raw
                .Split("\r\n")
                .Select(x =>
                {
                    var y = x.Split(',');
                    var firstRange = y[0].Split('-');
                    var secondRange = y[1].Split('-');

                    return (
                        (int.Parse(firstRange[0]), int.Parse(firstRange[1])),
                        (int.Parse(secondRange[0]), int.Parse(secondRange[1])));
                })
                .ToList();

            int count = 0;
            foreach (var pair in pairs)
            {
                if (pair.Item1.Item1 <= pair.Item2.Item1 && pair.Item1.Item2 >= pair.Item2.Item2
                    || pair.Item1.Item1 >= pair.Item2.Item1 && pair.Item1.Item2 <= pair.Item2.Item2)
                {
                    count++;
                }
            }
            return count.ToString();
        }

        protected override string? Problem2()
        {
            var pairs = Input
                .Raw
                .Split("\r\n")
                .Select(x =>
                {
                    var y = x.Split(',');
                    var firstRange = y[0].Split('-');
                    var secondRange = y[1].Split('-');

                    return (
                        (int.Parse(firstRange[0]), int.Parse(firstRange[1])),
                        (int.Parse(secondRange[0]), int.Parse(secondRange[1])));
                })
                .ToList();

            int count = 0;
            foreach (var pair in pairs)
            {
                if (
                    pair.Item1.Item1 <= pair.Item2.Item1 && pair.Item1.Item2 >= pair.Item2.Item1
                    || pair.Item1.Item1 <= pair.Item2.Item2 && pair.Item1.Item2 >= pair.Item2.Item2
                    || pair.Item2.Item1 <= pair.Item1.Item1 && pair.Item2.Item2 >= pair.Item1.Item1
                    || pair.Item2.Item1 <= pair.Item1.Item2 && pair.Item2.Item2 >= pair.Item1.Item2
                    )
                {
                    count++;
                }
            }
            return count.ToString();
        }
    }
}
