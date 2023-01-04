using AdventOfCode.Framework;
using Microsoft.Diagnostics.Runtime.Utilities;
using System.Management;
using System.Reflection.Metadata.Ecma335;

namespace AdventOfCode2022.Day19
{
    [Solution(19)]
#if RELEASE
    [SolutionInput("Day19\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day19\\InputTest.txt", Enabled = true)]
#endif
    internal class Day19 : Solution
    {
        public Day19(Input input) : base(input)
        {
        }

        private static void LogToConsole(object item)
        {
#if DEBUG
            Console.WriteLine(item.ToString());
#endif
        }

        public enum Decision
        {
            BuildOre,
            BuildClay,
            BuildObsidian,
            BuildGeode
        }

        private record struct ResourceBundle(int clay, int ore, int obsidian, int geode)
        {
            public bool Contains(ResourceBundle bundle)
            {
                return clay >= bundle.clay && ore >= bundle.ore && obsidian >= bundle.obsidian;
            }

            public ResourceBundle Spend(ResourceBundle spent)
            {
                //Can't spend geode
                return new ResourceBundle(clay - spent.clay, ore - spent.ore, obsidian - spent.obsidian, geode);
            }
            public ResourceBundle Produce(RobotCounts robots)
            {
                return new ResourceBundle(clay + robots.clayRobotCount, ore + robots.oreRobotCount, obsidian + robots.obsidianRobotCount, geode + robots.geodeRobotCount);
            }
        }

        private record Blueprint(int Id, ResourceBundle OreRobotCost, ResourceBundle ClayRobotCost, ResourceBundle ObsidianRobotCost, ResourceBundle GeodeRobotCost)
        {
            public int MaxOre()
            {
                return new[] { OreRobotCost.ore, ClayRobotCost.ore, ObsidianRobotCost.ore, GeodeRobotCost.ore }.Max();
            }
            public int MaxClay()
            {
                return ObsidianRobotCost.clay;
            }
            public int MaxObsidian()
            {
                return GeodeRobotCost.obsidian;
            }
        }

        private record RobotCounts(int oreRobotCount, int clayRobotCount, int obsidianRobotCount, int geodeRobotCount)
        {
            public int Totals => oreRobotCount + clayRobotCount + obsidianRobotCount + geodeRobotCount;
        }

        private record Itinerary(ResourceBundle stock, RobotCounts robots)
        {
            public Itinerary Produce()
            {
                return this with { stock = stock.Produce(robots) };
            }

            public Itinerary Build(Decision decision, Blueprint blueprint)
            {
                return decision switch
                {
                    Decision.BuildOre => new Itinerary(stock.Spend(blueprint.OreRobotCost), robots with { oreRobotCount = robots.oreRobotCount + 1 }),
                    Decision.BuildClay => new Itinerary(stock.Spend(blueprint.ClayRobotCost), robots with { clayRobotCount = robots.clayRobotCount + 1 }),
                    Decision.BuildObsidian => new Itinerary(stock.Spend(blueprint.ObsidianRobotCost), robots with { obsidianRobotCount = robots.obsidianRobotCount + 1 }),
                    Decision.BuildGeode => new Itinerary(stock.Spend(blueprint.GeodeRobotCost), robots with { geodeRobotCount = robots.geodeRobotCount + 1 }),
                    _ => throw new Exception()
                };
            }
        }

        private IEnumerable<Decision> PossibleDecisions(Itinerary itinerary, Blueprint blueprint)
        {

            if (itinerary.stock.Contains(blueprint.ClayRobotCost))
            {
                if (itinerary.robots.clayRobotCount <= blueprint.MaxClay())
                {
                    yield return Decision.BuildClay;
                }
            }
            if (itinerary.stock.Contains(blueprint.OreRobotCost))
            {
                if (itinerary.robots.oreRobotCount <= blueprint.MaxOre())
                {
                    yield return Decision.BuildOre;
                }
            }
            if (itinerary.stock.Contains(blueprint.ObsidianRobotCost))
            {
                if (itinerary.robots.obsidianRobotCount <= blueprint.MaxObsidian())
                {
                    yield return Decision.BuildObsidian;
                }
            }
            if (itinerary.stock.Contains(blueprint.GeodeRobotCost))
            {
                yield return Decision.BuildGeode;
            }
            yield break;
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

        private ResourceBundle ParseRobotCosts(ref ReadOnlySpan<char> text)
        {
            int ore = 0;
            int clay = 0;
            int obsidian = 0;
            ReadOnlySpan<char> previous = "0";
            foreach (var current in text.SplitFast(" "))
            {
                if (current.Equals("ore", StringComparison.CurrentCultureIgnoreCase) && !previous.Equals("Each", StringComparison.CurrentCultureIgnoreCase))
                {
                    ore = ParseNumber(ref previous);
                }
                if (current.Equals("clay", StringComparison.CurrentCultureIgnoreCase) && !previous.Equals("Each", StringComparison.CurrentCultureIgnoreCase))
                {
                    clay = ParseNumber(ref previous);
                }
                if (current.Equals("obsidian", StringComparison.CurrentCultureIgnoreCase) && !previous.Equals("Each", StringComparison.CurrentCultureIgnoreCase))
                {
                    obsidian = ParseNumber(ref previous);
                }

                previous = current;
            }
            return new ResourceBundle(clay, ore, obsidian, 0);
        }

        private List<Blueprint> Parse(string text)
        { 
            var blueprints = new List<Blueprint>();
            foreach (var blueprint in text.SplitFast("\r\n"))
            {
                var idStart = blueprint.IndexOf(' ');
                var remaining = blueprint.Slice(idStart + 1);
                var id = ParseNumber(ref remaining);

                var enumerator = blueprint.SplitFast(".");
                enumerator.MoveNext();
                var oreRobotDefinition = enumerator.Current;
                enumerator.MoveNext();
                var clayRobotDefinition = enumerator.Current;
                enumerator.MoveNext();
                var obsidianRobotDefinition = enumerator.Current;
                enumerator.MoveNext();
                var geodeRobotDefinition = enumerator.Current;

                blueprints.Add(
                    new Blueprint(
                        id,
                        ParseRobotCosts(ref oreRobotDefinition),
                        ParseRobotCosts(ref clayRobotDefinition),
                        ParseRobotCosts(ref obsidianRobotDefinition),
                        ParseRobotCosts(ref geodeRobotDefinition)));
            }
            return blueprints;
        }

        protected override string? Problem1()
        {
            var bluePrints = Parse(Input.Raw);
            var qualityByBluePrint = new Dictionary<int, int>();
            foreach (var blueprint in bluePrints)
            {
                var initialItinerary = new Itinerary(new ResourceBundle(), new RobotCounts(1, 0, 0, 0));
                var itineraries = new HashSet<Itinerary>()
                {
                    initialItinerary
                };

                for (int i = 0; i < 24; i++)
                {
                    var newItineraries = new HashSet<Itinerary>();
                    foreach (var itinerary in itineraries)
                    {
                        //Decide before we produce
                        var possibleDescisions = PossibleDecisions(itinerary, blueprint);

                        //Produce before we build
                        var newItinerary = itinerary.Produce();
                        newItineraries.Add(newItinerary);
                        //It's always allowed to do nothing.
                        foreach (var decision in possibleDescisions)
                        {
                            newItineraries.Add(newItinerary.Build(decision, blueprint));
                        }
                       
                    }
                    itineraries = Prune(newItineraries);
                }
                var maxGeode = itineraries.Max(i => i.stock.geode);
                qualityByBluePrint.Add(blueprint.Id, maxGeode);
            }


            return qualityByBluePrint.Sum(kvp => kvp.Key * kvp.Value).ToString();
        }

        private HashSet<Itinerary> Prune(HashSet<Itinerary> itineraries)
        {
            var result = new HashSet<Itinerary>();
            var mappedItineraries = itineraries
                .GroupBy(i => i.robots)
                .ToDictionary(g => g.Key, g => g.ToList());

            var maxObsidianRobots = mappedItineraries.Keys.Max(k => k.obsidianRobotCount);
            var maxGeodeRobots = mappedItineraries.Keys.Max(k => k.geodeRobotCount);


            foreach ((var robots, var mappeditineraries) in mappedItineraries)
            {
                if (robots.obsidianRobotCount < maxObsidianRobots && robots.geodeRobotCount < maxGeodeRobots)
                {
                    continue;
                }
                foreach (var itinerary in mappeditineraries)
                {
                    var greaterItineraries = mappeditineraries
                        .Where(i => i != itinerary)
                        .Where(i => i.stock.ore >= itinerary.stock.ore)
                        .Where(i => i.stock.clay >= itinerary.stock.clay)
                        .Where(i => i.stock.obsidian >= itinerary.stock.obsidian)
                        .Where(i => i.stock.geode >= itinerary.stock.geode);
                    if (!greaterItineraries.Any())
                    {
                        result.Add(itinerary);
                    }
                }
            }

            return result;
        }

        protected override string? Problem2()
        {
            var bluePrints = Parse(Input.Raw);
            var qualityByBluePrint = new Dictionary<int, int>();
            foreach (var blueprint in bluePrints.Take(3))
            {
                var initialItinerary = new Itinerary(new ResourceBundle(), new RobotCounts(1, 0, 0, 0));
                var itineraries = new HashSet<Itinerary>()
                {
                    initialItinerary
                };

                for (int i = 0; i < 32; i++)
                {
                    var newItineraries = new HashSet<Itinerary>();
                    foreach (var itinerary in itineraries)
                    {
                        //Decide before we produce
                        var possibleDescisions = PossibleDecisions(itinerary, blueprint);

                        //Produce before we build
                        var newItinerary = itinerary.Produce();
                        newItineraries.Add(newItinerary);
                        //It's always allowed to do nothing.
                        foreach (var decision in possibleDescisions)
                        {
                            var newItineraryToAdd = newItinerary.Build(decision, blueprint);

                            newItineraries.Add(newItineraryToAdd);
                            
                        }

                    }

                    itineraries = newItineraries;
                    //For larger i becomes too expensive.
                    if (i<25)
                    {
                        itineraries = Prune(itineraries);
                    }
                }
                var maxGeode = itineraries.MaxBy(i => i.stock.geode);
                if (maxGeode != null)
                {
                    qualityByBluePrint.Add(blueprint.Id, maxGeode.stock.geode);
                }
            }

            var result = 1;
            foreach (var value in qualityByBluePrint.Values)
            {
                result *= value;
            }
            return result.ToString();
        }
    }
}
