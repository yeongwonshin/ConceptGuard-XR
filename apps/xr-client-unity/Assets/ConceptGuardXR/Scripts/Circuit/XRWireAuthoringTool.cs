using System.Collections.Generic;
using UnityEngine;

namespace ConceptGuardXR
{
    public sealed class XRWireAuthoringTool : MonoBehaviour
    {
        private readonly List<XRWireConnection> wires = new List<XRWireConnection>();
        private CircuitTerminal pendingTerminal;
        private CircuitGraphBuilder graphBuilder;
        private SessionEventLogger eventLogger;
        private Material wireMaterial;

        public void Configure(CircuitGraphBuilder builder, SessionEventLogger logger, LabPalette palette)
        {
            graphBuilder = builder;
            eventLogger = logger;
            wireMaterial = new Material(palette.Get("Wire", palette.Cyan, 0.1f, 0.75f, true));
        }

        public void SelectTerminal(CircuitTerminal terminal)
        {
            if (terminal == null || terminal.Owner == null)
            {
                return;
            }

            if (pendingTerminal == null)
            {
                pendingTerminal = terminal;
                pendingTerminal.SetSelected(true);
                return;
            }

            if (pendingTerminal == terminal)
            {
                CancelPendingConnection();
                return;
            }

            if (pendingTerminal.Owner == terminal.Owner)
            {
                eventLogger?.Log("failed_connect", "{\"reason\":\"same_component\"}");
                pendingTerminal.SetSelected(false);
                pendingTerminal = terminal;
                pendingTerminal.SetSelected(true);
                return;
            }

            foreach (var existingWire in wires)
            {
                if (existingWire != null && existingWire.Connects(pendingTerminal, terminal))
                {
                    eventLogger?.Log("failed_connect", "{\"reason\":\"duplicate_connection\"}");
                    CancelPendingConnection();
                    return;
                }
            }

            CreateWire(pendingTerminal, terminal);
            pendingTerminal.SetSelected(false);
            terminal.SetSelected(false);
            pendingTerminal = null;
        }

        public void CancelPendingConnection()
        {
            if (pendingTerminal != null)
            {
                pendingTerminal.SetSelected(false);
                pendingTerminal = null;
            }
        }

        public void UndoLastWire()
        {
            CancelPendingConnection();
            for (var index = wires.Count - 1; index >= 0; index--)
            {
                var wire = wires[index];
                wires.RemoveAt(index);
                if (wire == null)
                {
                    continue;
                }

                eventLogger?.Log("disconnect", $"{{\"wire_id\":\"{wire.name}\"}}");
                Destroy(wire.gameObject);
                return;
            }
        }

        public void ClearAllWires()
        {
            CancelPendingConnection();
            foreach (var wire in wires)
            {
                if (wire != null)
                {
                    Destroy(wire.gameObject);
                }
            }

            wires.Clear();
            eventLogger?.Log("clear", "{\"target\":\"wires\"}");
        }

        private void CreateWire(CircuitTerminal from, CircuitTerminal to)
        {
            var wireObject = new GameObject($"wire_{from.Owner.ComponentId}_{to.Owner.ComponentId}_{wires.Count + 1:00}");
            wireObject.transform.SetParent(transform, false);
            var line = wireObject.AddComponent<LineRenderer>();
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            var wire = wireObject.AddComponent<XRWireConnection>();
            wire.Bind(from.Owner, to.Owner, from.transform, to.transform);
            wire.ConfigureVisual(new Material(wireMaterial), 0.026f);
            wires.Add(wire);

            var payload = $"{{\"from\":\"{from.Owner.ComponentId}\",\"to\":\"{to.Owner.ComponentId}\"}}";
            if (eventLogger != null)
            {
                eventLogger.Log("connect", payload);
            }
            else
            {
                graphBuilder?.RecordEvent("connect", payload);
            }
        }
    }
}
