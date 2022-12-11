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

namespace AdventOfCode2022.Day07
{
    [Solution(7)]
#if RELEASE
    [SolutionInput("Day07\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day07\\InputTest1.txt", Enabled = true)]
#endif
    internal class Day07 : Solution
    {
        public Day07(Input input) : base(input)
        {
        }

        private record File
        {
            public string Name { get; init; }

            public long Size { get; set; }

            public override string ToString()
            {
                return $"- {Name} (file, size={Size.ToString()})";
            }
        }

        private class DirectoryPath
        {
            public List<string> Parts { get; init; } = new List<string>();

            public DirectoryPath MoveUpOneLevel()
            {
                return new DirectoryPath()
                {
                    Parts = Parts.Take(Parts.Count - 1).ToList()
                };
            }

            public DirectoryPath MoveDownOneLevel(string directoryName)
            {
                return new DirectoryPath()
                {
                    Parts = Parts.Append(directoryName).ToList()
                };
            }

            public override int GetHashCode()
            {
                if (Parts.Count == 0)
                {
                    return 0;
                }
                return Parts[^1].GetHashCode();
            }

            public override bool Equals(object? other)
            {
                DirectoryPath? otherDirectory = other as DirectoryPath;
                if (otherDirectory == null)
                {
                    return false;
                }

                if (otherDirectory.Parts.Count != Parts.Count)
                {
                    return false;
                }
                foreach (var pair in otherDirectory.Parts.Zip(Parts))
                {
                    if (pair.First != pair.Second)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private record Directory
        {
            public string Name { get; init; } = "/";

            public DirectoryPath Path { get; init; } = new DirectoryPath();

            public List<Directory> Directories { get; } = new List<Directory>();

            public List<File> Files { get; init; } = new List<File>();
            public long Size()
            {
                return Files.Sum(f => f.Size) + Directories.Sum(d => d.Size());
            }

            public override string ToString()
            {
                var indent = new string(' ', 2 * Path.Parts.Count);
                var builder = new StringBuilder();
                var thisDirectory = Name;
                builder.AppendLine($"{indent}- {thisDirectory} (dir)");

                var contents = new Dictionary<string, string>();
                foreach (var directory in Directories)
                {
                    contents.Add(directory.Name, directory.ToString());
                }
                foreach (var file in Files)
                {
                    contents.Add(file.Name, $"{indent}  {file}{Environment.NewLine}");
                }

                foreach (var item in contents.Keys.ToList().OrderBy(s => s))
                {
                    builder.Append(contents[item]);
                }
                return builder.ToString();
            }

        }
        private Directory GetOrAdd(DirectoryPath path, Dictionary<DirectoryPath, Directory> directoriesSoFar)
        {
            Directory newCurrentDirectory;
            if (!directoriesSoFar.TryGetValue(path, out newCurrentDirectory))
            {
                newCurrentDirectory = new Directory()
                {
                    Name = path.Parts.Count == 0 ? "/" : path.Parts[^1],
                    Path = path
                };
                directoriesSoFar.Add(path, newCurrentDirectory);
                if (path.Parts.Count != 0)
                {
                    var parent = path.MoveUpOneLevel();
                    if (directoriesSoFar.TryGetValue(parent, out var parentDirectory))
                    {
                        parentDirectory.Directories.Add(newCurrentDirectory);
                    }

                }
            }
            return newCurrentDirectory;
        }

        private Dictionary<DirectoryPath, Directory> ParseDirectories(string commandText)
        {
            Dictionary<DirectoryPath, Directory> directoriesSoFar = new();

            DirectoryPath currentDirectoryPath = new DirectoryPath();
            foreach (var commandLine in commandText.SplitFast("\r\n"))
            {
                //Check if this is a command.
                if (commandLine.StartsWith("$"))
                {
                    if (commandLine.StartsWith("$ cd "))
                    {
                        var path = commandLine.Slice(5).ToString();
                        if (path == "/")
                        {
                            currentDirectoryPath = new DirectoryPath();
                        }
                        else if (path == "..")
                        {
                            currentDirectoryPath = currentDirectoryPath.MoveUpOneLevel();
                        }
                        else
                        {
                            currentDirectoryPath = currentDirectoryPath.MoveDownOneLevel(path);
                        }
                        GetOrAdd(currentDirectoryPath, directoriesSoFar);
                        continue;
                    }
                    else if (commandLine.StartsWith("$ ls"))
                    {
                        //Implictly we will list the dir contents.
                        continue;
                    }
                }

                //We must have content so we parse it.
                if (commandLine.StartsWith("dir"))
                {
                    var name = commandLine.Slice(4).ToString();
                    var path = currentDirectoryPath.MoveDownOneLevel(name);
                    GetOrAdd(path, directoriesSoFar);
                }
                else //File
                {
                    var parentDirectory = directoriesSoFar[currentDirectoryPath];
                    var fileParts = commandLine.ToString().Split(" ");
                    var file = new File()
                    {
                        Name = fileParts[1],
                        Size = long.Parse(fileParts[0])
                    };
                    parentDirectory.Files.Add(file);
                }

            }
            return directoriesSoFar;
        }


        protected override string? Problem1()
        {
            var text = Input.Raw;

            Dictionary<DirectoryPath, Directory> directoriesSoFar = ParseDirectories(text);

            var sum = 0L;
            foreach (var (path, directory) in directoriesSoFar)
            {
                if (directory.Size() <= 100000)
                {
                    sum += directory.Size();
                }
            }
            return sum.ToString();
        }


        protected override string? Problem2()
        {
            long totalSpace = 70000000;
            long requiredSpaceAvailable = 30000000;

            var text = Input.Raw;

            Dictionary<DirectoryPath, Directory> directoriesSoFar = ParseDirectories(text);

            var rootDirectory = directoriesSoFar[new DirectoryPath()];
            var spaceUsed = rootDirectory.Size();
            var spaceAvailable = totalSpace - spaceUsed;
            var spaceToFree = requiredSpaceAvailable - spaceAvailable;
            
            Directory? directoryToDelete = null;
            foreach (var (path, directory) in directoriesSoFar)
            {
                var size = directory.Size();
                if (size > spaceToFree && directory.Size() < (directoryToDelete?.Size() ?? long.MaxValue))
                {
                    directoryToDelete = directory;
                }
            }
            return directoryToDelete?.Size().ToString();
        }
    }
}
