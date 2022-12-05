using AdventOfCode.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2022
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

        private enum SelectedItem
        {
            Rock = 1,
            Paper = 2,
            Scissors = 3
        }

        private SelectedItem OpponentMoveToInteger(char opponentMove)
        {
            switch(opponentMove)
            {
                case 'A': 
                    return SelectedItem.Rock;
                case 'B':
                    return SelectedItem.Paper;
                case 'C':
                    return SelectedItem.Scissors;
                default:
                    throw new NotImplementedException();
            }
        }

        private SelectedItem MyMoveToInteger(char myMove)
        {
            switch (myMove)
            {
                case 'X':
                    return SelectedItem.Rock;
                case 'Y':
                    return SelectedItem.Paper;
                case 'Z':
                    return SelectedItem.Scissors;
                default:
                    throw new NotImplementedException();
            }
        }

        private int MyMoveToScore(char myMove)
        {
            switch (myMove)
            {
                case 'X':
                    return 0;
                case 'Y':
                    return 3;
                case 'Z':
                    return 6;
                default:
                    throw new NotImplementedException();
            }
        }

        private int ScoreRound(SelectedItem theirTurn, SelectedItem myTurn)
        {
            if (theirTurn == myTurn)
            {
                return 3 + (int)myTurn;
            }
            if (theirTurn == SelectedItem.Rock && myTurn == SelectedItem.Paper 
                || theirTurn == SelectedItem.Paper && myTurn == SelectedItem.Scissors
                || theirTurn == SelectedItem.Scissors && myTurn == SelectedItem.Rock)
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
            var rounds = Input
                .Raw
                .Split("\r\n")
                .Select(s =>
                {
                    var roundCells = s.Split(' ');
                    return (OpponentMoveToInteger(roundCells[0][0]), MyMoveToInteger(roundCells[1][0]));
                });

            return rounds.Select(r => ScoreRound(r.Item1, r.Item2)).Sum().ToString();
        }

        protected override string? Problem2()
        {
            var rounds = Input
                .Raw
                .Split("\r\n")
                .Select(s =>
                {
                    var roundCells = s.Split(' ');
                    return (OpponentMoveToInteger(roundCells[0][0]), MyMoveToScore(roundCells[1][0]));
                });

            return rounds.Select(r => ScoreRound(r.Item1, ComputeTurnGivenScore(r.Item1, r.Item2))).Sum().ToString();
        }
    }
}
