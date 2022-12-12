using AdventOfCode.Framework;
using CommandLine;
using Microsoft.CodeAnalysis;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AdventOfCode2022.Day09
{
    [Solution(9)]
#if RELEASE
    [SolutionInput("Day09\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day09\\InputTest1.txt", Enabled = false)]
    [SolutionInput("Day09\\InputTest2.txt", Enabled = true)]
#endif
    internal class Day09 : Solution
    {
        public Day09(Input input) : base(input)
        {
        }

        private record Position
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        private class SmallRopeTracker
        {
            public SmallRopeTracker(int width, int height)
            {
                Width = width;
                Height = height;
            }

            public Position HeadPosition { get; private set; } = new Position();

            public Position TailPosition { get; private set; } = new Position();

            public Position StartPosition { get; private set; } = new Position();

            public int Width { get; }
            public int Height { get; }

            private HashSet<Position> VisitedPositions = new HashSet<Position>() { new Position() };

            public int VisitedCount => VisitedPositions.Count();

            public void MoveUp()
            {
                HeadPosition = HeadPosition with { Y = HeadPosition.Y + 1 };
                if (HeadPosition.Y - TailPosition.Y == 2)
                {
                    TailPosition = HeadPosition with { Y = HeadPosition.Y - 1 };
                    VisitedPositions.Add(TailPosition);
                }
            }

            public void MoveDown()
            {
                HeadPosition = HeadPosition with { Y = HeadPosition.Y - 1 };
                if (TailPosition.Y - HeadPosition.Y == 2)
                {
                    TailPosition = HeadPosition with { Y = HeadPosition.Y + 1 };
                    VisitedPositions.Add(TailPosition);
                }
            }

            public void MoveLeft()
            {
                HeadPosition = HeadPosition with { X = HeadPosition.X - 1 };
                if (TailPosition.X - HeadPosition.X == 2)
                {
                    TailPosition = HeadPosition with { X = HeadPosition.X + 1 };
                    VisitedPositions.Add(TailPosition);
                }
            }

            public void MoveRight()
            {
                HeadPosition = HeadPosition with { X = HeadPosition.X + 1 };
                if (HeadPosition.X - TailPosition.X == 2)
                {
                    TailPosition = HeadPosition with { X = HeadPosition.X - 1 };
                    VisitedPositions.Add(TailPosition);
                }
            }

            public override string ToString()
            {
                var stringBuilder = new StringBuilder();
                for (int j = Height - 1; j >= 0; j--)
                {
                    for (int i = 0; i < Width; i++)
                    {
                        var thisPosition = new Position() { X = i, Y = j };
                        if (HeadPosition == thisPosition)
                        {
                            stringBuilder.Append('H');
                        }
                        else if (TailPosition == thisPosition)
                        {
                            stringBuilder.Append('T');
                        }
                        else if (StartPosition == thisPosition)
                        {
                            stringBuilder.Append('s');
                        }
                        else
                        {
                            stringBuilder.Append('.');
                        }
                    }
                    stringBuilder.AppendLine(string.Empty);
                }
                return stringBuilder.ToString();
            }

            public string ToVisitedString()
            {
                var stringBuilder = new StringBuilder();
                for (int j = Height - 1; j >= 0; j--)
                {
                    for (int i = 0; i < Width; i++)
                    {
                        var thisPosition = new Position() { X = i, Y = j };
                        if (StartPosition == thisPosition)
                        {
                            stringBuilder.Append('s');
                        }
                        else if (VisitedPositions.Contains(thisPosition))
                        {
                            stringBuilder.Append('#');
                        }
                        else
                        {
                            stringBuilder.Append('.');
                        }
                    }
                    stringBuilder.AppendLine(string.Empty);
                }
                return stringBuilder.ToString();
            }
        }


        private class LargeRopeTracker
        {
            public LargeRopeTracker(int width, int height, int length)
            {
                Width = width;
                Height = height;
                for (int i = 1; i < length; i++)
                {
                    TailPositions.Add(new Position());
                }
            }

            public Position HeadPosition { get; private set; } = new Position();

            public List<Position> TailPositions { get; private set; } = new List<Position>();

            public int Width { get; }
            public int Height { get; }

            private HashSet<Position> VisitedPositions = new HashSet<Position>() { new Position() };

            public int VisitedCount => VisitedPositions.Count();

            private void AdjustPositions()
            {
                var referencePosition = HeadPosition;
                for (int i = 0; i < TailPositions.Count; i++)
                {
                    var adjustedPosition = GetAdjustedPosition(referencePosition, TailPositions[i]);
                    if (adjustedPosition == TailPositions[i])
                    {
                        return;
                    }
                    TailPositions[i] = adjustedPosition;
                    referencePosition = TailPositions[i];
                }
                VisitedPositions.Add(TailPositions[^1]);
            }

            private Position GetAdjustedPosition(Position head, Position tail)
            {
                int xOffset = 0;
                int yOffset = 0;
                if (head.X - tail.X == 2)
                {
                    xOffset = -1;
                }
                if (tail.X - head.X == 2)
                {
                    xOffset = 1;
                }
                if (tail.Y - head.Y == 2)
                {
                    yOffset = 1;
                }
                if (head.Y - tail.Y == 2)
                {
                    yOffset = -1;
                }
                if (xOffset == 0 && yOffset == 0)
                {
                    return tail;
                }
                return head with { X = head.X + xOffset, Y = head.Y + yOffset };
            }

            public void MoveUp()
            {
                HeadPosition = HeadPosition with { Y = HeadPosition.Y + 1 };
                AdjustPositions();
            }

            public void MoveDown()
            {
                HeadPosition = HeadPosition with { Y = HeadPosition.Y - 1 };
                AdjustPositions();
            }

            public void MoveLeft()
            {
                HeadPosition = HeadPosition with { X = HeadPosition.X - 1 };
                AdjustPositions();
            }

            public void MoveRight()
            {
                HeadPosition = HeadPosition with { X = HeadPosition.X + 1 };
                AdjustPositions();
            }

            public override string ToString()
            {
                var stringBuilder = new StringBuilder();
                for (int j = Height - 1; j >= 0; j--)
                {
                    for (int i = 0; i < Width; i++)
                    {
                        var thisPosition = new Position() { X = i, Y = j };
                        char outputValue = '.';
                        for (int t = TailPositions.Count - 1; t >= 0; t--)
                        {
                            if (TailPositions[t] == thisPosition)
                            {
                                outputValue = (t + 1).ToString()[0];
                            }
                        }

                        if (HeadPosition == thisPosition)
                        {
                            outputValue = 'H';
                        }
                        stringBuilder.Append(outputValue);
                    }
                    stringBuilder.AppendLine(string.Empty);
                }
                return stringBuilder.ToString();
            }

            public string ToVisitedString()
            {
                var stringBuilder = new StringBuilder();
                for (int j = Height - 1; j >= 0; j--)
                {
                    for (int i = 0; i < Width; i++)
                    {
                        var thisPosition = new Position() { X = i, Y = j };
                        if (VisitedPositions.Contains(thisPosition))
                        {
                            stringBuilder.Append('#');
                        }
                        else
                        {
                            stringBuilder.Append('.');
                        }
                    }
                    stringBuilder.AppendLine(string.Empty);
                }
                return stringBuilder.ToString();
            }
        }



        protected override string? Problem1()
        {
            var text = Input.Raw;
            var tracker = new SmallRopeTracker(6, 5);
            foreach (var move in text.SplitFast("\r\n"))
            {
                var distance = int.Parse(move.Slice(1));
#if DEBUG
                Console.WriteLine($"== {move[0]} {distance} ==");
                Console.WriteLine();
#endif

                for (int i = 0; i < distance; i++)
                {
                    switch (move[0])
                    {
                        case 'U':
                            tracker.MoveUp();
                            break;
                        case 'D':
                            tracker.MoveDown();
                            break;
                        case 'L':
                            tracker.MoveLeft();
                            break;
                        case 'R':
                            tracker.MoveRight();
                            break;
                    }
#if DEBUG
                    Console.WriteLine(tracker.ToString());
                    Console.WriteLine();
#endif
                }
            }

#if DEBUG
            Console.WriteLine(tracker.ToVisitedString());
            Console.WriteLine();
#endif


            return tracker.VisitedCount.ToString();
        }


        protected override string? Problem2()
        {
            var text = Input.Raw;
            var tracker = new LargeRopeTracker(20, 20, 10);
            foreach (var move in text.SplitFast("\r\n"))
            {
                var distance = int.Parse(move.Slice(1));
#if DEBUG
                Console.WriteLine($"== {move[0]} {distance} ==");
                Console.WriteLine();
#endif

                for (int i = 0; i < distance; i++)
                {
                    switch (move[0])
                    {
                        case 'U':
                            tracker.MoveUp();
                            break;
                        case 'D':
                            tracker.MoveDown();
                            break;
                        case 'L':
                            tracker.MoveLeft();
                            break;
                        case 'R':
                            tracker.MoveRight();
                            break;
                    }
                }
#if DEBUG
                Console.WriteLine(tracker.ToString());
                Console.WriteLine();
#endif
            }

#if DEBUG
            Console.WriteLine(tracker.ToVisitedString());
            Console.WriteLine();
#endif


            return tracker.VisitedCount.ToString();
        }
    }
}
