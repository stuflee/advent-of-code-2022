using AdventOfCode.Framework;
using System.Text;

namespace AdventOfCode2022.Day10
{
    [Solution(10)]
#if RELEASE
    [SolutionInput("Day10\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day10\\InputTest1.txt", Enabled = false)]
    [SolutionInput("Day10\\InputTest2.txt", Enabled = true)]
#endif
    internal class Day10 : Solution
    {
        public Day10(Input input) : base(input)
        {
        }


        private class StateMachine
        {
            public int LineNumber { get; private set; } = 0;
            public int ColumnNumber { get; private set; } = 0;
            public bool[,] Screen { get; } = new bool[6, 40];

            public int ClockCycle { get; private set; }

            //Start of sprite
            public int X { get; private set; } = 1;

            public int SignalStrength { get; private set; } = 0;

            private void AddClockCycle()
            {
                //Problem 2 - Drawing.
                if (LineNumber < 6)
                {
                    if (X - 1 == ColumnNumber
                        || ColumnNumber == X
                        || ColumnNumber == X + 1)
                    {
                        Screen[LineNumber, ColumnNumber] = true;
                    }
                }
                
                ClockCycle++;

                //Problem 2
                ColumnNumber++;
                if (ClockCycle % 40 == 0)
                {
                    LineNumber += 1;
                    ColumnNumber = 0;
                }

                //Problem 1
                if ((ClockCycle + 20) % 40 == 0)
                {
                    SignalStrength += ClockCycle * X;
#if DEBUG
                    Console.WriteLine($"{ClockCycle} {X} {ClockCycle * X}");
#endif
                }
            }

            public void Noop()
            {
                AddClockCycle();
            }

            public void AddX(int x)
            {
                AddClockCycle();
                AddClockCycle();
                X += x;
            }

        }

        protected override string? Problem1()
        {
            var text = Input.Raw;
            var stateMachine = new StateMachine();
            foreach (var command in text.SplitFast("\r\n"))
            {
                if (command.StartsWith("noop"))
                {
                    stateMachine.Noop();
                }
                else
                {
                    var xShift = int.Parse(command.Slice(5));
                    stateMachine.AddX(xShift);
                }
            }
            Console.WriteLine($"Cycles: {stateMachine.ClockCycle}, X: {stateMachine.X} Strength: {stateMachine.SignalStrength}");
            Console.WriteLine();
            return stateMachine.SignalStrength.ToString();
        }


        protected override string? Problem2()
        {
            var text = Input.Raw;
            var stateMachine = new StateMachine();
            foreach (var command in text.SplitFast("\r\n"))
            {
                if (command.StartsWith("noop"))
                {
                    stateMachine.Noop();
                }
                else
                {
                    var xShift = int.Parse(command.Slice(5));
                    stateMachine.AddX(xShift);
                }
            }
            var output = new StringBuilder();
            for (int i=0; i<6; i++)
            {
                for (int j = 0; j < 40; j++)
                {
                    output.Append(stateMachine.Screen[i, j] ? '#' : '.');
                }
                output.AppendLine(string.Empty);
            }

            return output.ToString();
        }
    }
}
