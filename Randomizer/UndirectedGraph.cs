using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;

namespace Hp2Randomizer;

public class UndirectedGraph
{
    public int NodeCount => _nodeCount;
    private int _nodeCount;

    public IEnumerable<(int a, int b)> Edges => _edges;
    private List<(int a, int b)> _edges = new List<(int a, int b)>();

    public UndirectedGraph(int nodeCount)
    {
        _nodeCount = nodeCount;
    }

    public void AddEdge(int a, int b)
    {
        if (a > _nodeCount || a < 0 || b > _nodeCount || b < 0)
        {
            throw new ArgumentException($"Node index out of scope [0,{_nodeCount})");
        }

        //always put the lower index first so we can rely on that when parsing
        //that way any duplicate connections will be identical
        _edges.Add(a > b ? (a, b) : (b, a));
    }

    public static bool TryMakeConnectedHHGraph(int[] degrees, out UndirectedGraph graph)
    {
        //degree must be even
        var degreeTotal = degrees.Sum();
        if (degreeTotal % 2 != 0)
        {
            graph = null;
            return false;
        }

        var edges = new (int a, int b)[degrees.Sum() / 2];
        graph = new UndirectedGraph(degrees.Length);

        //pair index with degree count, sort by remaining degree descending
        int i = 0;
        var sorted = degrees.Select(x => ((int id, int remainingDegree))(i++, x)).OrderByDescending(x => x.remainingDegree).ToArray();

        while (sorted.Length > 0)
        {
            //by always selecting the smallest $(degree) remaining, a connected graph will be made when possible
            var index = sorted.Length - 1;

            //there must be enough nodes to connect to
            if (sorted[index].remainingDegree > index)
            {
                return false;
            }

            //connect to the $(degree) largest degree count nodes
            for (i = 0; i < sorted[index].remainingDegree; i++)
            {
                sorted[i].remainingDegree--;
                graph.AddEdge(sorted[index].id, sorted[i].id);
            }

            //remove the handled node and any with 0 remaining connections, resort
            sorted = sorted.Take(index).Where(x => x.remainingDegree != 0).OrderByDescending(x => x.remainingDegree).ToArray();
        }

        return true;
    }


    public UndirectedGraph Randomize(int attempts, Random random)
    {
        var result = this.Duplicate();

        int swapCount = 0;
        for (int i = 0; i < attempts; i++)
        {
            var current = result.Duplicate();

            //select two different edges
            var indexA = random.Next() % current._edges.Count;
            var indexB = (indexA + 1 + (random.Next() % (current._edges.Count - 1))) % current._edges.Count;

            var edgeA = current._edges[indexA];
            var edgeB = current._edges[indexB];

            if (edgeA.a != edgeB.a && edgeA.b != edgeB.b)
            {
                swapCount++;
                current._edges[indexA] = (edgeA.a, edgeB.b);
                current._edges[indexB] = (edgeB.a, edgeA.b);

                if (current.IsConnected() && current.IsSimple())
                {
                    result = current;
                }
            }
        }

        return result;
    }

    private void DFS(int nodeIndex, int[] visited)
    {
        visited[nodeIndex] = 1;

        foreach (var edge in _edges.Where(x => x.a == nodeIndex))
        {
            if (visited[edge.b] == 0)
            {
                DFS(edge.b, visited);
            }
        }

        foreach (var edge in _edges.Where(x => x.b == nodeIndex))
        {
            if (visited[edge.a] == 0)
            {
                DFS(edge.a, visited);
            }
        }
    }

    public bool IsSimple()
    {
        var edgeSets = new HashSet<int>[_nodeCount];
        for (int i = 0; i < _nodeCount; i++)
        {
            edgeSets[i] = new HashSet<int>();
        }

        //no self loops or double loops
        foreach (var edge in _edges)
        {
            if (edge.a == edge.b)
            {
                return false;
            }

            var set = edgeSets[edge.a];
            if (set.Contains(edge.b))
            {
                return false;
            }
            else
            {
                set.Add(edge.b);
            }
        }

        return true;
    }

    public bool IsConnected()
    {
        var visited = new int[_nodeCount];
        DFS(0, visited);

        return !visited.Any(x => x == 0);
    }

    public UndirectedGraph Duplicate()
    {
        var result = new UndirectedGraph(_nodeCount);
        result._edges = _edges.Select(x => (x.a, x.b)).ToList();

        return result;
    }
}