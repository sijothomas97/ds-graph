# Improvements — Social Network Graph

**Goal:** A deployed web app where users build a social graph, run friend-of-friend / shortest-path / centrality queries, and see it rendered as a live interactive network visualization.

## TL;DR — Path to production
- [x] Port graph engine to a .NET 8 library + xUnit tests
- [x] ASP.NET Core API + Swagger
- [x] Persistence with seed data — JSON-file store (`JsonGraphStore.cs`), not Neo4j/Postgres; swap-in notes for both documented in that file's doc comment
- [x] Frontend with interactive graph visualization — vanilla HTML/JS + hand-rolled force-directed SVG layout (no React, no build step, works offline), not a React SPA
- [x] Docker + GitHub Actions CI/CD
- [x] Richer algorithms (shortest path, mutual friends, recommendations, degree centrality) — no live public demo (no cloud deploy per project ground rules)

## Current state
- Legacy .NET Framework 4.7.2 WinForms desktop app (Windows-only), despite README calling it a "console app".
- Core is sound: adjacency-list directed graph with add/remove person, add friendship, direct friends, and DFS traversal for indirect friends.
- Purely in-memory, single-user, no persistence between runs.
- No tests, no CI, no containerization, no live demo, no way for anyone to try it.

## Key improvements
- **Port core to .NET 8**: extract the graph engine into a cross-platform class library, drop WinForms.
- **REST/GraphQL API**: ASP.NET Core minimal API exposing people, friendships, and graph queries (shortest path, BFS/DFS, degrees of separation).
- **Persistence**: back the graph with a real graph store (Neo4j) or EF Core + Postgres; seed with sample network.
- **Web frontend**: React/TypeScript SPA with interactive force-directed graph viz (D3 / Cytoscape.js / react-force-graph).
- **Richer graph algorithms**: shortest path, mutual friends, community detection, centrality/influencer ranking, friend recommendations.
- **Quality**: xUnit tests on the engine, correct README, OpenAPI/Swagger docs.

## Latest tech to showcase
- .NET 8 minimal APIs + C# 12; ASP.NET Core.
- Neo4j (Cypher) or EF Core + Postgres for graph persistence.
- React 18 + TypeScript + Vite; Cytoscape.js / react-force-graph for visualization.
- Docker + docker-compose; GitHub Actions CI/CD.
- Deploy to Azure Container Apps / Fly.io with a public live demo.

## Roadmap
1. Modernize: port engine to a .NET 8 library, add xUnit tests, wrap in an ASP.NET Core API with Swagger.
2. Persist & visualize: add Neo4j/Postgres backing store and a React graph-visualization frontend.
3. Ship: containerize, add GitHub Actions CI/CD, deploy a public live demo with richer algorithms (recommendations, centrality).
