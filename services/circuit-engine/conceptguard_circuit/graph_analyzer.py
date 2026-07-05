from __future__ import annotations

from dataclasses import dataclass, field
from typing import Any, Dict, List, Optional, Tuple

import networkx as nx


LOAD_TYPES = {"bulb", "resistor"}
SOURCE_TYPES = {"battery"}
SWITCH_TYPES = {"switch"}


@dataclass
class ElectricalValue:
    node_id: str
    node_type: str
    voltage_v: float = 0.0
    current_a: float = 0.0
    power_w: float = 0.0
    brightness: str = "off"


@dataclass
class FlowPath:
    node_ids: List[str]
    current_a: float
    voltage_v: float
    label: str


@dataclass
class CircuitResult:
    closed_circuit: bool
    topology: str
    notes: List[str]
    equivalent_resistance_ohm: Optional[float] = None
    source_voltage_v: float = 0.0
    total_current_a: float = 0.0
    values: List[ElectricalValue] = field(default_factory=list)
    flow_paths: List[FlowPath] = field(default_factory=list)
    open_edges: List[Dict[str, Any]] = field(default_factory=list)
    missing_connections: List[str] = field(default_factory=list)


class CircuitAnalyzer:
    """Prototype-grade circuit analyzer for ConceptGuard XR.

    The analyzer is deliberately conservative: it handles the MVP circuit set
    used in the XR prototype (battery, wire, switch, bulb, resistor), detects
    open circuits, classifies common series/parallel layouts, and returns
    values that are good enough for real-time instructional visualization.

    It is not a SPICE replacement. The goal is to provide stable feedback and
    XR overlays for middle/high-school circuit concepts.
    """

    def build_graph(self, circuit_graph: Dict[str, Any]) -> nx.Graph:
        graph = nx.Graph()
        for node in circuit_graph.get("nodes", []):
            node_id = str(node.get("id"))
            if not node_id or node_id == "None":
                continue
            graph.add_node(
                node_id,
                type=str(node.get("type", "unknown")),
                resistance_ohm=float(node.get("resistance_ohm", self._default_resistance(node.get("type")))),
                voltage_v=float(node.get("voltage_v", 3.0 if node.get("type") == "battery" else 0.0)),
                label=str(node.get("label", node_id)),
                raw=node,
            )

        for index, edge in enumerate(circuit_graph.get("edges", [])):
            a = edge.get("from")
            b = edge.get("to")
            if a not in graph or b not in graph:
                continue
            is_open = bool(edge.get("open", False))
            graph.add_edge(
                a,
                b,
                id=edge.get("id", f"edge_{index}"),
                terminal_from=edge.get("terminal_from", ""),
                terminal_to=edge.get("terminal_to", ""),
                open=is_open,
                raw=edge,
            )
        return graph

    def analyze(self, circuit_graph: Dict[str, Any]) -> CircuitResult:
        graph = self.build_graph(circuit_graph)
        active_graph = self._without_open_switches(graph)

        batteries = self._nodes_by_type(active_graph, SOURCE_TYPES)
        loads = self._nodes_by_type(active_graph, LOAD_TYPES)
        switches = self._nodes_by_type(graph, SWITCH_TYPES)

        notes: List[str] = []
        if not batteries:
            notes.append("no_battery")
        if not loads:
            notes.append("no_load")
        if any(self._is_open_switch(graph, s) for s in switches):
            notes.append("open_switch")

        closed, closed_component = self._has_closed_power_loop(active_graph, batteries, loads)
        if not closed:
            missing = self._missing_connections(active_graph, batteries, loads)
            values = [
                ElectricalValue(
                    node_id=node,
                    node_type=active_graph.nodes[node].get("type", "unknown"),
                    brightness="off" if active_graph.nodes[node].get("type") in LOAD_TYPES else "inactive",
                )
                for node in active_graph.nodes
            ]
            return CircuitResult(
                closed_circuit=False,
                topology="open",
                notes=notes or ["open_loop"],
                source_voltage_v=self._source_voltage(active_graph, batteries),
                values=values,
                open_edges=[data.get("raw", {}) for _, _, data in graph.edges(data=True) if data.get("open")],
                missing_connections=missing,
            )

        subgraph = active_graph.subgraph(closed_component).copy() if closed_component else active_graph
        topology = self._infer_topology(subgraph, loads)
        source_voltage = self._source_voltage(subgraph, batteries)
        equivalent_r = self._equivalent_resistance(subgraph, loads, topology)
        total_current = source_voltage / equivalent_r if equivalent_r and equivalent_r > 0 else 0.0
        values = self._node_values(subgraph, loads, topology, source_voltage, total_current, equivalent_r)
        paths = self._flow_paths(subgraph, batteries, loads, topology, source_voltage, total_current)

        return CircuitResult(
            closed_circuit=True,
            topology=topology,
            notes=notes,
            equivalent_resistance_ohm=round(equivalent_r, 4) if equivalent_r else None,
            source_voltage_v=round(source_voltage, 4),
            total_current_a=round(total_current, 6),
            values=values,
            flow_paths=paths,
            open_edges=[data.get("raw", {}) for _, _, data in graph.edges(data=True) if data.get("open")],
        )

    def _default_resistance(self, node_type: Any) -> float:
        if node_type == "bulb":
            return 10.0
        if node_type == "resistor":
            return 20.0
        return 0.0

    def _nodes_by_type(self, graph: nx.Graph, types: set[str]) -> List[str]:
        return [node for node, data in graph.nodes(data=True) if data.get("type") in types]

    def _without_open_switches(self, graph: nx.Graph) -> nx.Graph:
        active = graph.copy()
        for u, v, data in list(active.edges(data=True)):
            if data.get("open"):
                active.remove_edge(u, v)
        for switch in self._nodes_by_type(active, SWITCH_TYPES):
            if self._is_open_switch(graph, switch):
                active.remove_edges_from(list(active.edges(switch)))
        return active

    def _is_open_switch(self, graph: nx.Graph, switch_id: str) -> bool:
        raw = graph.nodes[switch_id].get("raw", {})
        return str(raw.get("state", "closed")).lower() in {"open", "off", "false"}

    def _has_closed_power_loop(
        self, graph: nx.Graph, batteries: List[str], loads: List[str]
    ) -> Tuple[bool, Optional[set[str]]]:
        if not batteries or not loads:
            return False, None
        cycles = [set(cycle) for cycle in nx.cycle_basis(graph)]
        for component in nx.connected_components(graph):
            component_set = set(component)
            has_battery = any(b in component_set for b in batteries)
            has_load = any(load in component_set for load in loads)
            has_cycle = any(cycle <= component_set for cycle in cycles)
            if has_battery and has_load and has_cycle:
                return True, component_set
        return False, None

    def _missing_connections(self, graph: nx.Graph, batteries: List[str], loads: List[str]) -> List[str]:
        missing: List[str] = []
        if not batteries:
            missing.append("battery")
        if not loads:
            missing.append("load")
        if batteries and loads:
            for load in loads:
                if not any(nx.has_path(graph, battery, load) for battery in batteries):
                    missing.append(f"path_to_{load}")
        if graph.number_of_edges() < max(2, graph.number_of_nodes()):
            missing.append("return_path")
        return sorted(set(missing))

    def _source_voltage(self, graph: nx.Graph, batteries: List[str]) -> float:
        if not batteries:
            return 0.0
        return sum(float(graph.nodes[b].get("voltage_v", 3.0)) for b in batteries)

    def _infer_topology(self, graph: nx.Graph, loads: List[str]) -> str:
        if len(loads) == 0:
            return "no_load"
        if len(loads) == 1:
            return "single_load"

        neighbor_sets = [set(graph.neighbors(load)) for load in loads]
        if len(set(tuple(sorted(neighbors)) for neighbors in neighbor_sets)) == 1:
            return "parallel"

        load_degree_two = all(graph.degree(load) == 2 for load in loads)
        if load_degree_two and len(nx.cycle_basis(graph)) == 1:
            return "series"
        return "mixed"

    def _equivalent_resistance(self, graph: nx.Graph, loads: List[str], topology: str) -> Optional[float]:
        resistances = [float(graph.nodes[load].get("resistance_ohm", 10.0)) for load in loads]
        resistances = [r for r in resistances if r > 0]
        if not resistances:
            return None
        if topology == "parallel":
            reciprocal = sum(1.0 / r for r in resistances)
            return 1.0 / reciprocal if reciprocal else None
        return sum(resistances)

    def _node_values(
        self,
        graph: nx.Graph,
        loads: List[str],
        topology: str,
        source_voltage: float,
        total_current: float,
        equivalent_r: Optional[float],
    ) -> List[ElectricalValue]:
        values: List[ElectricalValue] = []
        for node, data in graph.nodes(data=True):
            node_type = data.get("type", "unknown")
            if node_type not in LOAD_TYPES:
                values.append(ElectricalValue(node_id=node, node_type=node_type, brightness="source" if node_type == "battery" else "active"))
                continue

            resistance = float(data.get("resistance_ohm", 10.0))
            if topology == "parallel":
                voltage = source_voltage
                current = voltage / resistance if resistance else 0.0
            else:
                current = total_current
                voltage = current * resistance
            power = voltage * current
            values.append(
                ElectricalValue(
                    node_id=node,
                    node_type=node_type,
                    voltage_v=round(voltage, 4),
                    current_a=round(current, 6),
                    power_w=round(power, 6),
                    brightness=self._brightness(power),
                )
            )
        return values

    def _brightness(self, power_w: float) -> str:
        if power_w <= 0.01:
            return "off"
        if power_w < 0.25:
            return "dim"
        if power_w < 0.8:
            return "medium"
        return "bright"

    def _flow_paths(
        self,
        graph: nx.Graph,
        batteries: List[str],
        loads: List[str],
        topology: str,
        source_voltage: float,
        total_current: float,
    ) -> List[FlowPath]:
        if not batteries or not loads:
            return []
        battery = batteries[0]
        paths: List[FlowPath] = []
        if topology == "parallel":
            for load in loads:
                try:
                    path = nx.shortest_path(graph, battery, load)
                except nx.NetworkXNoPath:
                    path = [battery, load]
                resistance = float(graph.nodes[load].get("resistance_ohm", 10.0))
                branch_current = source_voltage / resistance if resistance else 0.0
                paths.append(
                    FlowPath(path, round(branch_current, 6), round(source_voltage, 4), f"branch_to_{load}")
                )
        else:
            cycle = nx.cycle_basis(graph)[0] if nx.cycle_basis(graph) else [battery] + loads
            paths.append(FlowPath(cycle, round(total_current, 6), round(source_voltage, 4), "main_loop"))
        return paths
