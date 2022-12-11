using AdventOfCode.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2022
{
    [Solution(6)]
#if RELEASE
    [SolutionInput("Day06\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day06\\InputTest1.txt", Enabled = true)]
    [SolutionInput("Day06\\InputTest2.txt", Enabled = true)]
    [SolutionInput("Day06\\InputTest3.txt", Enabled = true)]
    [SolutionInput("Day06\\InputTest4.txt", Enabled = true)]
    [SolutionInput("Day06\\InputTest5.txt", Enabled = true)]
#endif
    internal class Day06 : Solution
    {
        public Day06(Input input) : base(input)
        {
        }
      

        protected override string? Problem1()
        {
            var text = Input.Raw;

            var input = text.AsSpan();
            var set = new HashSet<char>(4);
            for (int i=0; i<input.Length; i++)
            {
                var thisWindow = input.Slice(i, 4);
                set.Clear();
                bool itemsAllUnique = true;
                foreach (char c in thisWindow)
                {
                    itemsAllUnique &= set.Add(c);
                }
                if (itemsAllUnique)
                {
                    return (i + 4).ToString();
                }
            }
            throw new Exception();
        }

        protected override string? Problem2()
        {
            var text = Input.Raw;
            Queue<char> queue = new Queue<char>();
            for (int i = 0; i < text.Length; i++)
            {
                if (queue.Count == 14)
                {
                    queue.Dequeue();
                }
                queue.Enqueue(text[i]);
                if (queue.Count == 14 && queue.ToHashSet().Count == 14)
                {
                    return (i + 1).ToString();
                }
            }
            throw new Exception();
        }
    }
}
