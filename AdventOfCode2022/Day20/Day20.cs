using AdventOfCode.Framework;
using Microsoft.Diagnostics.Runtime.Utilities;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.FrameworkEventSource;
using System.Linq;
using System.Management;
using System.Reflection.Metadata.Ecma335;

namespace AdventOfCode2022.Day19
{
    [Solution(20)]
#if RELEASE
    [SolutionInput("Day20\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day20\\InputTest.txt", Enabled = true)]
#endif
    internal class Day20 : Solution
    {
        public Day20(Input input) : base(input)
        {
        }

        private static void LogToConsole(object item)
        {
#if DEBUG
            Console.WriteLine(item.ToString());
#endif
        }

        private int ParseOptionalSign(ref ReadOnlySpan<char> text)
        {
            if (text[0] == '-')
            {
                text = text[1..];
                return -1;
            }
            return 1;
        }

        private int ParseNumber(ref ReadOnlySpan<char> text)
        {
            var sign = ParseOptionalSign(ref text);

            var number = text[0] - '0';
            text = text[1..];
            while (text.Length > 0 && text[0] <= '9' && text[0] >= '0')
            {
                number = number * 10 + text[0] - '0';
                text = text[1..];
            }
            return sign * number;
        }

        private List<int> Parse(string text)
        {
            var result = new List<int>();
            foreach (var line in text.SplitFast("\r\n"))
            {
                var localLine = line;
                result.Add(ParseNumber(ref localLine));
            }
            return result;
        }

        public long ModuloAdvanced(long value, int modulo)
        {
            var newValue = value;
            if (newValue < 0)
            {
                var multiplier = (long)(Math.Abs(newValue) / modulo) + 1;
                newValue = newValue + multiplier * modulo;
            }


            if (newValue >= modulo)
            {
                var divisor = (long)(newValue / modulo);
                var remainder = newValue - modulo * divisor;
                newValue = remainder;
            }
            while (newValue >= modulo)
            {
                newValue -= modulo;
            }
            return newValue;
        }


        protected override string? Problem1()
        {
            var list = Parse(Input.Raw);

            var listAndIndexes = new List<(long, long)>();
            for (int j = 0; j < list.Count; j++)
            {
                listAndIndexes.Add((j, list[j]));
            }
#if DEBUG
            for (int j = 0; j < list.Count; j++)
            {
                Console.Write(list[j]);
                Console.Write(",");
            }
            Console.WriteLine();
            Console.WriteLine();
#endif

            for (int i = 0; i < list.Count; i++)
            {
                var indexOfItemToMove = listAndIndexes.FindIndex(l => l.Item1 == i);
                var newIndex = (long)indexOfItemToMove;

                var item = listAndIndexes[indexOfItemToMove];
                listAndIndexes.RemoveAt(indexOfItemToMove);
                if (item.Item2 > 0)
                {
                    newIndex = ModuloAdvanced(newIndex + item.Item2, listAndIndexes.Count);
                }
                else if (item.Item2 < 0)
                {
                    newIndex = ModuloAdvanced(newIndex + item.Item2, listAndIndexes.Count);
                }
                listAndIndexes.Insert((int)newIndex, item);

                var indexBefore = ModuloAdvanced(newIndex - 1, listAndIndexes.Count);
                var indexAfter = ModuloAdvanced(newIndex + 1, listAndIndexes.Count);
#if DEBUG

                Console.WriteLine($"{item.Item2} moves between {listAndIndexes[(int)indexBefore].Item2} and {listAndIndexes[(int)indexAfter].Item2}");
#endif
                
#if DEBUG
                for (int j = 0; j < list.Count; j++)
                {
                    Console.Write(listAndIndexes[j].Item2);
                    Console.Write(",");
                }
                Console.WriteLine();
                Console.WriteLine();
#endif
            }

            var indexOfZero = listAndIndexes.FindIndex(l => l.Item2 == 0);
            var firstItemIndex = (indexOfZero + 1000) % list.Count;
            var secondItemIndex = (indexOfZero + 2000) % list.Count;
            var thirdItemIndex = (indexOfZero + 3000) % list.Count;
            var firstItem = listAndIndexes[firstItemIndex].Item2;
            var secondItem = listAndIndexes[secondItemIndex].Item2;
            var thirdItem = listAndIndexes[thirdItemIndex].Item2;


            return (firstItem + secondItem + thirdItem).ToString();
        }




        protected override string? Problem2()
        {
            var list = Parse(Input.Raw);
            var decryptionKey = 811589153;

            var listAndIndexes = new List<(long, long)>();
            for (int j = 0; j < list.Count; j++)
            {
                listAndIndexes.Add((j, (long)list[j] * decryptionKey));
            }
#if DEBUG
            for (int j = 0; j < list.Count; j++)
            {
                Console.Write(listAndIndexes[j].Item2);
                Console.Write(",");
            }
            Console.WriteLine();
            Console.WriteLine();
#endif

            for (int k = 0; k < 10; k++)
            {
                for (long i = 0; i < list.Count; i++)
                {
                    var indexOfItemToMove = listAndIndexes.FindIndex(l => l.Item1 == i);
                    var newIndex = (long)indexOfItemToMove;

                    var item = listAndIndexes[indexOfItemToMove];
                    listAndIndexes.RemoveAt(indexOfItemToMove);
                    if (item.Item2 > 0)
                    {
                        newIndex = ModuloAdvanced(newIndex + item.Item2, listAndIndexes.Count);
                        if (newIndex == 0)
                        {
                            newIndex = listAndIndexes.Count;
                        }
                    }
                    else if (item.Item2 < 0)
                    {
                        newIndex = ModuloAdvanced(newIndex + item.Item2, listAndIndexes.Count);
                        if (newIndex == 0)
                        {
                            newIndex = listAndIndexes.Count;
                        }
                    }
                    listAndIndexes.Insert((int)newIndex, item);

                    var indexBefore = ModuloAdvanced(newIndex - 1, listAndIndexes.Count);
                    var indexAfter = ModuloAdvanced(newIndex + 1, listAndIndexes.Count);


                }
#if DEBUG
                for (int j = 0; j < list.Count; j++)
                {
                    Console.Write(listAndIndexes[j].Item2);
                    Console.Write(",");
                }
                Console.WriteLine();
                Console.WriteLine();
#endif            
            }

            var indexOfZero = listAndIndexes.FindIndex(l => l.Item2 == 0);
            var firstItemIndex = (indexOfZero + 1000) % list.Count;
            var secondItemIndex = (indexOfZero + 2000) % list.Count;
            var thirdItemIndex = (indexOfZero + 3000) % list.Count;
            var firstItem = listAndIndexes[firstItemIndex].Item2;
            var secondItem = listAndIndexes[secondItemIndex].Item2;
            var thirdItem = listAndIndexes[thirdItemIndex].Item2;


            return (firstItem + secondItem + thirdItem).ToString();
        }
    }
}
