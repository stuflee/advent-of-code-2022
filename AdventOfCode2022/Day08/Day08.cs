using AdventOfCode.Framework;
using CommandLine;
using Microsoft.CodeAnalysis;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AdventOfCode2022.Day08
{
    [Solution(8)]
#if RELEASE
    [SolutionInput("Day08\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day08\\InputTest1.txt", Enabled = true)]
    [SolutionInput("Day08\\InputTest2.txt", Enabled = false)]
    [SolutionInput("Day08\\InputTest3.txt", Enabled = false)]
    [SolutionInput("Day08\\InputTest4.txt", Enabled = false)]
    [SolutionInput("Day08\\InputTest5.txt", Enabled = false)]
    [SolutionInput("Day08\\InputTest6.txt", Enabled = false)]
#endif
    internal class Day08 : Solution
    {
        public Day08(Input input) : base(input)
        {
        }

        protected override string? Problem1()
        {
            var text = Input.Raw;
            var rows = new List<char[]>();
            var visible = new List<bool[]>();
            foreach (var row in text.SplitFast("\r\n"))
            {
                var rowAsArray = row.ToArray();
                rows.Add(rowAsArray);
                visible.Add(new bool[rowAsArray.Length]);
            }

            var numberOfRows = rows.Count;
            var numberOfColumns = rows[0].Length;

            var biggestTreesFromTop = rows[0].Select(r => r).ToArray();
            var biggestTreesFromBottom = rows[numberOfRows - 1].Select(r => r).ToArray();

            for (int j = 0; j < numberOfColumns; j++)
            {
                visible[0][j] = true;
                visible[numberOfRows - 1][j] = true;
            }

            visible[0][0] = true;
            visible[numberOfRows - 1][numberOfColumns - 1] = true;


            //For each row
            for (int i = 1; i < numberOfRows - 1; i++)
            {
                int invertedI = numberOfRows - 1 - i;

                char biggestTreeFromLeftFromTop = rows[i][0];
                char biggestTreeFromRightFromTop = rows[i][numberOfColumns - 1];
                char biggestTreeFromLeftFromBottom = rows[invertedI][0];
                char biggestTreeFromRightFromBottom = rows[invertedI][numberOfColumns - 1];
                visible[i][0] = true;
                visible[i][numberOfColumns - 1] = true;
                for (int j = 1; j < numberOfColumns - 1; j++)
                {
                    //Correct loop order going top left to bottom right.
                    //
                    if (rows[i][j] > biggestTreeFromLeftFromTop)
                    {
                        biggestTreeFromLeftFromTop = rows[i][j];
                        visible[i][j] = true;
                    }
                    if (rows[i][j] > biggestTreesFromTop[j])
                    {
                        biggestTreesFromTop[j] = rows[i][j];
                        visible[i][j] = true;
                    }


                    int invertedJ = numberOfColumns - 1 - j;
                    //Incorrect loop order, going bottom right to top left
                    if (rows[invertedI][invertedJ] > biggestTreeFromRightFromBottom)
                    {
                        biggestTreeFromRightFromBottom = rows[invertedI][invertedJ];
                        visible[invertedI][invertedJ] = true;
                    }
                    if (rows[invertedI][invertedJ] > biggestTreesFromBottom[invertedJ])
                    {
                        biggestTreesFromBottom[invertedJ] = rows[invertedI][invertedJ];
                        visible[invertedI][invertedJ] = true;
                    }
                }
            }

            return visible.Sum(r => r.Sum(r => r ? 1 : 0)).ToString();
        }


        protected override string? Problem2()
        {
            var text = Input.Raw;
            var rows = new List<char[]>();
            var scenicScore = new List<long[]>();
            foreach (var row in text.SplitFast("\r\n"))
            {
                var rowAsArray = row.ToArray();
                rows.Add(rowAsArray);
                scenicScore.Add(new long[rowAsArray.Length]);
            }

            for (int i = 0; i < rows.Count; i++)
            {
                for (int j = 0; j < rows[i].Length; j++)
                {
                    scenicScore[i][j] = ComputeScenicScore(i, j, rows);
                }
            }
            return scenicScore.Max(s => s.Max()).ToString();
        }

        private long ComputeScenicScore(int row, int column, List<char[]> treeHeights)
        {
            var treeHeight = treeHeights[row][column];

            if (row == 0 || column == 0 || row == treeHeights.Count - 1 || column == treeHeights[0].Length - 1)
            {
                return 0;
            }

            int viewLeft = 1;
            int viewRight = 1;
            int viewUp = 1;
            int viewDown = 1;

            int columnLeft = column - 1;
            while (columnLeft > 0 && treeHeight > treeHeights[row][columnLeft])
            {
                viewLeft++;
                columnLeft--;
            }
            int columnRight = column + 1;
            while (columnRight < treeHeights[row].Length - 1 && treeHeight > treeHeights[row][columnRight])
            {
                viewRight++;
                columnRight++;
            }
            int rowUp = row - 1;
            while (rowUp > 0 && treeHeight > treeHeights[rowUp][column])
            {
                viewUp++;
                rowUp--;
            }
            int rowDown = row + 1;
            while (rowDown < treeHeights.Count - 1 && treeHeight > treeHeights[rowDown][column])
            {
                viewDown++;
                rowDown++;
            }
            return viewLeft * viewRight * viewUp * viewDown;
        }

    }
}
