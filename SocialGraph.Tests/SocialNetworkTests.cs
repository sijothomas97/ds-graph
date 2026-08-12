using SocialGraph.Core;
using Xunit;

namespace SocialGraph.Tests;

public class PersonManagementTests
{
    [Fact]
    public void AddPerson_NewPerson_ReturnsTrueAndIsContained()
    {
        var net = new SocialNetwork();
        Assert.True(net.AddPerson("Alice"));
        Assert.True(net.ContainsPerson("Alice"));
        Assert.Equal(1, net.PersonCount);
    }

    [Fact]
    public void AddPerson_Duplicate_ReturnsFalseAndDoesNotDouble()
    {
        var net = new SocialNetwork();
        net.AddPerson("Alice");
        Assert.False(net.AddPerson("Alice"));
        Assert.Equal(1, net.PersonCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddPerson_BlankName_Throws(string name)
    {
        var net = new SocialNetwork();
        Assert.Throws<ArgumentException>(() => net.AddPerson(name));
    }

    [Fact]
    public void ContainsPerson_Unknown_ReturnsFalse()
    {
        var net = new SocialNetwork();
        Assert.False(net.ContainsPerson("Ghost"));
    }

    [Fact]
    public void RemovePerson_RemovesPersonAndAllIncidentEdges()
    {
        var net = Fixtures.Triangle(); // A->B, B->C, C->A
        Assert.True(net.RemovePerson("B"));
        Assert.False(net.ContainsPerson("B"));
        Assert.Equal(2, net.PersonCount);
        // A->B (in-edge of B) and B->C (out-edge of B) both gone; C->A remains.
        Assert.Equal(1, net.FriendshipCount);
        Assert.Empty(net.GetDirectFriends("A"));
        Assert.Equal(new[] { "A" }, net.GetDirectFriends("C"));
    }

    [Fact]
    public void RemovePerson_Unknown_ReturnsFalse()
    {
        var net = new SocialNetwork();
        Assert.False(net.RemovePerson("Ghost"));
    }

    [Fact]
    public void RemovePerson_WithSelfLoop_DecrementsEdgeCountOnce()
    {
        var net = new SocialNetwork();
        net.AddPerson("A");
        net.AddFriendship("A", "A");
        Assert.Equal(1, net.FriendshipCount);
        net.RemovePerson("A");
        Assert.Equal(0, net.FriendshipCount);
        Assert.Equal(0, net.PersonCount);
    }
}

public class FriendshipTests
{
    [Fact]
    public void AddFriendship_IsDirected()
    {
        var net = new SocialNetwork();
        net.AddPerson("A");
        net.AddPerson("B");
        Assert.True(net.AddFriendship("A", "B"));
        Assert.Equal(new[] { "B" }, net.GetDirectFriends("A"));
        Assert.Empty(net.GetDirectFriends("B"));
        Assert.Equal(1, net.FriendshipCount);
    }

    [Fact]
    public void AddFriendship_Duplicate_ReturnsFalseAndDoesNotDoubleCount()
    {
        var net = new SocialNetwork();
        net.AddPerson("A");
        net.AddPerson("B");
        net.AddFriendship("A", "B");
        Assert.False(net.AddFriendship("A", "B"));
        Assert.Equal(1, net.FriendshipCount);
    }

    [Fact]
    public void AddFriendship_UnknownPerson_Throws()
    {
        var net = new SocialNetwork();
        net.AddPerson("A");
        Assert.Throws<ArgumentException>(() => net.AddFriendship("A", "Ghost"));
        Assert.Throws<ArgumentException>(() => net.AddFriendship("Ghost", "A"));
    }

    [Fact]
    public void AddFriendship_SelfLoop_IsAllowedAndCountedOnce()
    {
        var net = new SocialNetwork();
        net.AddPerson("A");
        Assert.True(net.AddFriendship("A", "A"));
        Assert.Equal(1, net.FriendshipCount);
        Assert.Equal(new[] { "A" }, net.GetDirectFriends("A"));
    }

    [Fact]
    public void RemoveFriendship_RemovesOnlyThatDirection()
    {
        var net = new SocialNetwork();
        net.AddPerson("A");
        net.AddPerson("B");
        net.AddFriendship("A", "B");
        net.AddFriendship("B", "A");
        Assert.True(net.RemoveFriendship("A", "B"));
        Assert.False(net.RemoveFriendship("A", "B"));
        Assert.Equal(1, net.FriendshipCount);
        Assert.Equal(new[] { "A" }, net.GetDirectFriends("B"));
    }

    [Fact]
    public void GetDirectFriends_UnknownPerson_Throws()
    {
        var net = new SocialNetwork();
        Assert.Throws<ArgumentException>(() => net.GetDirectFriends("Ghost"));
    }
}

public class IndirectFriendsDfsTests
{
    [Fact]
    public void GetIndirectFriends_ReturnsAllReachableExcludingStart()
    {
        // A -> B -> C -> D, A -> C; E isolated
        var net = Fixtures.FromEdges(
            new[] { "A", "B", "C", "D", "E" },
            ("A", "B"), ("B", "C"), ("C", "D"), ("A", "C"));
        var result = net.GetIndirectFriends("A");
        Assert.Equal(new[] { "B", "C", "D" }, result.Order());
        Assert.DoesNotContain("A", result);
        Assert.DoesNotContain("E", result);
    }

    [Fact]
    public void GetIndirectFriends_FollowsEdgeDirection()
    {
        var net = Fixtures.FromEdges(new[] { "A", "B" }, ("A", "B"));
        Assert.Empty(net.GetIndirectFriends("B"));
    }

    [Fact]
    public void GetIndirectFriends_CycleTerminates()
    {
        var net = Fixtures.Triangle(); // A->B->C->A
        Assert.Equal(new[] { "B", "C" }, net.GetIndirectFriends("A").Order());
    }

    [Fact]
    public void GetIndirectFriends_SelfLoop_DoesNotIncludeSelfOrHang()
    {
        var net = Fixtures.FromEdges(
            new[] { "A", "B" }, ("A", "A"), ("A", "B"));
        Assert.Equal(new[] { "B" }, net.GetIndirectFriends("A"));
    }

    [Fact]
    public void GetIndirectFriends_IsolatedPerson_ReturnsEmpty()
    {
        var net = new SocialNetwork();
        net.AddPerson("Loner");
        Assert.Empty(net.GetIndirectFriends("Loner"));
    }
}

public class ShortestPathTests
{
    [Fact]
    public void GetShortestPath_PicksMinimumHopPath()
    {
        // Long route A->B->C->D and shortcut A->X->D
        var net = Fixtures.FromEdges(
            new[] { "A", "B", "C", "D", "X" },
            ("A", "B"), ("B", "C"), ("C", "D"), ("A", "X"), ("X", "D"));
        Assert.Equal(new[] { "A", "X", "D" }, net.GetShortestPath("A", "D"));
        Assert.Equal(2, net.GetDegreesOfSeparation("A", "D"));
    }

    [Fact]
    public void GetShortestPath_DirectEdge_LengthOne()
    {
        var net = Fixtures.FromEdges(new[] { "A", "B" }, ("A", "B"));
        Assert.Equal(new[] { "A", "B" }, net.GetShortestPath("A", "B"));
        Assert.Equal(1, net.GetDegreesOfSeparation("A", "B"));
    }

    [Fact]
    public void GetShortestPath_RespectsDirection()
    {
        var net = Fixtures.FromEdges(new[] { "A", "B" }, ("A", "B"));
        Assert.Null(net.GetShortestPath("B", "A"));
        Assert.Equal(-1, net.GetDegreesOfSeparation("B", "A"));
    }

    [Fact]
    public void GetShortestPath_DisconnectedGraph_ReturnsNull()
    {
        // Two components: {A,B} and {C,D}
        var net = Fixtures.FromEdges(
            new[] { "A", "B", "C", "D" }, ("A", "B"), ("C", "D"));
        Assert.Null(net.GetShortestPath("A", "D"));
        Assert.Equal(-1, net.GetDegreesOfSeparation("A", "D"));
    }

    [Fact]
    public void GetShortestPath_SamePerson_ZeroDegrees()
    {
        var net = new SocialNetwork();
        net.AddPerson("A");
        Assert.Equal(new[] { "A" }, net.GetShortestPath("A", "A"));
        Assert.Equal(0, net.GetDegreesOfSeparation("A", "A"));
    }

    [Fact]
    public void GetShortestPath_SamePersonWithSelfLoop_StillZeroDegrees()
    {
        var net = Fixtures.FromEdges(new[] { "A" }, ("A", "A"));
        Assert.Equal(new[] { "A" }, net.GetShortestPath("A", "A"));
        Assert.Equal(0, net.GetDegreesOfSeparation("A", "A"));
    }

    [Fact]
    public void GetShortestPath_SelfLoopOnRoute_DoesNotAffectResult()
    {
        var net = Fixtures.FromEdges(
            new[] { "A", "B", "C" }, ("A", "A"), ("A", "B"), ("B", "B"), ("B", "C"));
        Assert.Equal(new[] { "A", "B", "C" }, net.GetShortestPath("A", "C"));
    }

    [Fact]
    public void GetShortestPath_UnknownPerson_Throws()
    {
        var net = new SocialNetwork();
        net.AddPerson("A");
        Assert.Throws<ArgumentException>(() => net.GetShortestPath("A", "Ghost"));
        Assert.Throws<ArgumentException>(() => net.GetShortestPath("Ghost", "A"));
    }
}

public class MutualFriendsTests
{
    [Fact]
    public void GetMutualFriends_ReturnsIntersectionSorted()
    {
        var net = Fixtures.FromEdges(
            new[] { "A", "B", "C", "D", "E" },
            ("A", "C"), ("A", "D"), ("A", "E"),
            ("B", "C"), ("B", "D"));
        Assert.Equal(new[] { "C", "D" }, net.GetMutualFriends("A", "B"));
    }

    [Fact]
    public void GetMutualFriends_NoOverlap_ReturnsEmpty()
    {
        var net = Fixtures.FromEdges(
            new[] { "A", "B", "C", "D" }, ("A", "C"), ("B", "D"));
        Assert.Empty(net.GetMutualFriends("A", "B"));
    }

    [Fact]
    public void GetMutualFriends_ExcludesTheTwoPeopleThemselves()
    {
        // A and B both follow each other and both follow C.
        var net = Fixtures.FromEdges(
            new[] { "A", "B", "C" },
            ("A", "B"), ("B", "A"), ("A", "A"), ("B", "B"),
            ("A", "C"), ("B", "C"));
        Assert.Equal(new[] { "C" }, net.GetMutualFriends("A", "B"));
    }

    [Fact]
    public void GetMutualFriends_SamePersonBothArgs_SelfLoopExcluded()
    {
        var net = Fixtures.FromEdges(
            new[] { "A", "B" }, ("A", "A"), ("A", "B"));
        Assert.Equal(new[] { "B" }, net.GetMutualFriends("A", "A"));
    }

    [Fact]
    public void GetMutualFriends_UnknownPerson_Throws()
    {
        var net = new SocialNetwork();
        net.AddPerson("A");
        Assert.Throws<ArgumentException>(() => net.GetMutualFriends("A", "Ghost"));
    }
}

public class FriendRecommendationTests
{
    [Fact]
    public void Recommendations_RankedByMutualCount_TiesAlphabetical()
    {
        // A's friends: B, C, D.
        // E is known via B and C (2 mutuals); F via D (1); G via D (1).
        var net = Fixtures.FromEdges(
            new[] { "A", "B", "C", "D", "E", "F", "G" },
            ("A", "B"), ("A", "C"), ("A", "D"),
            ("B", "E"), ("C", "E"), ("D", "F"), ("D", "G"));
        var recs = net.GetFriendRecommendations("A");
        Assert.Equal(
            new[]
            {
                new FriendRecommendation("E", 2),
                new FriendRecommendation("F", 1),
                new FriendRecommendation("G", 1),
            },
            recs);
    }

    [Fact]
    public void Recommendations_ExcludeSelfAndExistingFriends()
    {
        // B follows A back (self would be a candidate) and C is already A's friend.
        var net = Fixtures.FromEdges(
            new[] { "A", "B", "C", "D" },
            ("A", "B"), ("A", "C"), ("B", "A"), ("B", "C"), ("B", "D"));
        var recs = net.GetFriendRecommendations("A");
        Assert.Equal(new[] { new FriendRecommendation("D", 1) }, recs);
    }

    [Fact]
    public void Recommendations_MaxResults_TruncatesAfterRanking()
    {
        var net = Fixtures.FromEdges(
            new[] { "A", "B", "C", "D", "E" },
            ("A", "B"), ("A", "C"),
            ("B", "D"), ("C", "D"), ("B", "E"));
        var recs = net.GetFriendRecommendations("A", maxResults: 1);
        Assert.Equal(new[] { new FriendRecommendation("D", 2) }, recs);
    }

    [Fact]
    public void Recommendations_SelfLoop_DoesNotRecommendSelf()
    {
        var net = Fixtures.FromEdges(
            new[] { "A", "B", "C" },
            ("A", "A"), ("A", "B"), ("B", "A"), ("B", "C"));
        var recs = net.GetFriendRecommendations("A");
        Assert.Equal(new[] { new FriendRecommendation("C", 1) }, recs);
    }

    [Fact]
    public void Recommendations_DisconnectedPerson_ReturnsEmpty()
    {
        var net = Fixtures.FromEdges(
            new[] { "A", "B", "C" }, ("B", "C"));
        Assert.Empty(net.GetFriendRecommendations("A"));
    }

    [Fact]
    public void Recommendations_NegativeMaxResults_Throws()
    {
        var net = new SocialNetwork();
        net.AddPerson("A");
        Assert.Throws<ArgumentOutOfRangeException>(
            () => net.GetFriendRecommendations("A", maxResults: -1));
    }
}

public class DegreeCentralityTests
{
    [Fact]
    public void Ranking_OrdersByTotalDegreeThenName()
    {
        // Hub B: out {A, C}, in {A, C} => total 4.
        // A: out {B}, in {B} => 2. C: out {B}, in {B} => 2. D isolated => 0.
        var net = Fixtures.FromEdges(
            new[] { "A", "B", "C", "D" },
            ("A", "B"), ("B", "A"), ("C", "B"), ("B", "C"));
        var ranking = net.GetDegreeCentralityRanking();
        Assert.Equal(
            new[]
            {
                new CentralityScore("B", 2, 2),
                new CentralityScore("A", 1, 1),
                new CentralityScore("C", 1, 1),
                new CentralityScore("D", 0, 0),
            },
            ranking);
    }

    [Fact]
    public void Ranking_SelfLoopCountsOnceInAndOnceOut()
    {
        var net = Fixtures.FromEdges(new[] { "A", "B" }, ("A", "A"));
        var ranking = net.GetDegreeCentralityRanking();
        Assert.Equal(new CentralityScore("A", 1, 1), ranking[0]);
        Assert.Equal(2, ranking[0].TotalDegree);
        Assert.Equal(new CentralityScore("B", 0, 0), ranking[1]);
    }

    [Fact]
    public void Ranking_EmptyNetwork_ReturnsEmpty()
    {
        Assert.Empty(new SocialNetwork().GetDegreeCentralityRanking());
    }

    [Fact]
    public void Ranking_ReflectsRemovals()
    {
        var net = Fixtures.Triangle();
        net.RemovePerson("A");
        var ranking = net.GetDegreeCentralityRanking();
        // Remaining: B->C. B: out 1 in 0; C: out 0 in 1 — tie broken by name.
        Assert.Equal(
            new[]
            {
                new CentralityScore("B", 1, 0),
                new CentralityScore("C", 0, 1),
            },
            ranking);
    }
}

internal static class Fixtures
{
    /// <summary>A->B, B->C, C->A.</summary>
    public static SocialNetwork Triangle() =>
        FromEdges(new[] { "A", "B", "C" }, ("A", "B"), ("B", "C"), ("C", "A"));

    public static SocialNetwork FromEdges(
        string[] people, params (string From, string To)[] edges)
    {
        var net = new SocialNetwork();
        foreach (var p in people)
            net.AddPerson(p);
        foreach (var (from, to) in edges)
            net.AddFriendship(from, to);
        return net;
    }
}
