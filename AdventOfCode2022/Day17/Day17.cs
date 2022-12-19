using AdventOfCode.Framework;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Diagnostics.Tracing.Parsers.FrameworkEventSource;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static AdventOfCode2022.Day13.Day13;

namespace AdventOfCode2022.Day17
{
    [Solution(17)]
#if RELEASE
    //1585673352422
    [SolutionInput("Day17\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    //[SolutionInput("Day17\\Input.txt", Enabled = true)]
    [SolutionInput("Day17\\InputTest.txt", Enabled = true)]
#endif
    internal class Day17 : Solution
    {
        public Day17(Input input) : base(input)
        {
        }

        private static void LogToConsole(object item)
        {
#if DEBUG
            Console.WriteLine(item.ToString());
#endif
        }

        public enum Direction
        {
            Left,
            Down,
            Right
        }

        private record struct Location(int X, int Y);

        private abstract class IShape
        {
            public Location[] Points { get; init; }

            public IShape Move(Direction direction)
            {
                var movement = direction switch
                {
                    Direction.Left => new Location(-1, 0),
                    Direction.Right => new Location(1, 0),
                    Direction.Down => new Location(0, -1),
                    _ => throw new Exception()
                };

                var newPoints = Points.Select(p => new Location(p.X + movement.X, p.Y + movement.Y)).ToArray();
                return FromPoints(newPoints);
            }

            protected abstract IShape FromPoints(Location[] points);
        }

        private class RowShape : IShape
        {
            public RowShape(int lowestInitialY)
            {
                Points = new Location[]
                {
                    new Location(2, lowestInitialY),
                    new Location(3, lowestInitialY),
                    new Location(4, lowestInitialY),
                    new Location(5, lowestInitialY)
                };
            }
            private RowShape(Location[] points)
            {
                Points = points;
            }

            protected override IShape FromPoints(Location[] points)
            {
                return new RowShape(points);
            }
        }

        private class CrossShape : IShape
        {
            public CrossShape(int lowestInitialY)
            {
                Points = new Location[]
                {
                    new Location(3, lowestInitialY),
                    new Location(2, lowestInitialY + 1),
                    new Location(3, lowestInitialY + 1),
                    new Location(4, lowestInitialY + 1),
                    new Location(3, lowestInitialY + 2)
                };
        }

            private CrossShape(Location[] points)
            {
                Points = points;
            }

            protected override IShape FromPoints(Location[] points)
            {
                return new CrossShape(points);
            }
        }

        private class LShape : IShape
        {
            public LShape(int lowestInitialY)
            {
                Points = new Location[] 
                {
                    new Location(2, lowestInitialY),
                    new Location(3, lowestInitialY),
                    new Location(4, lowestInitialY),
                    new Location(4, lowestInitialY + 1),
                    new Location(4, lowestInitialY + 2),
                };
            }

            private LShape(Location[] points)
            {
                Points = points;
            }

            protected override IShape FromPoints(Location[] points)
            {
                return new LShape(points);
            }
        }

        private class ColumnShape : IShape
        {
            public ColumnShape(int lowestInitialY)
            {
                Points = new Location[]
                {
                    new Location(2, lowestInitialY),
                    new Location(2, lowestInitialY + 1),
                    new Location(2, lowestInitialY + 2),
                    new Location(2, lowestInitialY + 3),
                };
            }

            private ColumnShape(Location[] points)
            {
                Points = points;
            }

            protected override IShape FromPoints(Location[] points)
            {
                return new ColumnShape(points);
            }
        }

        private class SquareShape : IShape
        {
            public SquareShape(int lowestInitialY)
            {
                Points = new Location[]
                {
                    new Location(2, lowestInitialY),
                    new Location(3, lowestInitialY),
                    new Location(2, lowestInitialY + 1),
                    new Location(3, lowestInitialY + 1),
                };
            }
            private SquareShape(Location[] points)
            {
                Points = points;
            }

            protected override IShape FromPoints(Location[] points)
            {
                return new SquareShape(points);
            }

        }

        private class ShapeFactory
        {
            public int ShapeIndex { get; private set; } = 0;

            public IShape GetNextShape(int lowestInitialY)
            {
                var nextShapeIndex = ShapeIndex;
                ShapeIndex = (ShapeIndex + 1) % 5;

                return nextShapeIndex switch
                {
                    0 => new RowShape(lowestInitialY),
                    1 => new CrossShape(lowestInitialY),
                    2 => new LShape(lowestInitialY),
                    3 => new ColumnShape(lowestInitialY),
                    4 => new SquareShape(lowestInitialY),
                    _ => throw new InvalidOperationException()

                };
            }
        }

        private class JetFactory
        {
            private readonly List<Direction> movements;
            public int DirectionIndex { get; private set; }
            public JetFactory(List<Direction> movements)
            {
                this.movements = movements;
            }

            public Direction GetNextMovement()
            {
                var index = DirectionIndex;
                var movement = movements[index];
                DirectionIndex = (index + 1) % movements.Count;
                return movement;
            }
        }

        private List<Direction> Parse(string text)
        {
            List<Direction> results = new List<Direction>();
            foreach (char value in text)
            {
                if (value == '\r' || value == '\n')
                {
                    break;
                }
                var direction = value switch
                {
                    '<' => Direction.Left,
                    '>' => Direction.Right,
                    _ => throw new Exception()
                };
                results.Add(direction);
            }
            return results;
        }

        private void Print(HashSet<Location> locations, IShape? shape, int shapeIndex)
        {
#if false
            if (shapeIndex <= 10)
            {
                var maxY = shape == null ? locations.Max(p => p.Y) : shape.Points.Max(p => p.Y);
                for (long y = maxY; y > 0; y--)
                {
                    Console.Write('|');
                    for (long x = 0; x <= 6; x++)
                    {
                        var location = new Location(x, y);
                        if (shape != null && shape.Points.Contains(location))
                        {
                            Console.Write('@');
                        }
                        else if (locations.Contains(location))
                        {
                            Console.Write('#');
                        }
                        else
                        {
                            Console.Write('.');
                        }
                    }
                    Console.WriteLine('|');
                }
                Console.WriteLine("+-------+");
                Console.WriteLine();
            }
#endif
        }

        private void Print2(List<int[]> rows, IShape? shape, long shapeIndex)
        {
#if false
            if (shapeIndex <= 10)
            {
                var maxY = shape == null ? rows.Count() : shape.Points.Max(p => p.Y);
                for (long y = maxY; y >= 0; y--)
                {
                    Console.Write('|');
                    for (long x = 0; x <= 6; x++)
                    {
                        var location = new Location(x, y);
                        if (shape != null && shape.Points.Contains(location))
                        {
                            Console.Write('@');
                        }
                        else if (rows.Count > location.Y && rows[(int)location.Y][(int)location.X] == 1)
                        {
                            Console.Write('#');
                        }
                        else
                        {
                            Console.Write('.');
                        }
                    }
                    Console.WriteLine('|');
                }
                Console.WriteLine("+-------+");
                Console.WriteLine();
            }
#endif
        }


        protected override string? Problem1()
        {
            int numberOfShapes = 2022;
            int minAllowedX = 0;
            int maxAllowedX = 6;
            HashSet<Location> locations = new HashSet<Location>();
            var shapeFactory = new ShapeFactory();
            var jetFactory = new JetFactory(Parse(Input.Raw));
            var highestFilledY = 0;
            for (int i=0; i<numberOfShapes; i++)
            {
                bool shapeAtRest = false;
                var shape = shapeFactory.GetNextShape(highestFilledY + 4);

                Print(locations, shape, i);
                
                while (!shapeAtRest)
                {
                    var movement = jetFactory.GetNextMovement();
                    var proposedShape = shape.Move(movement);

                    if (proposedShape.Points.All(p => p.X >= minAllowedX && p.X <= maxAllowedX && !locations.Contains(p)))
                    {
                        shape = proposedShape;
                    }
                    Print(locations, shape, i);

                    proposedShape = shape.Move(Direction.Down);
                    if (proposedShape.Points.All(p => p.Y > 0 && !locations.Contains(p)))
                    {
                        shape = proposedShape;
                        Print(locations, shape, i);
                    }
                    else
                    {
                        foreach (var point in shape.Points)
                        { 
                            locations.Add(point);
                        }
                        highestFilledY = Math.Max(shape.Points.Max(p => p.Y), highestFilledY);
                        shapeAtRest = true;
                        Print(locations, null, i);
                    }
                }
            }

            return locations.Max(l => l.Y).ToString();
        }


        private List<int[]> PruneRows(List<int[]> rows)
        {
            if (rows.Count > 2000)
            {
                var newRows = new List<int[]>(2000);
                for (int i= rows.Count - 1000; i <= rows.Count - 1; i++)
                {
                    newRows.Add(rows[i]);
                }
                return newRows;
            }
            return rows;
        }

        private record TrackedData(long shapeNumber, long numberOfRows, long timesFound, long rowsDifference, long shapeDifference);

        protected override string? Problem2()
        {
            long numberOfShapes = 1000000000000;
            int minAllowedX = 0;
            int maxAllowedX = 6;
            List<int[]> rows = new List<int[]>();
            rows.Add(Enumerable.Range(0, 7).Select(i => 0).ToArray());
            
            var rowsRemoved = 0L;
            var firstEmptyY = 0;

            var shapeFactory = new ShapeFactory();
            var jetFactory = new JetFactory(Parse(Input.Raw));
            var trackedDataByShapeAndDirectionIndexes = new Dictionary<(int, int), TrackedData>();
            var trackedIndex = (-1, -1);

            for (long i = 0; i < numberOfShapes; i++)
            {
                var newRows = PruneRows(rows);
                rowsRemoved += rows.Count - newRows.Count;
                firstEmptyY -= rows.Count - newRows.Count;
                rows = newRows;


                if (trackedIndex.Item1 == -1 && trackedIndex.Item2 == -1)
                {
                    if (trackedDataByShapeAndDirectionIndexes.TryGetValue((shapeFactory.ShapeIndex, jetFactory.DirectionIndex), out var value))
                    {
                        var rowDifference = (rows.Count + rowsRemoved + 1) - value.numberOfRows;
                        var shapeDifference = i - value.shapeNumber;
                        
                        trackedDataByShapeAndDirectionIndexes[(shapeFactory.ShapeIndex, jetFactory.DirectionIndex)] = 
                            new TrackedData(i, (rows.Count + rowsRemoved + 1), value.timesFound + 1, rowDifference, shapeDifference);

                        //Repeating group.
                        if (rowDifference == value.rowsDifference && shapeDifference == value.shapeDifference)
                        {
                            trackedIndex = (shapeFactory.ShapeIndex, jetFactory.DirectionIndex);
                        }
                    }
                    else
                    {
                        trackedDataByShapeAndDirectionIndexes.Add((shapeFactory.ShapeIndex, jetFactory.DirectionIndex), new TrackedData(i, (rows.Count + rowsRemoved + 1), 1, 0, 0));
                    }
                }
                else if (trackedIndex == (shapeFactory.ShapeIndex, jetFactory.DirectionIndex))
                {
                    var trackedData = trackedDataByShapeAndDirectionIndexes[(shapeFactory.ShapeIndex, jetFactory.DirectionIndex)];
                    var rowDifference = (rows.Count + rowsRemoved + 1) - trackedData.numberOfRows;
                    var shapeDifference = i - trackedData.shapeNumber;

                    var targetI = numberOfShapes - 1;
                    var shapesToAdd = (targetI - i) / shapeDifference;

                    rowsRemoved = rowsRemoved + shapesToAdd * trackedData.rowsDifference;
                    i = i + shapesToAdd * shapeDifference;
                }

                bool shapeAtRest = false;
                var shape = shapeFactory.GetNextShape(firstEmptyY + 3);
                Print2(rows, shape, i);

                //We can always move down twice.  We can always process the first direction.
                var movement = jetFactory.GetNextMovement();
                shape = shape.Move(movement);
                shape = shape.Move(Direction.Down);

                
                if (movement == Direction.Left)
                {
                    movement = jetFactory.GetNextMovement();
                    shape = shape.Move(movement);
                    shape = shape.Move(Direction.Down);
                }

                while (!shapeAtRest)
                {
                    movement = jetFactory.GetNextMovement();
                    var proposedShape = shape.Move(movement);

                    bool shapeCanMove = true;
                    foreach (var point in proposedShape.Points)
                    {
                        if (point.X < minAllowedX || point.X > maxAllowedX)
                        {
                            shapeCanMove = false;
                            break;
                        }

                        if (rows.Count > point.Y && rows[point.Y][point.X] == 1)
                        {
                            shapeCanMove = false;
                            break;
                        }
                    }
                    if (shapeCanMove)
                    {
                        shape = proposedShape;
                    }
                    Print2(rows, shape, (int)i);


                    proposedShape = shape.Move(Direction.Down);

                    shapeCanMove = true;
                    foreach (var point in proposedShape.Points)
                    {
                        if (point.X < minAllowedX || point.X > maxAllowedX)
                        {
                            shapeCanMove = false;
                            break;
                        }

                        if (point.Y == -1)
                        {
                            shapeCanMove = false;
                            break;
}

                        if (rows.Count > point.Y && rows[point.Y][point.X] == 1)
                        {
                            shapeCanMove = false;
                            break;
                        }
                    }
                    if (shapeCanMove)
                    {
                        shape = proposedShape;
                        Print2(rows, shape, (int)i);
                    }
                    else
                    {
                        for (int j=0; j<shape.Points.Length;j++)
                        {
                            var point = shape.Points[j];
                            while (point.Y >= rows.Count )
                            {
                                rows.Add(new int[7]);
                            }
                            rows[(int)point.Y][point.X] = 1;
                            if (point.Y == firstEmptyY)
                            {
                                firstEmptyY++;
                            }

                        }
                        shapeAtRest = true;
                        Print2(rows, null, (int)i);
                    }
                }
            }
            return (rows.Count + rowsRemoved).ToString();
        }
    }
}
