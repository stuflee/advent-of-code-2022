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

namespace AdventOfCode2022.Day15
{
    [Solution(15)]
#if RELEASE
    [SolutionInput("Day15\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day15\\InputTest.txt", Enabled = true)]
#endif
    internal class Day15 : Solution
    {
        public Day15(Input input) : base(input)
        {
        }

        private static void LogToConsole(object item)
        {
#if DEBUG
            Console.WriteLine(item.ToString());
#endif
        }

        private record struct Location(int X, int Y)
        {
            public int DistanceFrom(Location otherLocation)
            {
                return Math.Abs(otherLocation.X - X) + Math.Abs(otherLocation.Y - Y);
            }
        };

        private record struct Sensor
        {
            public Location SensorLocation { get; init; }
            public Location NearestBeacon { get; init; }

            public int ScannedDistance { get; init; }
            
            public Sensor(Location sensorLocation, Location nearestBeacon)
            {
                ScannedDistance = sensorLocation.DistanceFrom(nearestBeacon);
                SensorLocation = sensorLocation;
                NearestBeacon = nearestBeacon;
            }

            public int DistanceFrom(Location l)
            {
                return SensorLocation.DistanceFrom(l);
            }

            public bool HasScanned(Location l)
            {
                return SensorLocation.DistanceFrom(l) <= ScannedDistance;
            }
        };


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

        private Sensor ParseSensor(ReadOnlySpan<char> sensorText)
        {
            var equalsIndex = sensorText.IndexOf("=");
            
            var remaining = sensorText.Slice(equalsIndex + 1);
            var xCoordinate = ParseNumber(ref remaining);
            
            remaining = remaining.Slice(4); //Skip comma and y=
            var yCoordinate = ParseNumber(ref remaining);
            
            equalsIndex = remaining.IndexOf("=");
            remaining = remaining.Slice(equalsIndex + 1);
            var xCoordinateBeacon = ParseNumber(ref remaining);
            
            remaining = remaining.Slice(4); //Skip comma and y=
            var yCoordinateBeacon = ParseNumber(ref remaining);

            return new Sensor(
                new Location(xCoordinate, yCoordinate), 
                new Location(xCoordinateBeacon, yCoordinateBeacon));
        }

        private List<Sensor> ParseSensors(string text)
        {
            var result = new List<Sensor>();
            foreach (var sensorLine in text.SplitFast("\r\n"))
            {
                result.Add(ParseSensor(sensorLine));
            }
            return result;
        }

        protected override string? Problem1()
        {
#if DEBUG
            var testRow = 10;
#else
            var testRow = 2000000;
#endif

            var sensors = ParseSensors(Input.Raw);

            var minX = sensors.Select(s => s.SensorLocation.X - s.ScannedDistance).Min();
            var maxX = sensors.Select(s => s.SensorLocation.X + s.ScannedDistance).Max();

            sensors.OrderBy(s => Math.Abs(2000000 - s.SensorLocation.Y));

            int scannedCount = 0;
            for (int i = minX; i <= maxX; i++)
            {
                bool hasBeenScanned = false;
                var testLocation = new Location(i, testRow);
                foreach (var sensor in sensors)
                {
                    if (sensor.HasScanned(testLocation))
                    {
                        if (sensor.NearestBeacon != testLocation)
                        {
                            hasBeenScanned = true;
                        }
                    }
                }
                if (hasBeenScanned)
                {
                    scannedCount++;
                }
            }

            return scannedCount.ToString();
        }

        protected override string? Problem2()
        {
#if DEBUG
            var searchArea = 20;
            var gridSize = 2;
#else
            var searchArea = 4000000L;
            var gridSize = 1000;
#endif

            var sensors = ParseSensors(Input.Raw);

            var sortedSensors = new Sensor[sensors.Count];
            //Break up the area into grids, some grids we'll be able to skip if we can establish they are totally within a sensor area.
            for (int x = 0; x <= (int)searchArea; x += gridSize)
            {
                int y = 0;
                while (y <= searchArea)
                {
                    var midPoint = new Location(x + gridSize/2 - 1, y + gridSize / 2 - 1);
                    int minBoundaryDistance = 0;
                    for (int i=0; i<sensors.Count; i++)
                    {
                        minBoundaryDistance = Math.Max(minBoundaryDistance, sensors[i].ScannedDistance - sensors[i].DistanceFrom(midPoint));
                    }

                    if (minBoundaryDistance <= gridSize)
                    {
                        //Order them by the sensors closest to the midpoint of the grid to maximise an early match.
                        var sortedSensorsTemp = 
                            sensors.OrderBy(s => s.DistanceFrom(midPoint));

                        var index = 0;
                        foreach (var sensor in sortedSensorsTemp)
                        {
                            sortedSensors[index++] = sensor;
                        }

                        for (int inner_x = x; inner_x < x + gridSize; inner_x++)
                        {
                            for (int inner_y = y; inner_y < y + gridSize; inner_y++)
                            {
                                var testLocation = new Location(inner_x, inner_y);
                                bool hasBeenScanned = false;
                                for (int i=0; i<sortedSensors.Length; i++)
                                {
                                    if (sortedSensors[i].HasScanned(testLocation))
                                    {
                                        hasBeenScanned = true;
                                        break;
                                    }
                                }
                                if (!hasBeenScanned)
                                {
                                    var result = new Location(inner_x, inner_y);
                                    Console.WriteLine(result);
                                    Console.WriteLine();
                                    return (result.X * 4000000L + result.Y).ToString();
                                }
                            }
                        }
                    }
                    y += gridSize;
                }
            }
             

            throw new EntryPointNotFoundException();
        }
    }
}
