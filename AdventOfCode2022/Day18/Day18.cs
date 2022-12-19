using AdventOfCode.Framework;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Diagnostics.Tracing.Parsers;
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
    [Solution(18)]
#if RELEASE
    [SolutionInput("Day18\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day18\\InputTest.txt", Enabled = true)]
#endif
    internal class Day18 : Solution
    {
        public Day18(Input input) : base(input)
        {
        }

        private static void LogToConsole(object item)
        {
#if DEBUG
            Console.WriteLine(item.ToString());
#endif
        }

        public record CubeFace(int xStart, int xEnd, int yStart, int yEnd, int zStart, int zEnd);
        

        public record Cube(int x, int y, int z)
        {
            public List<CubeFace> ToFaces()
            {
                return new List<CubeFace>()
                {
                    new CubeFace(x, x+1, y, y+1, z, z),
                    new CubeFace(x, x+1, y, y+1, z+1, z+1),
                    new CubeFace(x, x+1, y, y, z, z+1),
                    new CubeFace(x, x+1, y+1, y+1, z, z+1),
                    new CubeFace(x, x, y, y+1, z, z+1),
                    new CubeFace(x+1, x+1, y, y+1, z, z+1),
                };
            }

            public List<Cube> ToSurroundingCubes()
            {
                return new List<Cube>()
                {
                    new Cube(x, y, z+1),
                    new Cube(x, y, z-1),
                    new Cube(x, y + 1, z),
                    new Cube(x, y - 1, z),
                    new Cube(x + 1, y, z),
                    new Cube(x - 1, y, z),
                };
            }
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
        public List<Cube> ParseCubes(string text)
        {
            var result = new List<Cube>();
            foreach (var coordSet in text.SplitFast("\r\n"))
            {
                var copyCoordSet = coordSet;
                var x = ParseNumber(ref copyCoordSet);
                copyCoordSet = copyCoordSet[1..];
                var y = ParseNumber(ref copyCoordSet);
                copyCoordSet = copyCoordSet[1..];
                var z = ParseNumber(ref copyCoordSet);

                result.Add(new Cube(x, y, z));
            }
            return result;
        }

        protected override string? Problem1()
        {
            var cubes = ParseCubes(Input.Raw);
            var facesByCount = new Dictionary<CubeFace, int>();
            var cubeFaces = cubes
                .SelectMany(c => c.ToFaces())
                .GroupBy(c => c)
                .ToDictionary(g => g.Key, g => g.Count());

            return cubeFaces.Count(s => s.Value == 1).ToString();
        }

        private List<CubeFace> GetLinkedFaces(IEnumerable<CubeFace> faces, CubeFace faceToMatch)
        {
            var result = new List<CubeFace>();
            foreach (var face in faces)
            {
                if (face == faceToMatch)
                {
                    continue;
                }

                int counter = 0;
                if (face.xStart == faceToMatch.xStart)
                {
                    counter++;
                }
                if (face.yStart == faceToMatch.yStart)
                {
                    counter++;
                }
                if (face.zStart == faceToMatch.zStart)
                {
                    counter++;
                }
                if (face.xEnd == faceToMatch.xEnd)
                {
                    counter++;
                }
                if (face.yEnd == faceToMatch.yEnd)
                {
                    counter++;
                }
                if (face.zEnd == faceToMatch.zEnd)
                {
                    counter++;
                }
                if (counter == 8)
                {
                    result.Add(face);
                }
            }
            return result;
        }

        private record Bounds(int minx, int maxx, int miny, int maxy, int minz, int maxz);

        /// <summary>
        /// For a cube to be enclosed we have a few scenarios:
        /// 1. It is enclosed.
        /// 2. It is adjoining one or more cubes that are all enclosed.
        /// 
        /// This method is a hairy bit of recursion where we check all possible adjoining cubes are part of a set of enclosed cubes
        /// and if they are we return the number of faces of the top level cube which are covered directly.  
        /// </summary>
        private bool TryGetHiddenFaces(HashSet<Cube> allCubes, HashSet<Cube> assumeSurroundedCubes, int x, int y, int z, Bounds bounds, out int hiddenFaces)
        {
            hiddenFaces = 0;
            if (x < bounds.minx || x > bounds.maxx)
            {
                return false;
            }
            if (y < bounds.miny || y > bounds.maxy)
            {
                return false;
            }
            if (z < bounds.minz || z > bounds.maxz)
            {
                return false;
            }

            var cube = new Cube(x, y, z);
            if (allCubes.Contains(cube))
            {
                return false;
            }

            var surroundingCubes = cube.ToSurroundingCubes();
            var cubesToCheckMore = surroundingCubes.Where(s => !allCubes.Contains(s)).ToList();
            //Simple case, completly surrounded
            if (cubesToCheckMore.Count == 0 || assumeSurroundedCubes.Contains(cube))
            {
                hiddenFaces = 6 - cubesToCheckMore.Count;
                return true;
            }

            assumeSurroundedCubes.Add(cube);

            var cubesToLongCheck = cubesToCheckMore
                .Where(s => !assumeSurroundedCubes.Contains(s))
                .ToHashSet();
            if (cubesToLongCheck.Count == 0)
            {
                //We've already recursively checked any cubes not proven hidden so we are surrounded.
                //We return the count of cubes that directly touch it as these sides are obscured.
                hiddenFaces = 6 - cubesToCheckMore.Count;
                return true;
            }

            //We now need to do long checks
            var enclosedCubes = cubesToLongCheck.Where(c => !assumeSurroundedCubes.Contains(c));
            //For each cube around this one that we haven't already ruled out (one of the cubes adjoining us or one that is part of the free space
            //that has already been checked).
            var isEnclosed = enclosedCubes.All(c => TryGetHiddenFaces(allCubes, assumeSurroundedCubes, c.x, c.y, c.z, bounds, out _));
            if (isEnclosed)
            {
                hiddenFaces = 6 - cubesToCheckMore.Count;
                return true;
            }
            return false;
        }

        // <4316
        // <3096
        protected override string? Problem2()
        {
            var cubes = ParseCubes(Input.Raw);
            var facesByCount = new Dictionary<CubeFace, int>();

            var minx = cubes.Min(c => c.x);
            var maxx = cubes.Max(c => c.x);
            var miny = cubes.Min(c => c.y);
            var maxy = cubes.Max(c => c.y);
            var minz = cubes.Min(c => c.z);
            var maxz = cubes.Max(c => c.z);

            var bounds = new Bounds(minx, maxx, miny, maxy, minz, maxz);

            var cubeSet = new HashSet<Cube>(cubes);
            var facesEnclosed = 0;
            for (int x=minx; x<=maxx; x++)
            {
                for (int y = miny; y <= maxy; y++)
                {
                    for (int z = minz; z <= maxz; z++)
                    {
                        if (TryGetHiddenFaces(cubeSet, new HashSet<Cube>(), x, y, z, bounds, out var hiddenFaces))
                        {
                            facesEnclosed += hiddenFaces;
                        }
                    }
                }
            }

            var cubeFaces = cubes
                .SelectMany(c => c.ToFaces())
                .GroupBy(c => c)
                .ToDictionary(g => g.Key, g => g.Count());

            return (cubeFaces.Count(s => s.Value == 1) - facesEnclosed).ToString();
        }
    }
}
