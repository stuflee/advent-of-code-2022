using AdventOfCode.Framework;
using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2022.Day01
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
            for (int i=0; i<grid.Count; i++) //Rows
            {
                for (int j=0; j < grid[i].Length; j++) //Columns
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
            var paths = new List<List<Location>>()
            {
                new List<Location>()
                {
                    startLocation
                }
            };

            List<Location>? outPath;
            while (!exitFound)
            {
                List<List<Location>> newPaths = new List<List<Location>>();
                foreach (var path in paths)
                {
                    var lastLocation = path[path.Count - 1];

                    //Try Up
                    if (lastLocation.Row - 1 >= 0 
                        && grid[lastLocation.Row - 1][lastLocation.Column] <= grid[lastLocation.Row][lastLocation.Column] + 1)
                    {
                        var proposedLocation = new Location() { Row = lastLocation.Row - 1, Column = lastLocation.Column };
                        if (foundLocations.Add(proposedLocation))
                        {
                            var newPath = new List<Location>(path.Append(proposedLocation));
                            newPaths.Add(newPath);
                        }
                    }
                    //Try Down
                    if (lastLocation.Row + 1 < grid.Count
                        && grid[lastLocation.Row + 1][lastLocation.Column] <= grid[lastLocation.Row][lastLocation.Column] + 1)
                    {
                        var proposedLocation = new Location() { Row = lastLocation.Row + 1, Column = lastLocation.Column };
                        if (foundLocations.Add(proposedLocation))
                        {
                            var newPath = new List<Location>(path.Append(proposedLocation));
                            newPaths.Add(newPath);
                        }
                    }
                    //Try Left
                    if (lastLocation.Column - 1 >= 0
                        && grid[lastLocation.Row][lastLocation.Column - 1] <= grid[lastLocation.Row][lastLocation.Column] + 1)
                    {
                        var proposedLocation = new Location() { Row = lastLocation.Row, Column = lastLocation.Column - 1};
                        if (foundLocations.Add(proposedLocation))
                        {
                            var newPath = new List<Location>(path.Append(proposedLocation));
                            newPaths.Add(newPath);
                        }
                    }
                    //Try Right
                    if (lastLocation.Column + 1 < grid[0].Length
                        && grid[lastLocation.Row][lastLocation.Column + 1] <= grid[lastLocation.Row][lastLocation.Column] + 1)
                    {
                        var proposedLocation = new Location() { Row = lastLocation.Row, Column = lastLocation.Column + 1};
                        if (foundLocations.Add(proposedLocation))
                        {
                            var newPath = new List<Location>(path.Append(proposedLocation));
                            newPaths.Add(newPath);
                        }
                    }
                }
                paths = newPaths;
                outPath = paths.Where(p => p[p.Count - 1] == endLocation).FirstOrDefault();
                if (outPath != null)
                {
                    return (outPath?.Count - 1).ToString();
                }
            }
            throw new Exception();
        }

        protected override string? Problem2()
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
                    if (grid[i][j] == 'E')
                    {
                        startLocation = new Location() { Row = i, Column = j };
                        grid[i][j] = 'z';
                    }
                }
            }
            var paths = new List<List<Location>>()
            {
                new List<Location>()
                {
                    startLocation
                }
            };

            List<Location>? outPath;
            while (!exitFound)
            {
                List<List<Location>> newPaths = new List<List<Location>>();
                foreach (var path in paths)
                {
                    var lastLocation = path[path.Count - 1];

                    //Try Up
                    if (lastLocation.Row - 1 >= 0
                        && grid[lastLocation.Row - 1][lastLocation.Column] >= grid[lastLocation.Row][lastLocation.Column] - 1)
                    {
                        var proposedLocation = new Location() { Row = lastLocation.Row - 1, Column = lastLocation.Column };
                        if (foundLocations.Add(proposedLocation))
                        {
                            var newPath = new List<Location>(path.Append(proposedLocation));
                            newPaths.Add(newPath);
                        }
                    }
                    //Try Down
                    if (lastLocation.Row + 1 < grid.Count
                        && grid[lastLocation.Row + 1][lastLocation.Column] >= grid[lastLocation.Row][lastLocation.Column] - 1)
                    {
                        var proposedLocation = new Location() { Row = lastLocation.Row + 1, Column = lastLocation.Column };
                        if (foundLocations.Add(proposedLocation))
                        {
                            var newPath = new List<Location>(path.Append(proposedLocation));
                            newPaths.Add(newPath);
                        }
                    }
                    //Try Left
                    if (lastLocation.Column - 1 >= 0
                        && grid[lastLocation.Row][lastLocation.Column - 1] >= grid[lastLocation.Row][lastLocation.Column] - 1)
                    {
                        var proposedLocation = new Location() { Row = lastLocation.Row, Column = lastLocation.Column - 1 };
                        if (foundLocations.Add(proposedLocation))
                        {
                            var newPath = new List<Location>(path.Append(proposedLocation));
                            newPaths.Add(newPath);
                        }
                    }
                    //Try Right
                    if (lastLocation.Column + 1 < grid[0].Length
                        && grid[lastLocation.Row][lastLocation.Column + 1] >= grid[lastLocation.Row][lastLocation.Column] - 1)
                    {
                        var proposedLocation = new Location() { Row = lastLocation.Row, Column = lastLocation.Column + 1 };
                        if (foundLocations.Add(proposedLocation))
                        {
                            var newPath = new List<Location>(path.Append(proposedLocation));
                            newPaths.Add(newPath);
                        }
                    }
                }
                paths = newPaths;
                outPath = paths.Where(p => grid[p[p.Count - 1].Row][p[p.Count - 1].Column] == 'a').FirstOrDefault();
                if (outPath != null)
                {
                    return (outPath?.Count - 1).ToString();
                }
            }
            throw new Exception();
        }
    }
}
