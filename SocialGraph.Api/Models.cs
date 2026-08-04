namespace SocialGraph.Api;

/// <summary>Request body for creating a person.</summary>
public record PersonRequest(string Name);

/// <summary>
/// Request body for creating a friendship edge. Edges are directed;
/// set <see cref="Mutual"/> to true to also add the reverse edge.
/// </summary>
public record FriendshipRequest(string From, string To, bool Mutual = false);

/// <summary>A directed edge in the graph.</summary>
public record EdgeDto(string Source, string Target);

/// <summary>A node in the graph.</summary>
public record NodeDto(string Id);

/// <summary>The whole graph, ready for visualization.</summary>
public record GraphDto(IReadOnlyList<NodeDto> Nodes, IReadOnlyList<EdgeDto> Edges);

/// <summary>Shortest-path query result.</summary>
public record PathDto(string From, string To, IReadOnlyList<string>? Path, int Degrees);

/// <summary>Mutual-friends query result.</summary>
public record MutualFriendsDto(string A, string B, IReadOnlyList<string> MutualFriends);
