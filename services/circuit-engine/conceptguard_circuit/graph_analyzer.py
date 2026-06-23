from dataclasses import dataclass
from typing import Dict, List, Any
import networkx as nx

@dataclass
class CircuitResult:
    closed_circuit: bool
    topology: str
    notes: List[str]

class CircuitAnalyzer:
    """MVP circuit analyzer.

    This is intentionally simple for demo stability. Replace with a richer
    nodal-analysis model as the physics scope grows.
    """

    def build_graph(self, circuit_graph: Dict[str, Any]) -> nx.Graph:
        graph = nx.Graph()
        for node in circuit_graph.get("nodes", []):
            graph.add_node(node["id"], type=node.get("type", "unknown"))
        for edge in circuit_graph.get("edges", []):
            graph.add_edge(edge["from"], edge["to"], **edge)
        return graph

    def analyze(self, circuit_graph: Dict[str, Any]) -> CircuitResult:
        graph = self.build_graph(circuit_graph)
        batteries = [n for n, d in graph.nodes(data=True) if d.get("type") == "battery"]
        bulbs = [n for n, d in graph.nodes(data=True) if d.get("type") == "bulb"]

        notes = []
        if not batteries:
            notes.append("no_battery")
        if not bulbs:
            notes.append("no_bulb")

        closed = False
        if batteries and bulbs:
            # Demo proxy: a connected component with battery and bulb and at least as many edges as nodes can contain a cycle.
            for component in nx.connected_components(graph):
                sub = graph.subgraph(component)
                if any(n in batteries for n in sub.nodes) and any(n in bulbs for n in sub.nodes):
                    closed = sub.number_of_edges() >= sub.number_of_nodes()
                    break

        topology = self._infer_topology(graph, bulbs)
        return CircuitResult(closed_circuit=closed, topology=topology, notes=notes)

    def _infer_topology(self, graph: nx.Graph, bulbs: List[str]) -> str:
        if len(bulbs) < 2:
            return "single_load"
        # MVP heuristic: if bulbs share both neighbor sets, treat as parallel-like.
        neighbor_sets = [set(graph.neighbors(b)) for b in bulbs]
        if len(set(map(tuple, map(sorted, neighbor_sets)))) == 1:
            return "parallel_candidate"
        return "series_or_mixed_candidate"
