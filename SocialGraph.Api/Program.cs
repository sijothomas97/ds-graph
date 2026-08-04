using SocialGraph.Api;
using SocialGraph.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Social Graph API",
        Version = "v1",
        Description =
            "People, friendships and graph queries (shortest path, mutual " +
            "friends, recommendations, degree centrality) over an in-memory " +
            "social graph, persisted to a JSON file on disk. See " +
            "JsonGraphStore.cs for notes on swapping in Neo4j or Postgres.",
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Where the graph is persisted. Overridable via GRAPH_DATA_PATH for
// container/deploy environments (e.g. a mounted volume).
var dataPath = builder.Configuration["GRAPH_DATA_PATH"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "data", "graph.json");

builder.Services.AddSingleton(new JsonGraphStore(dataPath));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Social Graph API v1");
});
app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

var api = app.MapGroup("/api");

// ---------------------------------------------------------------- People --

api.MapGet("/people", (JsonGraphStore store) =>
    Results.Ok(store.Read(n => n.People.Order(StringComparer.Ordinal).ToList())))
    .WithName("GetPeople")
    .WithSummary("List all people in the network.");

api.MapPost("/people", (PersonRequest request, JsonGraphStore store) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new { error = "Name must not be empty." });

    var added = store.Mutate(n => n.AddPerson(request.Name));
    return added
        ? Results.Created($"/api/people/{Uri.EscapeDataString(request.Name)}", new { name = request.Name })
        : Results.Conflict(new { error = $"Person '{request.Name}' already exists." });
})
    .WithName("AddPerson")
    .WithSummary("Add a new person to the network.");

api.MapDelete("/people/{name}", (string name, JsonGraphStore store) =>
{
    var removed = store.Mutate(n => n.RemovePerson(name));
    return removed ? Results.NoContent() : Results.NotFound(new { error = $"Person '{name}' not found." });
})
    .WithName("RemovePerson")
    .WithSummary("Remove a person and every friendship touching them.");

api.MapGet("/people/{name}/friends", (string name, JsonGraphStore store) =>
{
    try
    {
        return Results.Ok(store.Read(n => n.GetDirectFriends(name)));
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
})
    .WithName("GetDirectFriends")
    .WithSummary("Direct (outgoing) friends of a person.");

api.MapGet("/people/{name}/network", (string name, JsonGraphStore store) =>
{
    try
    {
        return Results.Ok(store.Read(n => n.GetIndirectFriends(name)));
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
})
    .WithName("GetExtendedNetwork")
    .WithSummary("Everyone reachable from a person (direct + indirect friends).");

// ----------------------------------------------------------- Friendships --

api.MapPost("/friendships", (FriendshipRequest request, JsonGraphStore store) =>
{
    try
    {
        var changed = store.Mutate(n =>
        {
            var added = n.AddFriendship(request.From, request.To);
            if (request.Mutual)
                added |= n.AddFriendship(request.To, request.From);
            return added;
        });
        return changed
            ? Results.Created($"/api/friendships/{Uri.EscapeDataString(request.From)}/{Uri.EscapeDataString(request.To)}",
                new EdgeDto(request.From, request.To))
            : Results.Conflict(new { error = "Friendship already exists." });
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
})
    .WithName("AddFriendship")
    .WithSummary("Add a directed friendship edge (optionally mutual).");

api.MapDelete("/friendships/{from}/{to}", (string from, string to, JsonGraphStore store) =>
{
    var removed = store.Mutate(n => n.RemoveFriendship(from, to));
    return removed ? Results.NoContent() : Results.NotFound(new { error = "Friendship not found." });
})
    .WithName("RemoveFriendship")
    .WithSummary("Remove a directed friendship edge.");

// ---------------------------------------------------------------- Queries --

api.MapGet("/path", (string from, string to, JsonGraphStore store) =>
{
    try
    {
        var path = store.Read(n => n.GetShortestPath(from, to));
        var degrees = path is null ? -1 : path.Count - 1;
        return Results.Ok(new PathDto(from, to, path, degrees));
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
})
    .WithName("GetShortestPath")
    .WithSummary("Shortest directed path and degrees of separation between two people.");

api.MapGet("/mutual-friends", (string a, string b, JsonGraphStore store) =>
{
    try
    {
        var mutual = store.Read(n => n.GetMutualFriends(a, b));
        return Results.Ok(new MutualFriendsDto(a, b, mutual));
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
})
    .WithName("GetMutualFriends")
    .WithSummary("People who are direct friends of both A and B.");

api.MapGet("/people/{name}/recommendations", (string name, int? max, JsonGraphStore store) =>
{
    try
    {
        var results = store.Read(n => n.GetFriendRecommendations(name, max ?? int.MaxValue));
        return Results.Ok(results);
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
})
    .WithName("GetFriendRecommendations")
    .WithSummary("Friend-of-friend recommendations ranked by mutual-friend count.");

api.MapGet("/centrality", (JsonGraphStore store) =>
    Results.Ok(store.Read(n => n.GetDegreeCentralityRanking())))
    .WithName("GetCentralityRanking")
    .WithSummary("Everyone ranked by degree centrality (in + out degree).");

// ----------------------------------------------------------------- Graph --

app.MapGet("/graph", (JsonGraphStore store) =>
{
    var snapshot = store.Snapshot();
    var nodes = snapshot.People.Select(p => new NodeDto(p)).ToList();
    return Results.Ok(new GraphDto(nodes, snapshot.Friendships));
})
    .WithName("GetGraph")
    .WithSummary("Full graph as nodes + edges, ready for visualization.");

app.Run();

// Exposed for WebApplicationFactory-based integration tests.
public partial class Program { }
