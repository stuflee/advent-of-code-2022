// See https://aka.ms/new-console-template for more information
using AdventOfCode.Framework;
using AdventOfCode2022;

SolutionRunner runner = new();
runner.Solve(2);


Console.WriteLine(Day02.Beats(Day02.SelectedItem.Paper, Day02.SelectedItem.Paper));
Console.WriteLine(Day02.Beats(Day02.SelectedItem.Rock, Day02.SelectedItem.Rock));
Console.WriteLine(Day02.Beats(Day02.SelectedItem.Scissors, Day02.SelectedItem.Scissors));

Console.WriteLine(Day02.Beats(Day02.SelectedItem.Scissors, Day02.SelectedItem.Paper));
Console.WriteLine(Day02.Beats(Day02.SelectedItem.Rock, Day02.SelectedItem.Scissors));
Console.WriteLine(Day02.Beats(Day02.SelectedItem.Paper, Day02.SelectedItem.Rock));

Console.WriteLine(Day02.Beats(Day02.SelectedItem.Paper, Day02.SelectedItem.Scissors));
Console.WriteLine(Day02.Beats(Day02.SelectedItem.Scissors, Day02.SelectedItem.Rock));
Console.WriteLine(Day02.Beats(Day02.SelectedItem.Rock, Day02.SelectedItem.Paper));