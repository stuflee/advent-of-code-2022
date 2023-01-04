using AdventOfCode.Framework;
using Microsoft.Diagnostics.Runtime.Utilities;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.FrameworkEventSource;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

namespace AdventOfCode2022.Day19
{
    [Solution(21)]
#if RELEASE
    [SolutionInput("Day21\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day21\\Input.txt", Enabled = true)]
    [SolutionInput("Day21\\InputTest.txt", Enabled = false)]
#endif
    internal class Day21 : Solution
    {
        public Day21(Input input) : base(input)
        {
        }

        private static void LogToConsole(object item)
        {
#if DEBUG
            Console.WriteLine(item.ToString());
#endif
        }

        private abstract class BaseNode
        {
            public abstract double Evaluate(Dictionary<string, BaseNode> allNodes);
        }

        private class ValueNode : BaseNode
        {
            public ValueNode(double value)
            {
                Value = value;
            }

            public double Value { get; }

            public override double Evaluate(Dictionary<string, BaseNode> allNodes)
            {
                return Value;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        private class EqualityNode : BaseNode
        {
            public EqualityNode(string operand1, string operand2)
            {
                Operand1 = operand1;
                Operand2 = operand2;
            }

            public string Operand1 { get; }
            public string Operand2 { get; }

            public override double Evaluate(Dictionary<string, BaseNode> allNodes)
            {
                var value1 = allNodes[Operand1].Evaluate(allNodes);
                var value2 = allNodes[Operand2].Evaluate(allNodes);

                return Math.Abs(value1 - value2);
            }

            public override string ToString()
            {
                return $"{Operand1} == {Operand2}";
            }
        }

        private class OperatorNode : BaseNode
        {
            public OperatorNode(string operand1, string operand2, char symbol)
            {
                Operand1 = operand1;
                Operand2 = operand2;
                Symbol = symbol;
            }

            public string Operand1 { get; }
            public string Operand2 { get; }
            public char Symbol { get; }

            public override double Evaluate(Dictionary<string, BaseNode> allNodes)
            {
                var value1 = allNodes[Operand1].Evaluate(allNodes);
                var value2 = allNodes[Operand2].Evaluate(allNodes);
                checked
                {
                    switch (Symbol)
                    {
                        case '+':
                            return value1 + value2;
                        case '-':
                            return value1 - value2;
                        case '/':
                            return value1 / value2;
                        case '*':
                            return value1 * value2;
                        default:
                            throw new Exception("Unknown operand");
                    }
                }
            }

            public override string ToString()
            {
                return $"{Operand1} {Symbol} {Operand2}";
            }
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

        private Dictionary<string, BaseNode> ParseNodes(string text)
        {
            Dictionary<string, BaseNode> nodes = new();
            foreach (var line in text.SplitFast("\r\n"))
            {
                var nodeName = line.Slice(0, 4).ToString();
                var remainder = line.Slice(6);
                if (remainder.Length == 11)
                {
                    var name1 = remainder.Slice(0, 4).ToString();
                    var symbol = remainder.Slice(5, 1);
                    var name2 = remainder.Slice(7, 4).ToString();
                    nodes.Add(nodeName, new OperatorNode(name1, name2, symbol[0]));
                }
                else
                {
                    var value = ParseNumber(ref remainder);
                    nodes.Add(nodeName, new ValueNode(value));
                }
            }
            return nodes;
        }

        protected override string? Problem1()
        {
            var nodes = ParseNodes(Input.Raw);
            var rootNode = nodes["root"];
            return rootNode.Evaluate(nodes).ToString();
        }

        protected override string? Problem2()
        {
            var nodes = ParseNodes(Input.Raw);
            var rootNode = (OperatorNode)nodes["root"];
            var equalityNode = new EqualityNode(rootNode.Operand1, rootNode.Operand2);

            var increment = 1_000_000_000L;
            var value = 0L;
            var direction = 1;
            long result = 0;

            while (true)
            {
                var lowValue = value;
                var highValue = lowValue + direction * increment;
                
                nodes["humn"] = new ValueNode(lowValue);
                var lowResult = equalityNode.Evaluate(nodes);
                if (lowResult < 1e-15)
                {
                    result = lowValue;
                    break;
                }
                nodes["humn"] = new ValueNode(highValue);
                var highResult = equalityNode.Evaluate(nodes);
                if (highResult < 1e-15)
                {
                    result = highValue;
                    break;
                }
                Console.WriteLine(lowResult);
                //Is lowValue or highValue a better approximation?
                if (lowResult < highResult)
                {
                    //lowResult is better.  See if we need to change direction.
                    nodes["humn"] = new ValueNode(lowValue - direction * increment);
                    var lowerResult = equalityNode.Evaluate(nodes);
                    if (lowerResult < lowResult)
                    {
                        direction = -direction;
                    }
                    increment = increment / 2;
                }
                else
                {
                    value = highValue;
                }

            }

            return result.ToString();
        }
    }
}
