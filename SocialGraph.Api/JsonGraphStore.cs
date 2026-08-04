using System.Text.Json;
using System.Text.Json.Serialization;
using SocialGraph.Core;

namespace SocialGraph.Api;

/// <summary>
/// Serializable snapshot of the whole network.
/// </summary>
public record GraphSnapshot(
    IReadOnlyList<string> People,
    IReadOnlyList<EdgeDto> Friendships);

/// <summary>
/// Thread-safe, JSON-file-backed store around the in-memory
/// <see cref="SocialNetwork"/> engine.
///
/// Persistence model: the full graph is loaded from a JSON file at startup
/// (seeded with a sample network if the file does not exist) and re-written
/// atomically after every successful mutation. This is deliberately simple —
/// a single JSON document guarded by one lock — and is plenty for a demo /
/// single-instance deployment.
///
/// Swapping to a real database (Neo4j or Postgres):
///  1. Extract the public members of this class into an IGraphStore interface
///     and register the new implementation in Program.cs DI instead of this one.
///  2. Neo4j: map AddPerson -> MERGE (:Person {name}), AddFriendship ->
///     MERGE (:Person)-[:FRIENDS_WITH]-&gt;(:Person), and push the query methods
///     (shortest path, mutual friends, recommendations, centrality) down into
///     Cypher (shortestPath(), graph-data-science library) instead of the
///     in-memory engine. Use the official Neo4j.Driver NuGet package.
///  3. Postgres: two tables — people(name PK) and friendships(from_name, to_name,
///     PK(from_name, to_name), FKs -> people ON DELETE CASCADE) — via EF Core or
///     Dapper. Either keep the algorithms in SocialGraph.Core by hydrating the
///     graph per request (fine for small graphs), or use recursive CTEs for
///     shortest path / reachability.
/// The HTTP surface in Program.cs would not need to change.
/// </summary>
public class JsonGraphStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly object _sync = new();
    private readonly SocialNetwork _network = new();
    private readonly string _filePath;

    public JsonGraphStore(string filePath)
    {
        _filePath = filePath;

        if (File.Exists(_filePath))
        {
            var snapshot = JsonSerializer.Deserialize<GraphSnapshot>(
                File.ReadAllText(_filePath), SerializerOptions)
                ?? throw new InvalidDataException(
                    $"Graph data file '{_filePath}' contains no graph.");
            foreach (var person in snapshot.People)
                _network.AddPerson(person);
            foreach (var edge in snapshot.Friendships)
                _network.AddFriendship(edge.Source, edge.Target);
        }
        else
        {
            SeedData.Populate(_network);
            Save();
        }
    }

    /// <summary>Runs a read-only query against the network under the lock.</summary>
    public T Read<T>(Func<SocialNetwork, T> query)
    {
        lock (_sync)
        {
            return query(_network);
        }
    }

    /// <summary>
    /// Runs a mutation under the lock and persists the graph to disk if the
    /// mutation reports it changed something (returns true).
    /// </summary>
    public bool Mutate(Func<SocialNetwork, bool> mutation)
    {
        lock (_sync)
        {
            var changed = mutation(_network);
            if (changed)
                Save();
            return changed;
        }
    }

    /// <summary>Current snapshot of the whole graph (nodes + directed edges).</summary>
    public GraphSnapshot Snapshot()
    {
        lock (_sync)
        {
            return SnapshotUnlocked();
        }
    }

    private GraphSnapshot SnapshotUnlocked()
    {
        var people = _network.People.ToList();
        var edges = new List<EdgeDto>(_network.FriendshipCount);
        foreach (var person in people)
        {
            foreach (var friend in _network.GetDirectFriends(person))
                edges.Add(new EdgeDto(person, friend));
        }
        return new GraphSnapshot(people, edges);
    }

    private void Save()
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(_filePath));
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        // Atomic-ish write: write to a temp file, then move over the target.
        var tempPath = _filePath + ".tmp";
        File.WriteAllText(
            tempPath,
            JsonSerializer.Serialize(SnapshotUnlocked(), SerializerOptions));
        File.Move(tempPath, _filePath, overwrite: true);
    }
}
