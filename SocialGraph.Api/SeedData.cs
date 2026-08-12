using SocialGraph.Core;

namespace SocialGraph.Api;

/// <summary>
/// Seeds a small, interesting sample network: two friend clusters bridged by
/// Grace, a one-way "follower" edge, and one isolated newcomer — enough to
/// demo shortest paths, mutual friends, recommendations and centrality.
/// </summary>
public static class SeedData
{
    public static void Populate(SocialNetwork network)
    {
        string[] people =
        [
            "Alice", "Bob", "Carol", "Dave", "Eve", "Frank",
            "Grace", "Heidi", "Ivan", "Judy", "Mallory", "Niaj",
        ];
        foreach (var person in people)
            network.AddPerson(person);

        // Cluster 1: Alice / Bob / Carol / Dave / Eve (dense, mutual).
        AddMutual(network, "Alice", "Bob");
        AddMutual(network, "Alice", "Carol");
        AddMutual(network, "Alice", "Dave");
        AddMutual(network, "Bob", "Carol");
        AddMutual(network, "Bob", "Eve");
        AddMutual(network, "Carol", "Dave");
        AddMutual(network, "Dave", "Eve");

        // Grace bridges the two clusters.
        AddMutual(network, "Eve", "Grace");
        AddMutual(network, "Grace", "Heidi");

        // Cluster 2: Heidi / Ivan / Judy / Frank (mutual).
        AddMutual(network, "Heidi", "Ivan");
        AddMutual(network, "Heidi", "Judy");
        AddMutual(network, "Ivan", "Judy");
        AddMutual(network, "Judy", "Frank");
        AddMutual(network, "Ivan", "Frank");

        // Mallory follows Alice and Eve one-way (directed edges only).
        network.AddFriendship("Mallory", "Alice");
        network.AddFriendship("Mallory", "Eve");

        // Niaj just joined and has no friends yet (isolated node).
    }

    private static void AddMutual(SocialNetwork network, string a, string b)
    {
        network.AddFriendship(a, b);
        network.AddFriendship(b, a);
    }
}
