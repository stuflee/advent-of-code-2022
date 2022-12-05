using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2022
{
    [SimpleJob]
    [MemoryDiagnoser]
    public class StringBenchmark
    {
        private string content;

        [GlobalSetup]
        public void Setup()
        {
            content = File.ReadAllText(@"Day01\Input.txt");
        }

        [Benchmark]
        public int Split()
        {
            var length = 0;
            var result = content.Split("\r\n");
            foreach (var value in result)
            {
                length += value.Length;
            }
            return length;
        }

        [Benchmark]
        public int SplitFast()
        {
            int length = 0;
            var enumerator = content.SplitFast("\r\n");
            foreach (var result in enumerator)
            {
                length += result.Length;
            }
            return length;
        }
    }

    internal static class StringUtilities
    {
        public static SplitEnumerator SplitFast(this string str, ReadOnlySpan<char> delimiter)
        {
            return new SplitEnumerator(str, delimiter);
        }

        public static SplitEnumerator SplitFast(this ReadOnlySpan<char> str, ReadOnlySpan<char> delimiter)
        {
            return new SplitEnumerator(str, delimiter);
        }

        public ref struct SplitEnumerator
        {
            private ReadOnlySpan<char> _str;
            private ReadOnlySpan<char> _delimiter;

            public SplitEnumerator(ReadOnlySpan<char> str, ReadOnlySpan<char> delimiter)
            {
                _str = str;
                _delimiter = delimiter;
                Current = default;
            }

            public SplitEnumerator GetEnumerator() => this;

            public bool MoveNext()
            {
                var span = _str;
                if (span.Length == 0)
                {
                    return false;
                }

                var index = span.IndexOf(_delimiter);
                if (index == -1)
                {
                    _str = ReadOnlySpan<char>.Empty;
                    Current = span;
                    return true;
                }

                Current = span.Slice(0, index);
                _str = span.Slice(index + _delimiter.Length);
                return true;
            }

            public ReadOnlySpan<char> Current { get; private set; }
        }
    }
}
