using AdventOfCode.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2022.Day02
{
    [Solution(2)]
#if RELEASE
    [SolutionInput("Day02\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day02\\InputTest.txt", Enabled = true)]
#endif
    internal class Day02 : Solution
    {
        public Day02(Input input) : base(input)
        {
        }

        internal enum SelectedItem : int
        {
            Rock = 1,
            Paper = 2,
            Scissors = 3
        }

        internal static bool Beats(SelectedItem first, SelectedItem second)
        {
            if (first == second)
            {
                return false;
            }
            if ((int)first % 3 - (int)second != -1)
            {
                return true;
            }
            return false;
        }

        private SelectedItem ParseOpponentMove(char opponentMove)
        {
            return opponentMove switch
            {
                'A' => SelectedItem.Rock,
                'B' => SelectedItem.Paper,
                'C' => SelectedItem.Scissors,
                _ => throw new ArgumentException()
            };
        }

        private SelectedItem ParseMyMove(char myMove)
        {
            return myMove switch
            {
                'X' => SelectedItem.Rock,
                'Y' => SelectedItem.Paper,
                'Z' => SelectedItem.Scissors,
                _ => throw new ArgumentException()
            };
        }

        private int ParseMyScore(char myMove)
        {
            return myMove switch
            {
                'X' => 0,
                'Y' => 3,
                'Z' => 6,
                _ => throw new ArgumentException()
            };
        }

        //12855
        //13726
        private int ScoreRound(SelectedItem theirTurn, SelectedItem myTurn)
        {
            if (theirTurn == myTurn)
            {
                return 3 + (int)myTurn;
            }
            if (Beats(myTurn, theirTurn))
            {
                return 6 + (int)myTurn;
            }
            return 0 + (int)myTurn;
        }

        private SelectedItem ComputeTurnGivenScore(SelectedItem theirItem, int score)
        {
            switch (theirItem)
            {
                case SelectedItem.Rock:
                    switch (score)
                    {
                        case 0:
                            return SelectedItem.Scissors;
                        case 3:
                            return theirItem;
                        case 6:
                            return SelectedItem.Paper;
                        default: throw new NotImplementedException();
                    }
                case SelectedItem.Paper:
                    switch (score)
                    {
                        case 0:
                            return SelectedItem.Rock;
                        case 3:
                            return theirItem;
                        case 6:
                            return SelectedItem.Scissors;
                        default: throw new NotImplementedException();
                    }
                case SelectedItem.Scissors:
                    switch (score)
                    {
                        case 0:
                            return SelectedItem.Paper;
                        case 3:
                            return theirItem;
                        case 6:
                            return SelectedItem.Rock;
                        default: throw new NotImplementedException();
                    }
                default: throw new NotImplementedException();
            }
        }

        protected override string? Problem1()
        {
            int sum = 0;
            foreach (var round in Input.Raw.SplitFast("\r\n"))
            {
                var roundCells = round.SplitFast(" ");
                if (roundCells.MoveNext())
                {
                    SelectedItem opponentMove = ParseOpponentMove(roundCells.Current[0]);
                    SelectedItem myMove;
                    if (roundCells.MoveNext())
                    {
                        myMove = ParseMyMove(roundCells.Current[0]);
                        sum += ScoreRound(opponentMove, myMove);
                    }
                }
            }
            return sum.ToString();
        }

        protected override string? Problem2()
        {
            int sum = 0;
            foreach (var round in Input.Raw.SplitFast("\r\n"))
            {
                var roundCells = round.SplitFast(" ");
                if (roundCells.MoveNext())
                {
                    SelectedItem opponentMove = ParseOpponentMove(roundCells.Current[0]);
                    int myScore;
                    if (roundCells.MoveNext())
                    {
                        myScore = ParseMyScore(roundCells.Current[0]);
                        var myMove = ComputeTurnGivenScore(opponentMove, myScore);
                        sum += ScoreRound(opponentMove, myMove);
                    }
                }
            }
            return sum.ToString();
        }
    }
}
