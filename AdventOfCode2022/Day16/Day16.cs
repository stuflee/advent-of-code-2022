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

namespace AdventOfCode2022.Day16
{
    [Solution(16)]
#if RELEASE
    [SolutionInput("Day16\\Input.txt", Enabled = true)]
#endif
#if DEBUG
    [SolutionInput("Day16\\InputTest.txt", Enabled = true)]
#endif
    internal class Day16 : Solution
    {
        public Day16(Input input) : base(input)
        {
        }

        private static void LogToConsole(object item)
        {
#if DEBUG
            Console.WriteLine(item.ToString());
#endif
        }

        public record ValveNode(string Name, int FlowRate, List<string> LinkedNodes)
        {
        }

        private int ParseNumber(ReadOnlySpan<char> text)
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

        private string ParseValveName(ref ReadOnlySpan<char> text)
        {
            while (!(text[0] >= 'A' && text[0] <='Z'))
            {
                text = text[1..];
            }
            var valveName = text.Slice(0, 2).ToString();
            text = text.Slice(2);
            return valveName.ToString();
        }

        public ValveNode ParseValveNode(ReadOnlySpan<char> valve)
        {
            var valveName = valve.Slice(6,2).ToString();
            var equalsIndex = valve.IndexOf('=');
            var remainder = valve.Slice(equalsIndex + 1);
            var flowRate = ParseNumber(remainder);

            List<string> linkedValveNames = new();
            while (remainder.Length > 0)
            {
                var linkedValveName = ParseValveName(ref remainder);
                linkedValveNames.Add(linkedValveName);
            }
            return new ValveNode(valveName, flowRate, linkedValveNames);
        }

        public Dictionary<string, ValveNode> ParseValveNodes(string text)
        {
            var result = new Dictionary<string, ValveNode>();
            foreach (var valveNodeText in text.SplitFast("\r\n"))
            {
                var valveNode = ParseValveNode(valveNodeText);
                result.Add(valveNode.Name, valveNode);
            }
            return result;
        }

        public class TunnelRoute
        {
            private static StringBuilder sb = new StringBuilder(512);

            public string CurrentNode { get; private set; }
            public string CurrentElephantNode { get; private set; }
            public long Flow { get; private set; }
            public long FlowRate { get; private set; }

            private HashSet<string> EnabledNodes;

            private string nodeKey = string.Empty;
            private string enabledNodesKey = string.Empty;


            public string NodeKey()
            {
                return $"{nodeKey}";
            }

            public TunnelRoute(string firstNode, string firstElephantNode = "")
            {
                CurrentNode = firstNode;
                CurrentElephantNode = firstElephantNode;

                sb.Append(CurrentNode);
                sb.Append("_");
                sb.Append(CurrentElephantNode);
                sb.Append("_");
                nodeKey = sb.ToString();
                sb.Clear();

                EnabledNodes = new HashSet<string>();
            }

            private TunnelRoute(string currentNode, string currentElephantNode, HashSet<string> enabledNodes, long flow, long flowRate, string enabledNodesKey)
            {
                CurrentNode = currentNode;
                CurrentElephantNode = currentElephantNode;

                sb.Append(CurrentNode);
                sb.Append("_");
                sb.Append(CurrentElephantNode);
                sb.Append("_");
                sb.Append(enabledNodesKey);
                nodeKey = sb.ToString();
                sb.Clear();

                EnabledNodes = enabledNodes;
                Flow = flow;
                FlowRate = flowRate;
                this.enabledNodesKey = enabledNodesKey;
            }

            public bool HasEnabledNode(string nodeName)
            {
                return EnabledNodes.Contains(nodeName);
            }

            public void AddFlow(Dictionary<string, ValveNode> allNodes)
            {
                var flowToAdd = 0;
                foreach (var enabledNode in EnabledNodes)
                {
                    var node = allNodes[enabledNode];
                    flowToAdd += node.FlowRate;
                }
                Flow += flowToAdd;
                FlowRate = flowToAdd;
            }

            private string ToDistinctKey(HashSet<string> hashSet)
            {
                var stringBuilder = new StringBuilder();
                foreach (var s in hashSet.OrderBy(s => s))
                {
                    stringBuilder.Append(s);
                }
                return stringBuilder.ToString();

            }

            public TunnelRoute DeepClone(string newNode, string elephantNewNode)
            {
                var newEnabledNodes = EnabledNodes;
                string newEnabledNodesKey = this.enabledNodesKey;
                if (newNode == CurrentNode)
                {
                    if (!newEnabledNodes.Contains(CurrentNode))
                    {
                        var temp = new HashSet<string>(newEnabledNodes);
                        temp.Add(CurrentNode);
                        newEnabledNodes = temp;
                    }
                }
                if (elephantNewNode == CurrentElephantNode && CurrentElephantNode != string.Empty)
                {
                    if (!newEnabledNodes.Contains(CurrentElephantNode))
                    {
                        var temp = ReferenceEquals(EnabledNodes, newEnabledNodes) ? new HashSet<string>(newEnabledNodes) : newEnabledNodes;
                        temp.Add(CurrentElephantNode);
                        newEnabledNodes = temp;
                    }

                }
                if (!ReferenceEquals(EnabledNodes, newEnabledNodes))
                {
                    newEnabledNodesKey = ToDistinctKey(newEnabledNodes);
                }
                return new TunnelRoute(newNode, elephantNewNode, newEnabledNodes, Flow, FlowRate, newEnabledNodesKey);
            }

        }

        private void AddOrUpdateNodeWithGreatestFlowRate(Dictionary<string, TunnelRoute> allRoutes, TunnelRoute maybeNewRoute)
        {
            var maybeNewRouteKey = maybeNewRoute.NodeKey();

            if (allRoutes.TryGetValue(maybeNewRouteKey, out var possibleDuplicatePath))
            {
                //We want the path with this key which has the greatest flow to be the winner.
                if (possibleDuplicatePath.Flow < maybeNewRoute.Flow)
                {
                    allRoutes[maybeNewRouteKey] = maybeNewRoute;
                }
            }
            else
            {
                allRoutes.Add(maybeNewRouteKey, maybeNewRoute);
            }
        }

        private void PrunePaths(Dictionary<string, TunnelRoute> allRoutes, Dictionary<string, ValveNode> valveNodes)
        {
            var maxPossibleFlow = valveNodes.Select(v => v.Value.FlowRate).Sum();
            var maxCurrentFlow = allRoutes.Max(kvp => kvp.Value.Flow);

            var maxCurrentFlowAllEnabled = allRoutes
                .Where(kvp => kvp.Value.FlowRate == maxPossibleFlow)
                .Select(kvp => kvp.Value.Flow);

            long maxFlowAllEnabled = 0;
            if (maxCurrentFlowAllEnabled.Any())
            {
                maxFlowAllEnabled = maxCurrentFlowAllEnabled.Max();
                if (maxCurrentFlow == maxFlowAllEnabled)
                {
                    var routeWithAllEnabledAndMaxFlow = allRoutes
                        .Where(kvp => kvp.Value.FlowRate == maxPossibleFlow && kvp.Value.Flow == maxFlowAllEnabled);

                    if (false && routeWithAllEnabledAndMaxFlow.Any())
                    {
                        var first = routeWithAllEnabledAndMaxFlow.First();
                        allRoutes.Clear();
                        allRoutes.Add(first.Key, first.Value);
                        return;
                    }
                }
            }

            var maxFlow = allRoutes.Max(kvp => kvp.Value.Flow);
            var maxFlowRate = allRoutes.Max(kvp => kvp.Value.FlowRate);
            var flowToPrune = Math.Max(maxFlowRate, 200);
            var routeKeys = allRoutes.Keys.ToList();

            foreach (var routeKey in routeKeys)
            {
                if (allRoutes.TryGetValue(routeKey, out var route))
                {
                    if (route.FlowRate == maxPossibleFlow && route.Flow < maxFlowAllEnabled)
                    {
                        allRoutes.Remove(routeKey);
                    }
                    else if (route.Flow < maxFlow - flowToPrune)
                    {
                        allRoutes.Remove(routeKey);
                    }
                }
            }

        }

        protected override string? Problem1()
        {
            var valveNodes = ParseValveNodes(Input.Raw);
            var initialValve = valveNodes["AA"];

            var allPaths = new Dictionary<string, TunnelRoute>();
            var newNode = new TunnelRoute("AA");
            allPaths.Add(newNode.NodeKey(), newNode);
            var time = 0;
            var maxFlowAllEnabled = valveNodes.Select(kvp => kvp.Value.FlowRate).Sum();

            while (time < 30)
            {
                var newPaths = new Dictionary<string, TunnelRoute>();
                foreach ((var key, var path) in allPaths)
                {
                    path.AddFlow(valveNodes);

                    if (path.FlowRate == maxFlowAllEnabled)
                    {
                        continue;
                    }

                    //First we see if the current node is enabled, if not
                    //we have a logic fork to add a new path enabling this node.
                    if (!path.HasEnabledNode(path.CurrentNode))
                    {
                        var valveNodeToEnable = valveNodes[path.CurrentNode];
                        if (valveNodeToEnable.FlowRate != 0)
                        {
                            var pathWithEnabledNode = path.DeepClone(path.CurrentNode, path.CurrentElephantNode);
                            AddOrUpdateNodeWithGreatestFlowRate(newPaths, pathWithEnabledNode);
                        }
                    }

                    //Then we see if there are other nodes which could be moved to,
                    //in which case we have a new logic fork for each new path created from these.
                    var currentNode = path.CurrentNode;
                    var node = valveNodes[currentNode];
                    foreach (var linkedNode in node.LinkedNodes)
                    {
                        var newPath = path.DeepClone(linkedNode, path.CurrentElephantNode);
                        AddOrUpdateNodeWithGreatestFlowRate(newPaths, newPath);
                    }

                    AddOrUpdateNodeWithGreatestFlowRate(newPaths, path);
                }
                allPaths = newPaths;
                time = time + 1;

                PrunePaths(allPaths, valveNodes);

                Console.WriteLine($"{time} - {allPaths.Select(p => p.Value.Flow).Max()}");
            }


             return allPaths.Select(p => p.Value.Flow).Max().ToString();
        }



        protected override string? Problem2()
        {
            var valveNodes = ParseValveNodes(Input.Raw);
            var initialValve = valveNodes["AA"];

            var allPaths = new Dictionary<string, TunnelRoute>();
            var newNode = new TunnelRoute("AA", "AA");
            allPaths.Add(newNode.NodeKey(), newNode);

            var maxFlowAllEnabled = valveNodes.Select(kvp => kvp.Value.FlowRate).Sum();

            var time = 0;
            while (time < 26)
            {
                var newPaths = new Dictionary<string, TunnelRoute>();
                foreach ((var key, var path) in allPaths)
                {
                    path.AddFlow(valveNodes);

                    if (path.FlowRate == maxFlowAllEnabled)
                    {
                        AddOrUpdateNodeWithGreatestFlowRate(newPaths, path);
                        continue;
                    }

                    var currentNodeName = path.CurrentNode;
                    var currentNode = valveNodes[currentNodeName];
                    var possibleNewNodes = (IEnumerable<string>)currentNode.LinkedNodes;
                    if (currentNode.FlowRate > 0 && !path.HasEnabledNode(currentNodeName))
                    {
                        possibleNewNodes = possibleNewNodes.Append(currentNodeName);
                    }

                    var currentElephantNodeName = path.CurrentElephantNode;
                    var currentElephantNode = valveNodes[currentElephantNodeName];
                    var possibleNewElephantNodes = new List<string>(currentElephantNode.LinkedNodes);
                    if (currentElephantNode.FlowRate > 0 && !path.HasEnabledNode(currentElephantNodeName))
                    {
                        possibleNewElephantNodes.Add(currentElephantNodeName);
                    }


                    foreach (var possibleNewNode in possibleNewNodes)
                    {
                        for (int j = 0; j < possibleNewElephantNodes.Count; j++)
                        {
                            var newPath = path.DeepClone(possibleNewNode, possibleNewElephantNodes[j]);
                            AddOrUpdateNodeWithGreatestFlowRate(newPaths, newPath);
                        }
                    }
                }
                allPaths = newPaths;
                time = time + 1;

                PrunePaths(allPaths, valveNodes);

                var maxFlow = allPaths.Select(p => p.Value.Flow).Max();
                var countWithMaxFlow = allPaths.Where(p => p.Value.Flow == maxFlow).Count();
                Console.WriteLine($"{time} - {maxFlow} - {countWithMaxFlow} - {allPaths.Count}");
            }


            return allPaths.Select(p => p.Value.Flow).Max().ToString();
        }
    }
}
