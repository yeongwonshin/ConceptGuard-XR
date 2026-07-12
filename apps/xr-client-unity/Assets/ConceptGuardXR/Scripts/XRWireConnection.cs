using System;
using UnityEngine;

namespace ConceptGuardXR
{
    [Serializable]
    public sealed class CircuitEdgePayload
    {
        public string id;
        public string from;
        public string to;
        public string terminal_from;
        public string terminal_to;
        public bool open;
    }

    [RequireComponent(typeof(LineRenderer))]
    public sealed class XRWireConnection : MonoBehaviour
    {
        [SerializeField] private XRComponentNode fromNode;
        [SerializeField] private XRComponentNode toNode;
        [SerializeField] private Transform fromTerminal;
        [SerializeField] private Transform toTerminal;
        [SerializeField] private bool open;
        [SerializeField] private float wireLift = 0.08f;

        private LineRenderer lineRenderer;

        public XRComponentNode FromNode => fromNode;
        public XRComponentNode ToNode => toNode;
        public Transform FromTerminal => fromTerminal;
        public Transform ToTerminal => toTerminal;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 4;
            lineRenderer.useWorldSpace = true;
            lineRenderer.numCapVertices = 6;
            lineRenderer.numCornerVertices = 5;
        }

        private void LateUpdate()
        {
            if (fromNode == null || toNode == null || lineRenderer == null)
            {
                return;
            }

            var start = fromTerminal != null ? fromTerminal.position : fromNode.transform.position;
            var end = toTerminal != null ? toTerminal.position : toNode.transform.position;
            var midpoint = Vector3.Lerp(start, end, 0.5f) + Vector3.up * (wireLift + Vector3.Distance(start, end) * 0.08f);
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, Vector3.Lerp(start, midpoint, 0.55f));
            lineRenderer.SetPosition(2, Vector3.Lerp(midpoint, end, 0.45f));
            lineRenderer.SetPosition(3, end);
        }

        public void Bind(XRComponentNode from, XRComponentNode to, Transform fromPin, Transform toPin)
        {
            fromNode = from;
            toNode = to;
            fromTerminal = fromPin;
            toTerminal = toPin;
            open = false;
            LateUpdate();
        }

        public void ConfigureVisual(Material material, float width)
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }

            lineRenderer.material = material;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
        }

        public bool Connects(CircuitTerminal first, CircuitTerminal second)
        {
            if (first == null || second == null)
            {
                return false;
            }

            return (fromTerminal == first.transform && toTerminal == second.transform) ||
                   (fromTerminal == second.transform && toTerminal == first.transform);
        }

        public CircuitEdgePayload ToPayload()
        {
            return new CircuitEdgePayload
            {
                id = name,
                from = fromNode != null ? fromNode.ComponentId : string.Empty,
                to = toNode != null ? toNode.ComponentId : string.Empty,
                terminal_from = fromTerminal != null ? fromTerminal.name : string.Empty,
                terminal_to = toTerminal != null ? toTerminal.name : string.Empty,
                open = open || fromNode == null || toNode == null
            };
        }
    }
}
