using AdventOfCode.Framework;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2022.Day12
{
    [Solution(12)]
#if RELEASE
    [SolutionInput("Day12\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day12\\InputTest.txt", Enabled = true)]
#endif
    internal class Day12 : Solution
    {
        public Day12(Input input) : base(input)
        {
        }

        private List<char[]> GetGrid()
        {
            List<char[]> rows = new List<char[]>();
            foreach (var line in Input.Raw.SplitFast("\r\n"))
            {
                rows.Add(line.ToArray());
            }
            return rows;
        }

        private record struct Location
        {
            public readonly int Row { get; init; }
            public readonly int Column { get; init; }
        }

        protected override string? Problem1()
        {
            var grid = GetGrid();
            var foundLocations = new HashSet<Location>();
            bool exitFound = false;
            Location startLocation = new Location();
            Location endLocation = new Location();
            for (int i = 0; i < grid.Count; i++) //Rows
            {
                for (int j = 0; j < grid[i].Length; j++) //Columns
                {
                    if (grid[i][j] == 'S')
                    {
                        startLocation = new Location() { Row = i, Column = j };
                        grid[i][j] = 'a';
                    }
                    if (grid[i][j] == 'E')
                    {
                        endLocation = new Location() { Row = i, Column = j };
                        grid[i][j] = 'z';
                    }
                }
            }
            //Could stack allow with fixed size if we track index populated.
            var locations = new List<Location>()
            {
                startLocation
            };
            //Could stack allow with fixed size if we track index populated.
            List<Location> newLocations = new List<Location>();

            int stepCount = 0;
            while (!exitFound)
            {
                foreach (var location in locations)
                {
                    //Try Up
                    if (location.Row - 1 >= 0
                        && grid[location.Row - 1][location.Column] <= grid[location.Row][location.Column] + 1)
                    {
                        var proposedLocation = location with { Row = location.Row - 1 };
                        if (foundLocations.Add(proposedLocation))
                        {
                            newLocations.Add(proposedLocation);
                        }
                    }
                    //Try Down
                    if (location.Row + 1 < grid.Count
                        && grid[location.Row + 1][location.Column] <= grid[location.Row][location.Column] + 1)
                    {
                        var proposedLocation = location with { Row = location.Row + 1 };
                        if (foundLocations.Add(proposedLocation))
                        {
                            newLocations.Add(proposedLocation);
                        }
                    }
                    //Try Left
                    if (location.Column - 1 >= 0
                        && grid[location.Row][location.Column - 1] <= grid[location.Row][location.Column] + 1)
                    {
                        var proposedLocation = location with { Column = location.Column - 1 };
                        if (foundLocations.Add(proposedLocation))
                        {
                            newLocations.Add(proposedLocation);
                        }
                    }
                    //Try Right
                    if (location.Column + 1 < grid[0].Length
                        && grid[location.Row][location.Column + 1] <= grid[location.Row][location.Column] + 1)
                    {
                        var proposedLocation = location with { Column = location.Column + 1 };
                        if (foundLocations.Add(proposedLocation))
                        {
                            newLocations.Add(proposedLocation);
                        }
                    }
                }
                stepCount += 1;
                if (newLocations.Find(p => p == endLocation) == endLocation)
                {
                    return stepCount.ToString();
                }
                var temp = locations;
                locations = newLocations;
                newLocations = temp;
                newLocations.Clear();
            }
            throw new Exception();
        }

        protected override string? Problem2()
        {
            var grid = GetGrid();
            var foundLocations = new HashSet<Location>();
            bool exitFound = false;
            Location startLocation = new Location();
            for (int i = 0; i < grid.Count; i++) //Rows
            {
                for (int j = 0; j < grid[i].Length; j++) //Columns
                {
                    if (grid[i][j] == 'E')
                    {
                        startLocation = new Location() { Row = i, Column = j };
                        grid[i][j] = 'z';
                    }
                }
            }
            var locations = new List<Location>()
            {
                startLocation,
            };

            int stepCount = 0;
            while (!exitFound)
            {
                List<Location> newLocations = new List<Location>();
                foreach (var lastLocation in locations)
                {
                    //Try Up
                    if (lastLocation.Row - 1 >= 0
                        && grid[lastLocation.Row - 1][lastLocation.Column] >= grid[lastLocation.Row][lastLocation.Column] - 1)
                    {
                        var proposedLocation = new Location() { Row = lastLocation.Row - 1, Column = lastLocation.Column };
                        if (foundLocations.Add(proposedLocation))
                        {
                            newLocations.Add(proposedLocation);
                        }
                    }
                    //Try Down
                    if (lastLocation.Row + 1 < grid.Count
                        && grid[lastLocation.Row + 1][lastLocation.Column] >= grid[lastLocation.Row][lastLocation.Column] - 1)
                    {
                        var proposedLocation = new Location() { Row = lastLocation.Row + 1, Column = lastLocation.Column };
                        if (foundLocations.Add(proposedLocation))
                        {
                            newLocations.Add(proposedLocation);
                        }
                    }
                    //Try Left
                    if (lastLocation.Column - 1 >= 0
                        && grid[lastLocation.Row][lastLocation.Column - 1] >= grid[lastLocation.Row][lastLocation.Column] - 1)
                    {
                        var proposedLocation = new Location() { Row = lastLocation.Row, Column = lastLocation.Column - 1 };
                        if (foundLocations.Add(proposedLocation))
                        {
                            newLocations.Add(proposedLocation);
                        }
                    }
                    //Try Right
                    if (lastLocation.Column + 1 < grid[0].Length
                        && grid[lastLocation.Row][lastLocation.Column + 1] >= grid[lastLocation.Row][lastLocation.Column] - 1)
                    {
                        var proposedLocation = new Location() { Row = lastLocation.Row, Column = lastLocation.Column + 1 };
                        if (foundLocations.Add(proposedLocation))
                        {
                            newLocations.Add(proposedLocation);
                        }
                    }
                }
                stepCount += 1;
                if (newLocations.FindIndex(p => grid[p.Row][p.Column] == 'a') != -1)
                {
                    return stepCount.ToString();
                }

                var temp = locations;
                locations = newLocations;
                newLocations = temp;
                newLocations.Clear();
            }
            throw new Exception();
        }
    }
}
