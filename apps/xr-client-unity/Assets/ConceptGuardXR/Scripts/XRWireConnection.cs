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

        private LineRenderer lineRenderer;

        public XRComponentNode FromNode => fromNode;
        public XRComponentNode ToNode => toNode;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
        }

        private void LateUpdate()
        {
            if (fromNode == null || toNode == null) return;
            var start = fromTerminal != null ? fromTerminal.position : fromNode.transform.position;
            var end = toTerminal != null ? toTerminal.position : toNode.transform.position;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }

        public void Bind(XRComponentNode from, XRComponentNode to, Transform fromPin = null, Transform toPin = null)
        {
            fromNode = from;
            toNode = to;
            fromTerminal = fromPin;
            toTerminal = toPin;
            open = false;
            LateUpdate();
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
