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
    [Solution(22)]
#if RELEASE
    [SolutionInput("Day22\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day22\\InputTest.txt", Enabled = true)]
#endif
    internal class Day22 : Solution
    {
        public Day22(Input input) : base(input)
        {
        }

        private static void LogToConsole(object item)
        {
#if DEBUG
            Console.WriteLine(item.ToString());
#endif
        }

        private enum Direction
        {
            Right = 0,
            Down = 1,
            Left = 2,
            Up = 3,
        }

        private int ParseNumber(ref ReadOnlySpan<char> text)
        {
            var number = text[0] - '0';
            text = text[1..];
            while (text.Length > 0 && text[0] <= '9' && text[0] >= '0')
            {
                number = number * 10 + text[0] - '0';
                text = text[1..];
            }
            return number;
        }

        private Direction ParseRotation(ref ReadOnlySpan<char> text)
        {
            var letter = text[0];
            text = text[1..];
            switch (letter)
            {
                case 'R':
                    return Direction.Right;
                case 'L':
                    return Direction.Left;
                default:
                    throw new Exception();
            }
        }

        private record struct Vector(int x, int y)
        {
            public Vector Add(Vector v)
            {
                return this with { x = this.x + v.x, y = this.y + v.y };
            }
        }

        private record struct Position(Vector v, Direction d);

        private abstract class Instruction
        {
            public abstract Position Apply(Position initialPosition, List<char[]> markers);
        }

        private class RotationInstruction : Instruction
        {
            public RotationInstruction(Direction rotation)
            {
                Rotation = rotation;
            }

            public Direction Rotation { get; }

            public override Position Apply(Position initialPosition, List<char[]> markers)
            {
                if (Rotation == Direction.Left)
                {
                    return RotateLeft(initialPosition);
                }
                return RotateRight(initialPosition);
            }

            private Position RotateRight(Position initialPosition)
            {
                switch (initialPosition.d)
                {
                    case Direction.Left:
                        return initialPosition with { d = Direction.Up };
                    case Direction.Right:
                        return initialPosition with { d = Direction.Down };
                    case Direction.Up:
                        return initialPosition with { d = Direction.Right };
                    case Direction.Down:
                        return initialPosition with { d = Direction.Left };
                    default:
                        throw new Exception();

                }
            }

            private Position RotateLeft(Position initialPosition)
            {
                switch (initialPosition.d)
                {
                    case Direction.Left:
                        return initialPosition with { d = Direction.Down };
                    case Direction.Right:
                        return initialPosition with { d = Direction.Up };
                    case Direction.Up:
                        return initialPosition with { d = Direction.Left };
                    case Direction.Down:
                        return initialPosition with { d = Direction.Right };
                    default:
                        throw new Exception();

                }
            }

            public override string ToString()
            {
                return $"Rotate {Rotation}";
            }
        }

        private class MovementInstruction : Instruction
        {
            public MovementInstruction(int distance)
            {
                Distance = distance;
            }

            public int Distance { get; }

            public override Position Apply(Position initialPosition, List<char[]> markers)
            {
                Vector movementVector;
                switch (initialPosition.d)
                {
                    case Direction.Up:
                        movementVector = new(0, -1);
                        break;
                    case Direction.Down:
                        movementVector = new(0, 1);
                        break;
                    case Direction.Left:
                        movementVector = new(-1, 0);
                        break;
                    case Direction.Right:
                        movementVector = new( 1, 0);
                        break;
                    default:
                        throw new Exception();
                }

                int positionMoved = 0;
                Vector lastValidPosition = initialPosition.v;
                Vector lastPosition = lastValidPosition;
                while (positionMoved < Distance)
                {
                    var nextProposedPosition = lastPosition.Add(movementVector);
                    if (nextProposedPosition.y < 0)
                    {
                        nextProposedPosition = nextProposedPosition with { y = markers.Count - 1 };
                    }
                    if (nextProposedPosition.y > markers.Count - 1)
                    {
                        nextProposedPosition = nextProposedPosition with { y = 0 };
                    }
                    if (nextProposedPosition.x < 0)
                    {
                        nextProposedPosition = nextProposedPosition with { x = markers[0].Length - 1 };
                    }
                    if (nextProposedPosition.x > markers[0].Length - 1)
                    {
                        nextProposedPosition = nextProposedPosition with { x = 0 };
                    }
                    var row = markers[nextProposedPosition.y];
                    var coordinateValue = ' ';
                    if (nextProposedPosition.x < row.Length)
                    {
                        coordinateValue = row[nextProposedPosition.x];
                    }
                    
                    if (coordinateValue == '.')
                    {
                        positionMoved += 1;
                        lastValidPosition = nextProposedPosition;
                    }
                    else if (coordinateValue == '#')
                    {
                        break;
                    }
                    lastPosition = nextProposedPosition;
                }
                return initialPosition with { v = lastValidPosition };
            }

            public override string ToString()
            {
                return $"Move {Distance}";
            }

        }


        private List<Instruction> ParseInstructions(ReadOnlySpan<char> instructions)
        {
            var results = new List<Instruction>();
            var localInstructions = instructions;
            while (localInstructions.Length > 0)
            {
                var distance = ParseNumber(ref localInstructions);
                results.Add(new MovementInstruction(distance));
                if (localInstructions.Length > 0)
                {
                    var rotation = ParseRotation(ref localInstructions);
                    results.Add(new RotationInstruction(rotation));
                }
            }
            return results;
        }

        private List<char[]> ParseMarkers(ReadOnlySpan<char> instructions)
        {
            var results = new List<char[]>();
            foreach (var line in instructions.SplitFast("\r\n"))
            {
                var lineValues = line.ToArray();
                results.Add(lineValues);
            }
            return results;
        }

        protected override string? Problem1()
        {
            var input = Input.Raw;
            var inputComponents = input.Split("\r\n\r\n");

            var markers = ParseMarkers(inputComponents[0]);
            var instructions = ParseInstructions(inputComponents[1]);

            var topRow = markers[0];
            var startIndex = 0;
            while (topRow[startIndex] == ' ')
            {
                startIndex++;
            }
            var position = new Position(new Vector(startIndex, 0), Direction.Right);

            foreach (var instruction in instructions)
            {
                //Console.WriteLine($"Applying {instruction} to {position}");
                var newPosition = instruction.Apply(position, markers);
                //Console.WriteLine($"New Position  {newPosition}");
                position = newPosition;
            }

            return (1000 * (position.v.y + 1) + 4 * (position.v.x + 1) + (int)position.d).ToString();
        }

        protected override string? Problem2()
        {
            return string.Empty;
        }
    }
}
