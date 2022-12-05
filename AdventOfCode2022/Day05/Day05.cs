using AdventOfCode.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2022
{
    [Solution(5)]
#if RELEASE
    [SolutionInput("Day05\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day05\\InputTest.txt", Enabled = true)]
#endif
    internal class Day05 : Solution
    {
        public Day05(Input input) : base(input)
        {
        }
      
        private record Move
        {
            public Move(int fromStack, int toStack, int count)
            {
                FromStack = fromStack;
                ToStack = toStack;
                Count = count;
            }

            public int FromStack { get; }
            public int ToStack { get; }
            public int Count { get; }
        }

        private class Crates
        {
            private List<Stack<char>> stacks = new List<Stack<char>>();
            public Crates(int numberOfStacks)
            {
                for (int i=0; i<numberOfStacks; i++)
                {
                    stacks.Add(new Stack<char>());
                }
            }

            public void Add(int stackNumber, char item)
            {
                stacks[stackNumber - 1].Push(item);
            }

            public char Remove(int stackNumber)
            {
                return stacks[stackNumber - 1].Pop();
            }

            public string TopCrates()
            {
                var output = new StringBuilder();
                foreach (var stack in stacks)
                {
                    output.Append(stack.Peek());
                }
                return output.ToString();
            }

            public override string ToString()
            {
                var maxStackSize = stacks.Max(s => s.Count);
                var copies = stacks.Select(s => new Stack<char>(s.ToArray().Reverse())).ToArray();
                var output = new StringBuilder();

                for (int i= maxStackSize; i>0; i--)
                {
                    foreach (var stack in copies)
                    {
                        var item = stack.Count == i
                            ? $"[{stack.Pop()}] " : "    ";
                        
                        output.Append(item);
                    }
                    output.Append(Environment.NewLine);
                }
                for (int i=0; i<copies.Length; i++)
                {
                    output.Append($" {i}  ");
                }
                return output.ToString();
            }
        }

        protected override string? Problem1()
        {
            var cratesAndMoves = Input.Raw.Split("\r\n\r\n");
            var crates = cratesAndMoves[0];
            var crateRows = crates.Split("\r\n");
            var finalRow = crateRows[crateRows.Length - 1];
            var numberOfCrates = finalRow.Split(' ', StringSplitOptions.RemoveEmptyEntries).Count();
            var cratesDefinition = new Crates(numberOfCrates);
            for (int i = crateRows.Length - 2; i >=0; i--)
            {
                var row = crateRows[i];
                var crateIndex = 1;
                for (int j=1; j<row.Length; j+=4)
                {
                    var item = row[j];
                    if (row[j] != ' ')
                    {
                        cratesDefinition.Add(crateIndex, item);
                    }
                    crateIndex++;
                }
            }
            var parsedMoves = new List<Move>();
            foreach (var move in cratesAndMoves[1].Split("\r\n"))
            {
                var moveSplit = move.Split(' ');
                var count = int.Parse(moveSplit[1]);
                var from = int.Parse(moveSplit[3]);
                var to = int.Parse(moveSplit[5]);
                parsedMoves.Add(new Move(from, to, count));
            }

            foreach (var move in parsedMoves)
            {
                for (int i=0; i<move.Count; i++)
                {
                    cratesDefinition.Add(move.ToStack, cratesDefinition.Remove(move.FromStack));
                }
            }

            return cratesDefinition.TopCrates();

        }

        protected override string? Problem2()
        {
            var cratesAndMoves = Input.Raw.Split("\r\n\r\n");
            var crates = cratesAndMoves[0];
            var crateRows = crates.Split("\r\n");
            var finalRow = crateRows[crateRows.Length - 1];
            var numberOfCrates = finalRow.Split(' ', StringSplitOptions.RemoveEmptyEntries).Count();
            var cratesDefinition = new Crates(numberOfCrates);
            for (int i = crateRows.Length - 2; i >= 0; i--)
            {
                var row = crateRows[i];
                var crateIndex = 1;
                for (int j = 1; j < row.Length; j += 4)
                {
                    var item = row[j];
                    if (row[j] != ' ')
                    {
                        cratesDefinition.Add(crateIndex, item);
                    }
                    crateIndex++;
                }
            }
            var parsedMoves = new List<Move>();
            foreach (var move in cratesAndMoves[1].Split("\r\n"))
            {
                var moveSplit = move.Split(' ');
                var count = int.Parse(moveSplit[1]);
                var from = int.Parse(moveSplit[3]);
                var to = int.Parse(moveSplit[5]);
                parsedMoves.Add(new Move(from, to, count));
            }

            foreach (var move in parsedMoves)
            {
                var buffer = new char[move.Count];
                for (int i = 0; i < move.Count; i++)
                {
                    buffer[i] = cratesDefinition.Remove(move.FromStack);
                }
                for (int i = move.Count - 1; i >= 0 ; i--)
                {
                    cratesDefinition.Add(move.ToStack, buffer[i]);
                }
            }

            return cratesDefinition.TopCrates();
        }
    }
}
