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

namespace AdventOfCode2022.Day14
{
    [Solution(14)]
#if RELEASE
    //6038 Incorrect & too low.
    [SolutionInput("Day14\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day14\\InputTest.txt", Enabled = true)]
    //[SolutionInput("Day14\\Input.txt", Enabled = true)]
#endif
    internal class Day14 : Solution
    {
        public Day14(Input input) : base(input)
        {
        }

        private record struct Location(int X, int Y);

        private record struct SandGrain
        {
            public Location Location { get; set; }
           
        }

        private record struct RockPath
        {
            public Location Start { get; init; }

            public Location End { get; init; }

            public IEnumerable<Location> ToLocations()
            {
                if (Start.X == End.X)
                {
                    var startX = Start.X;
                    var startY = Math.Min(Start.Y, End.Y);
                    var endY = Math.Max(Start.Y, End.Y);
                    return Enumerable
                        .Range(startY, endY - startY + 1)
                        .Select(y => new Location() { X = startX, Y = y });
                }
                else
                {
                    var startY = Start.Y;
                    var startX = Math.Min(Start.X, End.X);
                    var endX = Math.Max(Start.X, End.X);
                    return Enumerable
                        .Range(startX, endX - startX + 1)
                        .Select(x => new Location() { X = x, Y = startY });
                }

            }
        }

        private class RockModel
        {
            private HashSet<Location> takenLocations;

            private int highestYCoordinate;

            private int highestXCoordinate;

            private int lowestXCoordinate;

            private bool haveLastSafeLocation;

            private Location lastSafeLocation;

            public RockModel(List<RockPath> rockPaths)
            {
                highestYCoordinate = Math.Max(
                    rockPaths.Select(r => r.Start.Y).Max(),
                    rockPaths.Select(r => r.End.Y).Max());

                highestXCoordinate = Math.Max(
                    rockPaths.Select(r => r.Start.X).Max(),
                    rockPaths.Select(r => r.End.X).Max());

                lowestXCoordinate = Math.Min(
                    rockPaths.Select(r => r.Start.X).Min(),
                    rockPaths.Select(r => r.End.X).Min());
                takenLocations = rockPaths.SelectMany(r => r.ToLocations()).ToHashSet();
            }

            public void AddBlockingLine()
            {
                var yCoordinate = highestYCoordinate + 2;
                //Create a horizontal path that is big enough to capture height in both directions to ensure no overflow.
                //Minimal size reduces search time when dropping sand.
                var newPath = new RockPath()
                {
                    Start = new Location() { X = lowestXCoordinate - highestYCoordinate, Y = yCoordinate },
                    End = new Location() { X = highestXCoordinate + highestYCoordinate, Y = yCoordinate },
                };
                foreach (var path in newPath.ToLocations())
                {
                    takenLocations.Add(path);
                }
                highestYCoordinate = highestYCoordinate + 2;
            }

            public bool TryAddSandGrain(SandGrain sandGrain)
            {
                var initialLocation = sandGrain.Location;
                if (Contains(initialLocation))
                {
                    return false;
                }
                bool isARestLocation = false;
                var proposedLocation = haveLastSafeLocation ? lastSafeLocation : initialLocation;
                var lastSafeLocationThisIteration = initialLocation;
                while (!isARestLocation && proposedLocation.Y < highestYCoordinate)
                {
                    if (!Contains(proposedLocation with { Y = proposedLocation.Y + 1 }))
                    {
                        //can move down, but not a rest location.
                        lastSafeLocationThisIteration = proposedLocation;
                        proposedLocation = proposedLocation with { Y = proposedLocation.Y + 1 };
                    }
                    else if (!Contains(proposedLocation with { Y = proposedLocation.Y + 1, X = proposedLocation.X - 1 }))
                    {
                        lastSafeLocationThisIteration = proposedLocation;
                        proposedLocation = proposedLocation with { Y = proposedLocation.Y + 1, X = proposedLocation.X - 1 };
                    }
                    else if (!Contains(proposedLocation with { Y = proposedLocation.Y + 1, X = proposedLocation.X + 1 }))
                    {
                        lastSafeLocationThisIteration = proposedLocation;
                        proposedLocation = proposedLocation with { Y = proposedLocation.Y + 1, X = proposedLocation.X + 1 };
                    }
                    else //Can't move anywhere.
                    {
                        isARestLocation = true;
                    }
                }
                if (proposedLocation == lastSafeLocation)
                {
                    haveLastSafeLocation = false;
                }
                else
                { 
                    //The last location empty before we settled is the first we will try next time.
                    lastSafeLocation = lastSafeLocationThisIteration;
                    haveLastSafeLocation = true;
                }

                if (proposedLocation.Y >= highestYCoordinate)
                {
                    return false;
                }
                //LogToConsole($"Able to add sand to location {proposedLocation}");
                takenLocations.Add(proposedLocation);
                return true;
            }

            private int counter = 0;
            public bool Contains(Location location)
            {
                if (counter++ % 10000 == 0)
                {
                    Console.WriteLine(counter++);
                }
                return takenLocations.Contains(location);
            }
        }

        private static void LogToConsole(object item)
        {
#if DEBUG
            Console.WriteLine(item.ToString());
#endif
        }

        private int ParseNumber(ref ReadOnlySpan<char> rockPath)
        {
            var number = rockPath[0] - '0';
            rockPath = rockPath[1..];
            while (rockPath.Length > 0 && rockPath[0] <= '9' && rockPath[0] >= '0')
            {
                number = number * 10 + rockPath[0] - '0';
                rockPath = rockPath[1..];
            }
            return number;
        }


        private Location ParseLocation(ref ReadOnlySpan<char> rockPath)
        {
            var firstLocation = ParseNumber(ref rockPath);
            rockPath = rockPath[1..]; //Skip comma, perhaps should validate but I'm lazy.
            var secondLocation = ParseNumber(ref rockPath);
            if (rockPath.Length > 0)
            {
                rockPath = rockPath[4..]; //Skip ' -> ', perhaps should validate but I'm lazy.
            }
            return new Location() { X  = firstLocation, Y = secondLocation };
        }

        private List<RockPath> ParsePath(ReadOnlySpan<char> rockPath)
        {
            var result = new List<RockPath>();
            Location startLocation = ParseLocation(ref rockPath);
            while (rockPath.Length > 0)
            {
                var endLocation = ParseLocation(ref rockPath);
                result.Add(new RockPath() { Start = startLocation, End = endLocation });
                startLocation = endLocation;
            }
            return result;
        }

        private RockModel Parse(string text)
        {
            var rockPaths = new List<RockPath>();
            foreach (var line in text.SplitFast("\r\n"))
            {
                var paths = ParsePath(line);
                rockPaths.AddRange(paths);
            }
            return new RockModel(rockPaths);
        }

        protected override string? Problem1()
        {
            var rockModel = Parse(Input.Raw);
            bool sandGrainCanBeAdded = true;
            int count = 0;
            while (sandGrainCanBeAdded)
            {
                var sandGrain = new SandGrain() { Location = new Location(500, 0) };
                sandGrainCanBeAdded = rockModel.TryAddSandGrain(sandGrain);
                if (sandGrainCanBeAdded)
                { 
                    count++;
                }
            }

            return count.ToString();
        }

        protected override string? Problem2()
        {
            var rockModel = Parse(Input.Raw);
            rockModel.AddBlockingLine();
            bool sandGrainCanBeAdded = true;
            int count = 0;
            while (sandGrainCanBeAdded)
            {
                var sandGrain = new SandGrain() { Location = new Location(500, 0) };
                sandGrainCanBeAdded = rockModel.TryAddSandGrain(sandGrain);
                if (sandGrainCanBeAdded)
                {
                    count++;
                    //LogToConsole($"Added grain of sand {count}");
                }
            }

            return count.ToString();
        }
    }
}
