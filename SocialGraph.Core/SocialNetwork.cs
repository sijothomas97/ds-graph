namespace SocialGraph.Core;

/// <summary>
/// A directed social graph backed by adjacency sets.
/// Ported from the legacy .NET Framework 4.7.2 WinForms engine
/// (legacy/src/Graph.cs + GraphNode.cs) and extended with
/// BFS shortest path, mutual friends, friend recommendations and
/// degree-centrality ranking.
///
/// Semantics preserved from the legacy engine:
///  - edges are directed (AddFriendship(a, b) does NOT add b -> a);
///  - self-loops are permitted (the legacy code never guarded against them);
///  - removing a person also removes every edge pointing at them.
///
/// Improvements over the legacy engine:
///  - O(1) person lookup (dictionary instead of linked-list scan);
///  - duplicate edges are ignored (adjacency is a set);
///  - unknown-person arguments throw ArgumentException instead of
///    NullReferenceException.
/// </summary>
public class SocialNetwork
{
    private readonly Dictionary<string, HashSet<string>> _adjacency =
        new(StringComparer.Ordinal);

    /// <summary>Number of people in the network.</summary>
    public int PersonCount => _adjacency.Count;

    /// <summary>Number of directed friendship edges in the network.</summary>
    public int FriendshipCount { get; private set; }

    /// <summary>All people in the network, in insertion order.</summary>
    public IReadOnlyCollection<string> People => _adjacency.Keys;

    /// <summary>
    /// Adds a person. Returns false (no-op) if the person already exists.
    /// </summary>
    public bool AddPerson(string name)
    {
        ValidateName(name);
        if (_adjacency.ContainsKey(name))
            return false;
        _adjacency[name] = new HashSet<string>(StringComparer.Ordinal);
        return true;
    }

    /// <summary>True if the person exists in the network.</summary>
    public bool ContainsPerson(string name) =>
        name is not null && _adjacency.ContainsKey(name);

    /// <summary>
    /// Removes a person and every edge to or from them.
    /// Returns false if the person does not exist.
    /// </summary>
    public bool RemovePerson(string name)
    {
        if (name is null || !_adjacency.TryGetValue(name, out var outgoing))
            return false;

        FriendshipCount -= outgoing.Count;
        _adjacency.Remove(name);

        foreach (var otherAdj in _adjacency.Values)
        {
            if (otherAdj.Remove(name))
                FriendshipCount--;
        }
        return true;
    }

    /// <summary>
    /// Adds a directed friendship edge from <paramref name="from"/> to
    /// <paramref name="to"/>. Both people must already exist.
    /// Returns false (no-op) if the edge already exists.
    /// </summary>
    public bool AddFriendship(string from, string to)
    {
        var fromAdj = RequirePerson(from);
        RequirePerson(to);
        if (!fromAdj.Add(to))
            return false;
        FriendshipCount++;
        return true;
    }

    /// <summary>
    /// Removes the directed edge from -> to. Returns false if it did not exist.
    /// </summary>
    public bool RemoveFriendship(string from, string to)
    {
        if (from is null || to is null ||
            !_adjacency.TryGetValue(from, out var fromAdj))
            return false;
        if (!fromAdj.Remove(to))
            return false;
        FriendshipCount--;
        return true;
    }

    /// <summary>
    /// The direct (out-edge) friends of a person, sorted alphabetically.
    /// </summary>
    public IReadOnlyList<string> GetDirectFriends(string name)
    {
        var adj = RequirePerson(name);
        return adj.Order(StringComparer.Ordinal).ToList();
    }

    /// <summary>
    /// All people reachable from <paramref name="start"/> via iterative DFS,
    /// excluding <paramref name="start"/> itself — i.e. direct plus indirect
    /// friends. Ported from the legacy DepthFirstTraverse; safe on cycles and
    /// self-loops.
    /// </summary>
    public IReadOnlyList<string> GetIndirectFriends(string start)
    {
        RequirePerson(start);

        var visited = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<string>();
        var toVisit = new Stack<string>();
        toVisit.Push(start);

        while (toVisit.Count > 0)
        {
            var current = toVisit.Pop();
            if (!visited.Add(current))
                continue;
            if (current != start)
                result.Add(current);
            foreach (var neighbour in _adjacency[current])
            {
                if (!visited.Contains(neighbour))
                    toVisit.Push(neighbour);
            }
        }
        return result;
    }

    /// <summary>
    /// BFS shortest path from <paramref name="from"/> to <paramref name="to"/>
    /// following directed edges. Returns the full path including both
    /// endpoints, or null if <paramref name="to"/> is unreachable.
    /// A path from a person to themselves is the single-element path [from],
    /// even when a self-loop exists.
    /// </summary>
    public IReadOnlyList<string>? GetShortestPath(string from, string to)
    {
        RequirePerson(from);
        RequirePerson(to);

        if (from == to)
            return new[] { from };

        var previous = new Dictionary<string, string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal) { from };
        var queue = new Queue<string>();
        queue.Enqueue(from);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var neighbour in _adjacency[current])
            {
                if (!visited.Add(neighbour))
                    continue;
                previous[neighbour] = current;
                if (neighbour == to)
                    return ReconstructPath(previous, from, to);
                queue.Enqueue(neighbour);
            }
        }
        return null;
    }

    /// <summary>
    /// Degrees of separation (number of edges on the shortest directed path)
    /// between two people. 0 for the same person, -1 if unreachable.
    /// </summary>
    public int GetDegreesOfSeparation(string from, string to)
    {
        var path = GetShortestPath(from, to);
        return path is null ? -1 : path.Count - 1;
    }

    /// <summary>
    /// People who are direct friends of both <paramref name="a"/> and
    /// <paramref name="b"/>, excluding a and b themselves, sorted
    /// alphabetically.
    /// </summary>
    public IReadOnlyList<string> GetMutualFriends(string a, string b)
    {
        var adjA = RequirePerson(a);
        var adjB = RequirePerson(b);

        return adjA.Intersect(adjB, StringComparer.Ordinal)
                   .Where(p => p != a && p != b)
                   .Order(StringComparer.Ordinal)
                   .ToList();
    }

    /// <summary>
    /// Friend recommendations for a person: friends-of-friends who are not
    /// already direct friends (and not the person themselves), ranked by the
    /// number of mutual friends (descending), ties broken alphabetically.
    /// </summary>
    public IReadOnlyList<FriendRecommendation> GetFriendRecommendations(
        string name, int maxResults = int.MaxValue)
    {
        if (maxResults < 0)
            throw new ArgumentOutOfRangeException(nameof(maxResults));
        var directFriends = RequirePerson(name);

        var mutualCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var friend in directFriends)
        {
            if (friend == name)
                continue; // self-loop: skip
            foreach (var candidate in _adjacency[friend])
            {
                if (candidate == name || directFriends.Contains(candidate))
                    continue;
                mutualCounts[candidate] =
                    mutualCounts.GetValueOrDefault(candidate) + 1;
            }
        }

        return mutualCounts
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key, StringComparer.Ordinal)
            .Take(maxResults)
            .Select(kv => new FriendRecommendation(kv.Key, kv.Value))
            .ToList();
    }

    /// <summary>
    /// Every person ranked by degree centrality (descending), ties broken
    /// alphabetically. Out-degree counts people they befriended, in-degree
    /// counts people who befriended them; a self-loop contributes 1 to each.
    /// Ranking is by total degree (out + in).
    /// </summary>
    public IReadOnlyList<CentralityScore> GetDegreeCentralityRanking()
    {
        var inDegree = _adjacency.Keys.ToDictionary(
            k => k, _ => 0, StringComparer.Ordinal);
        foreach (var adj in _adjacency.Values)
        {
            foreach (var target in adj)
                inDegree[target]++;
        }

        return _adjacency
            .Select(kv => new CentralityScore(
                kv.Key, kv.Value.Count, inDegree[kv.Key]))
            .OrderByDescending(s => s.TotalDegree)
            .ThenBy(s => s.Name, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<string> ReconstructPath(
        Dictionary<string, string> previous, string from, string to)
    {
        var path = new List<string> { to };
        var current = to;
        while (current != from)
        {
            current = previous[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }

    private HashSet<string> RequirePerson(string name)
    {
        ValidateName(name);
        if (!_adjacency.TryGetValue(name, out var adj))
            throw new ArgumentException(
                $"Person '{name}' does not exist in the network.", nameof(name));
        return adj;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Person name must be a non-empty, non-whitespace string.",
                nameof(name));
    }
}

/// <summary>A friend recommendation with its mutual-friend count.</summary>
public readonly record struct FriendRecommendation(string Name, int MutualFriendCount);

/// <summary>Degree-centrality score for one person.</summary>
public readonly record struct CentralityScore(string Name, int OutDegree, int InDegree)
{
    public int TotalDegree => OutDegree + InDegree;
}
