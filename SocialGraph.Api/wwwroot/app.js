// Social Graph Explorer — vanilla JS frontend.
// No build step, no external dependencies: a hand-rolled force-directed
// layout drawn onto an inline SVG, plus fetch() calls against the
// SocialGraph.Api endpoints. Designed to work fully offline.

const API = ""; // same-origin

// ---------------------------------------------------------------------
// Force-directed layout engine (simple Fruchterman-Reingold-ish physics:
// spring attraction along edges, repulsion between all node pairs,
// mild centering gravity, velocity damping). Runs on a fixed tick loop.
// ---------------------------------------------------------------------
class ForceLayout {
  constructor(width, height) {
    this.width = width;
    this.height = height;
    this.nodes = []; // {id, x, y, vx, vy, fixed}
    this.edges = []; // {source, target} referencing node ids
  }

  setData(nodeIds, edgePairs) {
    const existing = new Map(this.nodes.map(n => [n.id, n]));
    this.nodes = nodeIds.map(id => {
      const prev = existing.get(id);
      if (prev) return prev;
      const angle = Math.random() * Math.PI * 2;
      const r = Math.min(this.width, this.height) * 0.3;
      return {
        id,
        x: this.width / 2 + r * Math.cos(angle),
        y: this.height / 2 + r * Math.sin(angle),
        vx: 0,
        vy: 0,
        fixed: false,
      };
    });
    const byId = new Map(this.nodes.map(n => [n.id, n]));
    this.edges = edgePairs
      .filter(e => byId.has(e.source) && byId.has(e.target) && e.source !== e.target)
      .map(e => ({ source: byId.get(e.source), target: byId.get(e.target) }));
  }

  reheat() {
    for (const n of this.nodes) {
      if (n.fixed) continue;
      const angle = Math.random() * Math.PI * 2;
      const r = Math.min(this.width, this.height) * 0.3;
      n.x = this.width / 2 + r * Math.cos(angle);
      n.y = this.height / 2 + r * Math.sin(angle);
      n.vx = 0;
      n.vy = 0;
    }
  }

  resize(width, height) {
    this.width = width;
    this.height = height;
  }

  tick() {
    const n = this.nodes;
    const k = Math.sqrt((this.width * this.height) / Math.max(1, n.length)) * 0.9; // ideal distance
    const repulseStrength = k * k;

    // Repulsion between all pairs (O(n^2) — fine for graphs of a few hundred nodes).
    for (let i = 0; i < n.length; i++) {
      for (let j = i + 1; j < n.length; j++) {
        const a = n[i], b = n[j];
        let dx = a.x - b.x;
        let dy = a.y - b.y;
        let distSq = dx * dx + dy * dy;
        if (distSq < 0.01) { dx = (Math.random() - 0.5); dy = (Math.random() - 0.5); distSq = 0.01; }
        const dist = Math.sqrt(distSq);
        const force = repulseStrength / distSq;
        const fx = (dx / dist) * force;
        const fy = (dy / dist) * force;
        if (!a.fixed) { a.vx += fx; a.vy += fy; }
        if (!b.fixed) { b.vx -= fx; b.vy -= fy; }
      }
    }

    // Spring attraction along edges.
    for (const e of this.edges) {
      const a = e.source, b = e.target;
      let dx = b.x - a.x;
      let dy = b.y - a.y;
      let dist = Math.sqrt(dx * dx + dy * dy) || 0.01;
      const force = (dist - k) / k * k * 0.06;
      const fx = (dx / dist) * force;
      const fy = (dy / dist) * force;
      if (!a.fixed) { a.vx += fx; a.vy += fy; }
      if (!b.fixed) { b.vx -= fx; b.vy -= fy; }
    }

    // Gravity toward center + damping + integrate.
    const cx = this.width / 2, cy = this.height / 2;
    const damping = 0.82;
    for (const node of n) {
      if (node.fixed) continue;
      node.vx += (cx - node.x) * 0.002;
      node.vy += (cy - node.y) * 0.002;
      node.vx *= damping;
      node.vy *= damping;
      // clamp velocity to avoid explosions
      const speed = Math.sqrt(node.vx * node.vx + node.vy * node.vy);
      const maxSpeed = 40;
      if (speed > maxSpeed) {
        node.vx = (node.vx / speed) * maxSpeed;
        node.vy = (node.vy / speed) * maxSpeed;
      }
      node.x += node.vx * 0.15;
      node.y += node.vy * 0.15;
      const margin = 30;
      node.x = Math.max(margin, Math.min(this.width - margin, node.x));
      node.y = Math.max(margin, Math.min(this.height - margin, node.y));
    }
  }
}

// ---------------------------------------------------------------------
// Graph renderer: owns the SVG, draws nodes/edges from the layout each
// frame, and wires up click/drag interaction.
// ---------------------------------------------------------------------
class GraphView {
  constructor(svgEl, onNodeClick) {
    this.svg = svgEl;
    this.onNodeClick = onNodeClick;
    this.layout = new ForceLayout(svgEl.clientWidth || 800, svgEl.clientHeight || 600);
    this.selectedId = null;
    this.highlightEdges = new Set(); // "a->b" strings
    this.highlightNodes = new Set();
    this.dragNode = null;
    this.running = false;

    this.defsSetup();
    this.edgeLayer = this._svgEl("g", { class: "edges" });
    this.nodeLayer = this._svgEl("g", { class: "nodes" });
    this.svg.appendChild(this.edgeLayer);
    this.svg.appendChild(this.nodeLayer);

    this.edgeEls = new Map(); // "a->b" -> line
    this.nodeEls = new Map(); // id -> {g, circle, text}

    window.addEventListener("resize", () => {
      const rect = this.svg.getBoundingClientRect();
      this.layout.resize(rect.width, rect.height);
    });

    this.svg.addEventListener("pointermove", (e) => this._onPointerMove(e));
    this.svg.addEventListener("pointerup", () => { this.dragNode = null; });
    this.svg.addEventListener("pointerleave", () => { this.dragNode = null; });
  }

  defsSetup() {
    const defs = this._svgEl("defs");
    const marker = this._svgEl("marker", {
      id: "arrow", viewBox: "0 0 10 10", refX: "9", refY: "5",
      markerWidth: "7", markerHeight: "7", orient: "auto-start-reverse",
    });
    const path = this._svgEl("path", { d: "M 0 0 L 10 5 L 0 10 z", class: "arrowhead" });
    marker.appendChild(path);
    defs.appendChild(marker);
    this.svg.appendChild(defs);
  }

  _svgEl(tag, attrs = {}) {
    const el = document.createElementNS("http://www.w3.org/2000/svg", tag);
    for (const [k, v] of Object.entries(attrs)) el.setAttribute(k, v);
    return el;
  }

  setGraph(nodes, edges) {
    const rect = this.svg.getBoundingClientRect();
    this.layout.resize(rect.width || 800, rect.height || 600);
    this.layout.setData(nodes.map(n => n.id), edges.map(e => ({ source: e.source, target: e.target })));
    this._rebuildDom(nodes, edges);
    if (!this.running) {
      this.running = true;
      requestAnimationFrame(() => this._frame());
    }
  }

  _rebuildDom(nodes, edges) {
    this.edgeLayer.innerHTML = "";
    this.nodeLayer.innerHTML = "";
    this.edgeEls.clear();
    this.nodeEls.clear();

    const degree = new Map(nodes.map(n => [n.id, 0]));
    for (const e of edges) {
      degree.set(e.source, (degree.get(e.source) || 0) + 1);
      degree.set(e.target, (degree.get(e.target) || 0) + 1);
    }

    for (const e of edges) {
      const key = `${e.source}->${e.target}`;
      const line = this._svgEl("line", { class: "edge", "marker-end": "url(#arrow)" });
      this.edgeLayer.appendChild(line);
      this.edgeEls.set(key, line);
    }

    for (const node of nodes) {
      const isolated = (degree.get(node.id) || 0) === 0;
      const g = this._svgEl("g", { class: `node${isolated ? " isolated" : ""}`, "data-id": node.id });
      const r = isolated ? 8 : 10 + Math.min(8, (degree.get(node.id) || 0));
      const circle = this._svgEl("circle", { r: String(r) });
      const text = this._svgEl("text", { dy: -(r + 6) });
      text.textContent = node.id;
      g.appendChild(circle);
      g.appendChild(text);
      g.addEventListener("pointerdown", (e) => this._onNodePointerDown(e, node.id));
      g.addEventListener("click", () => this.onNodeClick && this.onNodeClick(node.id));
      this.nodeLayer.appendChild(g);
      this.nodeEls.set(node.id, { g, circle, text, r });
    }
    this.applySelection(this.selectedId);
    this.applyHighlight(this.highlightNodes, this.highlightEdges);
  }

  _onNodePointerDown(e, id) {
    e.stopPropagation();
    const layoutNode = this.layout.nodes.find(n => n.id === id);
    if (!layoutNode) return;
    this.dragNode = layoutNode;
    layoutNode.fixed = true;
  }

  _onPointerMove(e) {
    if (!this.dragNode) return;
    const rect = this.svg.getBoundingClientRect();
    this.dragNode.x = e.clientX - rect.left;
    this.dragNode.y = e.clientY - rect.top;
    this.dragNode.vx = 0;
    this.dragNode.vy = 0;
  }

  releaseDrag() {
    // Called on pointerup globally; keep node fixed where dropped so the
    // user's arrangement sticks, but allow physics to keep others moving.
  }

  reLayout() {
    for (const n of this.layout.nodes) n.fixed = false;
    this.layout.reheat();
  }

  applySelection(id) {
    this.selectedId = id;
    for (const [nodeId, els] of this.nodeEls) {
      els.circle.classList.toggle("selected", nodeId === id);
    }
  }

  applyHighlight(nodeIds = new Set(), edgeKeys = new Set()) {
    this.highlightNodes = nodeIds;
    this.highlightEdges = edgeKeys;
    for (const [nodeId, els] of this.nodeEls) {
      els.circle.classList.toggle("highlight", nodeIds.has(nodeId));
    }
    for (const [key, line] of this.edgeEls) {
      line.classList.toggle("highlight", edgeKeys.has(key));
    }
  }

  clearHighlight() {
    this.applyHighlight(new Set(), new Set());
  }

  _frame() {
    this.layout.tick();
    for (const [key, line] of this.edgeEls) {
      const [s, t] = key.split("->");
      const sn = this.layout.nodes.find(n => n.id === s);
      const tn = this.layout.nodes.find(n => n.id === t);
      if (!sn || !tn) continue;
      line.setAttribute("x1", sn.x);
      line.setAttribute("y1", sn.y);
      line.setAttribute("x2", tn.x);
      line.setAttribute("y2", tn.y);
    }
    for (const node of this.layout.nodes) {
      const els = this.nodeEls.get(node.id);
      if (!els) continue;
      els.g.setAttribute("transform", `translate(${node.x}, ${node.y})`);
    }
    requestAnimationFrame(() => this._frame());
  }
}

// ---------------------------------------------------------------------
// App wiring
// ---------------------------------------------------------------------
const statusEl = document.getElementById("status");
const svg = document.getElementById("graph-svg");
let allPeople = [];
let allEdges = [];
let graphView;

function setStatus(text, isError = false) {
  statusEl.textContent = text;
  statusEl.style.color = isError ? "#ff6b6b" : "";
}

async function api(path, options) {
  const res = await fetch(API + path, options);
  let body = null;
  try { body = await res.json(); } catch { /* no body */ }
  if (!res.ok) {
    const message = (body && body.error) || `${res.status} ${res.statusText}`;
    throw new Error(message);
  }
  return body;
}

function fillSelect(select, people, placeholder) {
  const prev = select.value;
  select.innerHTML = "";
  if (placeholder) {
    const opt = document.createElement("option");
    opt.value = "";
    opt.textContent = placeholder;
    select.appendChild(opt);
  }
  for (const p of people) {
    const opt = document.createElement("option");
    opt.value = p;
    opt.textContent = p;
    select.appendChild(opt);
  }
  if (people.includes(prev)) select.value = prev;
}

function refreshPeopleSelects() {
  fillSelect(document.getElementById("path-from"), allPeople);
  fillSelect(document.getElementById("path-to"), allPeople);
  fillSelect(document.getElementById("mutual-a"), allPeople);
  fillSelect(document.getElementById("mutual-b"), allPeople);
  fillSelect(document.getElementById("friend-from"), allPeople);
  fillSelect(document.getElementById("friend-to"), allPeople);
}

async function loadGraph() {
  setStatus("Loading graph…");
  const graph = await api("/graph");
  allPeople = graph.nodes.map(n => n.id).sort((a, b) => a.localeCompare(b));
  allEdges = graph.edges;
  graphView.setGraph(graph.nodes, graph.edges);
  refreshPeopleSelects();
  setStatus(`${graph.nodes.length} people · ${graph.edges.length} friendships`);
}

function chip(text, extraClass) {
  const li = document.createElement("li");
  li.textContent = text;
  if (extraClass) li.className = extraClass;
  return li;
}

function fillChipList(ul, items, emptyText) {
  ul.innerHTML = "";
  if (!items || items.length === 0) {
    ul.appendChild(chip(emptyText, "empty"));
    return;
  }
  for (const item of items) ul.appendChild(chip(item));
}

async function showNodeDetails(name) {
  document.getElementById("node-empty").hidden = true;
  const details = document.getElementById("node-details");
  details.hidden = false;
  document.getElementById("node-name").textContent = name;

  graphView.applySelection(name);

  const [friends, recs, network] = await Promise.all([
    api(`/api/people/${encodeURIComponent(name)}/friends`).catch(() => []),
    api(`/api/people/${encodeURIComponent(name)}/recommendations?max=8`).catch(() => []),
    api(`/api/people/${encodeURIComponent(name)}/network`).catch(() => []),
  ]);

  fillChipList(document.getElementById("node-friends"), friends, "No direct friends yet.");
  fillChipList(
    document.getElementById("node-recs"),
    recs.map(r => `${r.name} (${r.mutualFriendCount})`),
    "No recommendations available."
  );
  fillChipList(document.getElementById("node-network"), network, "Not connected to anyone.");

  // Highlight this node + its direct edges on the graph.
  const edgeKeys = new Set();
  for (const e of allEdges) {
    if (e.source === name || e.target === name) edgeKeys.add(`${e.source}->${e.target}`);
  }
  graphView.applyHighlight(new Set(friends).add(name), edgeKeys);

  activateTab("node");
}

function activateTab(tabId) {
  for (const btn of document.querySelectorAll(".tab-btn")) {
    btn.classList.toggle("active", btn.dataset.tab === tabId);
  }
  for (const panel of document.querySelectorAll(".tab-panel")) {
    panel.classList.toggle("active", panel.id === `tab-${tabId}`);
  }
}

document.getElementById("tabs").addEventListener("click", (e) => {
  const btn = e.target.closest(".tab-btn");
  if (btn) activateTab(btn.dataset.tab);
});

document.getElementById("btn-refresh").addEventListener("click", () => loadGraph().catch(e => setStatus(e.message, true)));
document.getElementById("btn-reset-layout").addEventListener("click", () => graphView.reLayout());

document.getElementById("btn-path").addEventListener("click", async () => {
  const from = document.getElementById("path-from").value;
  const to = document.getElementById("path-to").value;
  const resultEl = document.getElementById("path-result");
  if (!from || !to) { resultEl.innerHTML = '<span class="error-text">Choose both people.</span>'; return; }
  try {
    const result = await api(`/api/path?from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}`);
    if (!result.path) {
      resultEl.innerHTML = `<span class="error-text">No path from ${from} to ${to}.</span>`;
      graphView.clearHighlight();
      return;
    }
    const chainHtml = result.path.map(p => `<span class="node-pill">${p}</span>`).join(" → ");
    resultEl.innerHTML = `<div class="path-chain">${chainHtml}</div><p>${result.degrees} degree(s) of separation.</p>`;

    const nodeSet = new Set(result.path);
    const edgeKeys = new Set();
    for (let i = 0; i < result.path.length - 1; i++) {
      edgeKeys.add(`${result.path[i]}->${result.path[i + 1]}`);
    }
    graphView.applySelection(null);
    graphView.applyHighlight(nodeSet, edgeKeys);
  } catch (err) {
    resultEl.innerHTML = `<span class="error-text">${err.message}</span>`;
  }
});

document.getElementById("btn-mutual").addEventListener("click", async () => {
  const a = document.getElementById("mutual-a").value;
  const b = document.getElementById("mutual-b").value;
  const resultEl = document.getElementById("mutual-result");
  if (!a || !b) { resultEl.innerHTML = '<span class="error-text">Choose both people.</span>'; return; }
  try {
    const result = await api(`/api/mutual-friends?a=${encodeURIComponent(a)}&b=${encodeURIComponent(b)}`);
    if (result.mutualFriends.length === 0) {
      resultEl.innerHTML = `<p>${a} and ${b} have no mutual friends.</p>`;
    } else {
      resultEl.innerHTML = `<p>${result.mutualFriends.length} mutual friend(s): ${result.mutualFriends.join(", ")}</p>`;
    }
    graphView.applySelection(null);
    graphView.applyHighlight(new Set([a, b, ...result.mutualFriends]), new Set());
  } catch (err) {
    resultEl.innerHTML = `<span class="error-text">${err.message}</span>`;
  }
});

document.getElementById("btn-centrality").addEventListener("click", async () => {
  const listEl = document.getElementById("centrality-result");
  listEl.innerHTML = "<li>Loading…</li>";
  try {
    const result = await api("/api/centrality");
    listEl.innerHTML = "";
    for (const score of result) {
      const li = document.createElement("li");
      li.textContent = `${score.name} — total ${score.totalDegree} (out ${score.outDegree}, in ${score.inDegree})`;
      listEl.appendChild(li);
    }
  } catch (err) {
    listEl.innerHTML = `<li class="error-text">${err.message}</li>`;
  }
});

document.getElementById("btn-add-person").addEventListener("click", async () => {
  const input = document.getElementById("new-person-name");
  const resultEl = document.getElementById("manage-result");
  const name = input.value.trim();
  if (!name) return;
  try {
    await api("/api/people", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name }),
    });
    resultEl.innerHTML = `<span class="success-text">Added ${name}.</span>`;
    input.value = "";
    await loadGraph();
  } catch (err) {
    resultEl.innerHTML = `<span class="error-text">${err.message}</span>`;
  }
});

document.getElementById("btn-add-friendship").addEventListener("click", async () => {
  const from = document.getElementById("friend-from").value;
  const to = document.getElementById("friend-to").value;
  const mutual = document.getElementById("friend-mutual").checked;
  const resultEl = document.getElementById("manage-result");
  if (!from || !to) { resultEl.innerHTML = '<span class="error-text">Choose both people.</span>'; return; }
  try {
    await api("/api/friendships", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ from, to, mutual }),
    });
    resultEl.innerHTML = `<span class="success-text">Added friendship ${from} → ${to}${mutual ? " (mutual)" : ""}.</span>`;
    await loadGraph();
  } catch (err) {
    resultEl.innerHTML = `<span class="error-text">${err.message}</span>`;
  }
});

graphView = new GraphView(svg, (id) => showNodeDetails(id).catch(e => setStatus(e.message, true)));
loadGraph().catch(e => setStatus(e.message, true));
