using AdventOfCode.Framework;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static AdventOfCode2022.Day13.Day13;

namespace AdventOfCode2022.Day13
{
    [Solution(13)]
#if RELEASE
    //6038 Incorrect & too low.
    [SolutionInput("Day13\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day13\\InputTest1.txt", Enabled = true)]
#endif
    internal class Day13 : Solution
    {
        public Day13(Input input) : base(input)
        {
        }

        public interface IElement
        {
            public bool IsList { get; }
        }

        public class ElementComparer : IComparer<IElement>
        {
            public int Compare(IElement? x, IElement? y)
            {
                var firstElement = x as Element;
                var secondElement = y as Element;
                if (firstElement != null && secondElement != null)
                {
                    return CompareElements(firstElement, secondElement);
                }

                if (firstElement == null)
                {
                    var firstList = (ElementList)x;
                    if (secondElement != null)
                    {
                        return CompareElementListAndElement(firstList, secondElement);
                    }
                    else
                    {
                        return CompareElementLists(firstList, (ElementList)y);
                    }
                }
                else //firstElement != null, secondElement == null
                {
                    var secondList = (ElementList)y;
                    return CompareElementAndElementList(firstElement, secondList);
                }
            }

            private int CompareElements(Element x, Element y)
            {
                LogToConsole($"Compare {x} vs {y}");
                var comparisonResult = x.Value - y.Value;
                if (comparisonResult < 0)
                {
                    LogToConsole($"Left side is smaller, so inputs are in the right order.");
                }
                else if (comparisonResult > 0)
                {
                    LogToConsole($"Right side is smaller, so inputs are not in the right order.");
                }
                return x.Value - y.Value;
            }

            private int CompareElementListAndElement(ElementList x, Element y)
            {
                var yList = new ElementList();
                yList.Elements.Add(y);
                LogToConsole($"Compare {x} vs {y}");
                var comparisonResult = CompareElementLists(x, yList);
                return comparisonResult;
            }

            private int CompareElementAndElementList(Element x, ElementList y)
            {
                var xList = new ElementList();
                xList.Elements.Add(x);
                LogToConsole($"Compare {x} vs {y}");
                var comparisonResult = CompareElementLists(xList, y);
                return comparisonResult;
            }

            private int CompareElementLists(ElementList x, ElementList y)
            {
                LogToConsole($"Compare {x} vs {y}");
                foreach ((var first, var second) in x.Elements.Zip(y.Elements))
                {
                    var comparison = Compare(first, second);
                    if (comparison != 0)
                    {
                        return comparison;
                    }
                }
                if (x.Elements.Count > y.Elements.Count)
                {
                    LogToConsole("Right side ran out of elements, so inputs are not in the right order");
                    return 1;
                }
                if (y.Elements.Count > x.Elements.Count)
                {
                    LogToConsole("Left side ran out of elements, so inputs are in the right order");
                    return -1;
                }
                return 0;
            }
        }



        public class Element : IElement
        {
            public int Value { get; set; }

            public bool IsList => false;

            public override string ToString()
            {
                return $"{Value}";
            }
        }

        public class ElementList : IElement
        {
            public List<IElement> Elements { get; init; } = new List<IElement>();

            public bool IsList => true;

            public override string ToString()
            {
                StringBuilder s = new();
                s.Append("[");
                for (int i = 0; i < Elements.Count; i++)
                {
                    if (i != 0)
                    {
                        s.Append(",");
                    }
                    var element = Elements[i];
                    s.Append(element.ToString());
                }
                s.Append("]");
                return s.ToString();
            }
        }

        public IEnumerable<(ElementList, ElementList)> ParsePairs(ReadOnlySpan<char> first)
        {
            var pairs = new List<(ElementList, ElementList)>();
            foreach (var pairOfLines in Input.Raw.SplitFast("\r\n\r\n"))
            {
                var pairEnumerator = pairOfLines.SplitFast("\r\n");
                pairEnumerator.MoveNext();
                var firstOfPair = pairEnumerator.Current;
                pairEnumerator.MoveNext();
                var secondOfPair = pairEnumerator.Current;
                pairs.Add(ParsePair(firstOfPair, secondOfPair));
            }
            return pairs;
        }

        public (ElementList, ElementList) ParsePair(ReadOnlySpan<char> first, ReadOnlySpan<char> second)
        {
            //Items are always lists so we can parse them as specific lists.
            first = first[1..];
            second = second[1..];
            var result = (ParseList(ref first), ParseList(ref second));
            return result;
        }

        public IElement ParseElement(ref ReadOnlySpan<char> remaining)
        {
            var result = remaining[0];
            if (result == '[')
            {
                remaining = remaining[1..];
                return ParseList(ref remaining);
            }
            else //must be a number
            {
                remaining = remaining[1..];
                var number = result - 48;
                while (remaining[0] >= '0' && remaining[0] <= '9')
                {
                    result = remaining[0];
                    remaining = remaining[1..];
                    number = number * 10 + result - 48;
                }
                return new Element() { Value = number };
            }
        }

        public ElementList ParseList(ref ReadOnlySpan<char> remaining)
        {
            var elementsList = new ElementList();
            while (remaining[0] != ']')
            {
                var result = remaining[0];
                if (result == '[')
                {
                    remaining = remaining[1..];
                    elementsList.Elements.Add(ParseList(ref remaining));
                }
                else if (result == ',')
                {
                    remaining = remaining[1..];
                    //Nothing to do, we need to parse another element now.
                }
                else
                {
                    elementsList.Elements.Add(ParseElement(ref remaining));
                }
            }
            remaining = remaining[1..];
            //end of list
            return elementsList;
        }


        private static void LogToConsole(object item)
        {
#if DEBUG
            Console.WriteLine(item.ToString());
#endif
        }

        protected override string? Problem1()
        {
            var comparer = new ElementComparer();
            var pairNumber = 1;
            var pairsInOrder = 0;
            foreach (var pair in ParsePairs(Input.Raw))
            {
                var comparison = comparer.Compare(pair.Item1, pair.Item2);
                if (comparison < 0)
                {
                    pairsInOrder += pairNumber;
                }
                pairNumber += 1;

            }
            return pairsInOrder.ToString();
        }

        protected override string? Problem2()
        {
            var comparer = new ElementComparer();
            var allItems = new List<ElementList>();
            var delimiters = ParsePair("[[2]]", "[[6]]");
            foreach (var pair in ParsePairs(Input.Raw).Append(delimiters))
            {
                allItems.Add(pair.Item1);
                allItems.Add(pair.Item2);
            }
            allItems.Sort(comparer);
            var result = 1;
            var index = 1;
            foreach (var item in allItems)
            {
                LogToConsole(item);
                if (ReferenceEquals(item, delimiters.Item1) || ReferenceEquals(item, delimiters.Item2))
                {
                    result *= index;
                }
                index++;
            }
            return result.ToString();
        }
    }
}
