using AdventOfCode.Framework;
using Microsoft.CodeAnalysis;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.ExceptionServices;
using System.Text;
using static AdventOfCode2022.Day02.Day02;

namespace AdventOfCode2022.Day10
{
    [Solution(11)]
#if RELEASE
    [SolutionInput("Day11\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day11\\InputTest1.txt", Enabled = true)]
#endif
    internal class Day11 : Solution
    {
        public Day11(Input input) : base(input)
        {
        }

        public enum Operand
        {
            Multiply,
            Add
        }

        public abstract class BaseOperation
        {
            public Operand Operand { get; init; }

            public abstract void Perform(WorryValueBase worryValue);
        }

        public class ParameteredOperation : BaseOperation
        {
            public int OtherValue { get; init; }

            public override void Perform(WorryValueBase worryValue)
            {
                switch (Operand)
                {
                    case Operand.Multiply:
                        {
                            worryValue.Multiply(OtherValue);
                            LogToConsole($"    Worry level is multiplied by {OtherValue} to {worryValue}.");
                            break;
                        }
                    case Operand.Add:
                        {
                            worryValue.Add(OtherValue);
                            LogToConsole($"    Worry level is increased by {OtherValue} to {worryValue}.");
                            break;
                        }
                    default:throw new NotImplementedException();
                }
            }
        }

        public class ParameterlessOperation : BaseOperation
        {
            public Operand Operand { get; init; }

            public override void Perform(WorryValueBase worryValue)
            {
                switch (Operand)
                {
                    case Operand.Multiply:
                        {
                            worryValue.Square();
                            LogToConsole($"    Worry level is multiplied by itself to {worryValue}.");
                            break;
                        }
                    default: throw new NotImplementedException();
                }
            }
        }

        public abstract class WorryValueBase
        {
            public abstract bool IsDivisibleBy(long divisor);

            public abstract void Multiply(long value);

            public abstract void Square();

            public abstract void Add(long value);

            public virtual void Divide(long value)
            {
                throw new NotImplementedException();
            }
        }

        public class WorryValueSimple : WorryValueBase
        {
            public long Value;

            public WorryValueSimple(long value) 
            {
                Value = value;
            }

            public override bool IsDivisibleBy(long divisor)
            {
                return Value % divisor == 0;
            }

            public override void Multiply(long value)
            {
                Value = Value * value;
            }

            public override void Square()
            {
                Value = Value * Value;
            }

            public override void Add(long value)
            {
                Value = Value + value;
            }

            public override void Divide(long value)
            {
                Value = Value / value;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        public class WorryValueComplex : WorryValueBase
        {
            public Dictionary<long, long> FactorsAndModulo = new Dictionary<long, long>();
            public WorryValueComplex(long value, List<long> allFactors) 
            {
                foreach (var factor in allFactors)
                {
                    FactorsAndModulo.Add(factor, value % factor);
                }
            }

            public override bool IsDivisibleBy(long divisor)
            {
                return FactorsAndModulo[divisor] == 0;
            }

            public override void Multiply(long value)
            {
                var factors = FactorsAndModulo.Keys.ToList();
                foreach (var factor in factors)
                {
                    FactorsAndModulo[factor] = (FactorsAndModulo[factor] * value) % factor; 
                }
            }

            public override void Square()
            {
                var factors = FactorsAndModulo.Keys.ToList();
                foreach (var factor in factors)
                {
                    FactorsAndModulo[factor] = (FactorsAndModulo[factor] * FactorsAndModulo[factor]) % factor;
                }
            }

            public override void Add(long value)
            {
                var factors = FactorsAndModulo.Keys.ToList();
                foreach (var factor in factors)
                {
                    FactorsAndModulo[factor] = (FactorsAndModulo[factor] + value) % factor;
                }
            }

            public override string ToString()
            {
                return $"Not Implemented";
            }
        }


        public class Monkey 
        {
            public int Number { get; init; }

            public Queue<WorryValueBase> Items { get; init; } = new Queue<WorryValueBase>();

            public BaseOperation Operation { get; init; }

            public long TestDivisor { get; init; }

            public int MonkeyIfTrue { get; init; }

            public int MonkeyIfFalse { get; init; }

            public long InspectionCount { get; set; } = 0;
        }

        private Monkey ParseMonkey<T>(ReadOnlySpan<char> text, Func<long, T> WorryCreator) where T : WorryValueBase
        { 
            var textEnumerator = text.SplitFast("\r\n");
            textEnumerator.MoveNext();
            
            //There are less than 10 monkies so we don't need length
            var monkeyIndex = int.Parse(textEnumerator.Current.Slice(7, 1));
            textEnumerator.MoveNext();
            
            var startingItemsList = textEnumerator.Current.Slice("  Starting items: ".Length);
            List<T> items = new List<T>();
            foreach (var item in startingItemsList.SplitFast(", "))
            {
                items.Add(WorryCreator(long.Parse(item)));
            }

            textEnumerator.MoveNext();
            var operation = textEnumerator.Current.Slice("  Operation: new = ".Length);
            
            textEnumerator.MoveNext();
            var testValue = int.Parse(textEnumerator.Current.Slice("  Test: divisible by ".Length));
            
            textEnumerator.MoveNext();
            var monkeyIfTrue = int.Parse(textEnumerator.Current.Slice("    If true: throw to monkey ".Length));

            textEnumerator.MoveNext();
            var monkeyIfFalse = int.Parse(textEnumerator.Current.Slice("    If false: throw to monkey ".Length));

            var operand = MemoryExtensions.Equals(operation.Slice("old ".Length, 1), "*", StringComparison.CurrentCultureIgnoreCase)
                ? Operand.Multiply : Operand.Add;

            BaseOperation thisMonkeyOperation;
            if (MemoryExtensions.Equals(operation.Slice(0, 3), "old", StringComparison.CurrentCultureIgnoreCase)
                && MemoryExtensions.Equals(operation.Slice(operation.Length - 3), "old", StringComparison.CurrentCultureIgnoreCase))
            {
                thisMonkeyOperation = new ParameterlessOperation()
                {
                    Operand = operand,
                };
            }
            else
            {
                var parameter = int.Parse(operation.Slice("old ? ".Length));
                thisMonkeyOperation = new ParameteredOperation()
                {
                    Operand = operand,
                    OtherValue = parameter
                };
            }
            return new Monkey()
            {
                Number = monkeyIndex,
                Operation = thisMonkeyOperation,
                Items = new Queue<WorryValueBase>(items),
                MonkeyIfFalse = monkeyIfFalse,
                MonkeyIfTrue = monkeyIfTrue,
                TestDivisor = testValue
            };

        }

        private static void LogToConsole(string message)
        {
            //Console.WriteLine(message);
        }

        protected override string? Problem1()
        {
            var text = Input.Raw;
            List<Monkey> monkies = new List<Monkey>();
            foreach (var monkeyText in text.SplitFast("\r\n\r\n"))
            {
                var monkey = ParseMonkey(monkeyText, value => new WorryValueSimple(value));
                monkies.Add(monkey);
            }

            for (int i=0; i<20; i++)
            {
                foreach (var monkey in monkies)
                {
                    LogToConsole($"Monkey {monkey.Number}:");
                    while (monkey.Items.Count > 0)
                    {
                        var item = monkey.Items.Dequeue();
                        
                        monkey.InspectionCount = monkey.InspectionCount + 1;

                        LogToConsole($"  Monkey inspects and item with a worry level of {item}.");
                        monkey.Operation.Perform(item);
                        item.Divide(3);
                        LogToConsole($"    Monkey gets bored with item. Worry level is divided by 3 to {item}.");

                        int newMonkey;
                        if (item.IsDivisibleBy(monkey.TestDivisor))
                        {
                            LogToConsole($"    Current worry level is divisible by {monkey.TestDivisor}.");
                            newMonkey = monkey.MonkeyIfTrue;
                        }
                        else
                        {
                            LogToConsole($"    Current worry level is not divisible by {monkey.TestDivisor}.");
                            newMonkey = monkey.MonkeyIfFalse;
                        }
                        LogToConsole($"    Item with worry level {item} is thrown to monkey {newMonkey}.");

                        var newMoneyToThrowTo = monkies[newMonkey];
                        newMoneyToThrowTo.Items.Enqueue(item);
                    }
                }

#if DEBUG
                Console.WriteLine($"After round {i + 1}, the monkeys are holding items with these worry levels:");
                foreach (var monkey in monkies)
                {
                    Console.WriteLine($"Monkey {monkey.Number}: {string.Join(", ", monkey.Items)}");
                }
                Console.WriteLine();
#endif
            }

#if DEBUG
            foreach (var monkey in monkies)
            {
                Console.WriteLine($"Monkey {monkey.Number} inspected itemes {monkey.InspectionCount} times");
            }
            Console.WriteLine();
#endif

            var monkeysByMostInspections = monkies.OrderByDescending(m => m.InspectionCount).ToList();


            return (monkeysByMostInspections[0].InspectionCount * monkeysByMostInspections[1].InspectionCount).ToString();
        }


        protected override string? Problem2()
        {
            var text = Input.Raw;
            var monkies = new List<Monkey>();
            foreach (var monkeyText in text.SplitFast("\r\n\r\n"))
            {
                var monkey = ParseMonkey(monkeyText, value => new WorryValueSimple(value));
                monkies.Add(monkey);
            }
            var factors = monkies.Select(m => m.TestDivisor).ToList();

            monkies = new();
            foreach (var monkeyText in text.SplitFast("\r\n\r\n"))
            {
                var monkey = ParseMonkey(monkeyText, value => new WorryValueComplex(value, factors));
                monkies.Add(monkey);
            }

            for (int i = 1; i <= 10000; i++)
            {
                foreach (var monkey in monkies)
                {
                    while (monkey.Items.Count > 0)
                    {
                        var item = monkey.Items.Dequeue();

                        monkey.InspectionCount = monkey.InspectionCount + 1;

                        monkey.Operation.Perform(item);

                        int newMonkey;
                        if (item.IsDivisibleBy(monkey.TestDivisor))
                        {
                            newMonkey = monkey.MonkeyIfTrue;
                        }
                        else
                        {
                            newMonkey = monkey.MonkeyIfFalse;
                        }
                        var newMoneyToThrowTo = monkies[newMonkey];
                        newMoneyToThrowTo.Items.Enqueue(item);
                    }
                }
#if DEBUG
                if (i % 20 == 0 || i % 1000 == 0)
                {
                    Console.WriteLine($"== After round {i} ==");
                    foreach (var monkey in monkies)
                    {
                        Console.WriteLine($"Monkey {monkey.Number} inspected itemes {monkey.InspectionCount} times has {monkey.Items.Count} items");
                    }
                    Console.WriteLine();
                }
#endif
            }

            var monkeysByMostInspections = monkies.OrderByDescending(m => m.InspectionCount).ToList();


            return (monkeysByMostInspections[0].InspectionCount * monkeysByMostInspections[1].InspectionCount).ToString();
        }
    }
}
