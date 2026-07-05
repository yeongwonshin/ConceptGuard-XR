using System;
using System.Collections.Generic;
using UnityEngine;

namespace ConceptGuardXR
{
    public enum CircuitComponentType
    {
        Battery,
        Bulb,
        Resistor,
        Switch,
        Wire,
        Unknown
    }

    [Serializable]
    public sealed class CircuitNodePayload
    {
        public string id;
        public string type;
        public string label;
        public float resistance_ohm;
        public float voltage_v;
        public string state;
        public Vector3Payload position;
    }

    [Serializable]
    public sealed class Vector3Payload
    {
        public float x;
        public float y;
        public float z;

        public static Vector3Payload From(Vector3 value)
        {
            return new Vector3Payload { x = value.x, y = value.y, z = value.z };
        }
    }

    public sealed class XRComponentNode : MonoBehaviour
    {
        [Header("ConceptGuard Identity")]
        [SerializeField] private string componentId;
        [SerializeField] private CircuitComponentType componentType = CircuitComponentType.Unknown;
        [SerializeField] private string displayLabel;

        [Header("Electrical Properties")]
        [SerializeField] private float resistanceOhm = 10f;
        [SerializeField] private float voltageV = 3f;
        [SerializeField] private bool switchClosed = true;

        [Header("XR Terminals")]
        [SerializeField] private List<Transform> terminals = new List<Transform>();

        public string ComponentId => componentId;
        public CircuitComponentType ComponentType => componentType;
        public IReadOnlyList<Transform> Terminals => terminals;
        public bool SwitchClosed
        {
            get => switchClosed;
            set => switchClosed = value;
        }

        private void Reset()
        {
            EnsureId();
            displayLabel = gameObject.name;
        }

        private void OnValidate()
        {
            EnsureId();
            if (string.IsNullOrWhiteSpace(displayLabel))
            {
                displayLabel = gameObject.name;
            }
        }

        public CircuitNodePayload ToPayload()
        {
            return new CircuitNodePayload
            {
                id = componentId,
                type = ToApiType(componentType),
                label = string.IsNullOrWhiteSpace(displayLabel) ? componentId : displayLabel,
                resistance_ohm = componentType == CircuitComponentType.Bulb || componentType == CircuitComponentType.Resistor ? resistanceOhm : 0f,
                voltage_v = componentType == CircuitComponentType.Battery ? voltageV : 0f,
                state = componentType == CircuitComponentType.Switch ? (switchClosed ? "closed" : "open") : "active",
                position = Vector3Payload.From(transform.position)
            };
        }

        public Transform GetClosestTerminal(Vector3 worldPosition)
        {
            Transform best = null;
            var bestDistance = float.MaxValue;
            foreach (var terminal in terminals)
            {
                if (terminal == null) continue;
                var distance = Vector3.SqrMagnitude(terminal.position - worldPosition);
                if (distance < bestDistance)
                {
                    best = terminal;
                    bestDistance = distance;
                }
            }
            return best != null ? best : transform;
        }

        public static string ToApiType(CircuitComponentType type)
        {
            switch (type)
            {
                case CircuitComponentType.Battery: return "battery";
                case CircuitComponentType.Bulb: return "bulb";
                case CircuitComponentType.Resistor: return "resistor";
                case CircuitComponentType.Switch: return "switch";
                case CircuitComponentType.Wire: return "wire";
                default: return "unknown";
            }
        }

        private void EnsureId()
        {
            if (string.IsNullOrWhiteSpace(componentId))
            {
                componentId = $"{gameObject.name}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            }
        }
    }
}
