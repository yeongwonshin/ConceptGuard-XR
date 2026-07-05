using System;
using System.Collections.Generic;
using UnityEngine;

namespace ConceptGuardXR
{
    [Serializable]
    public sealed class CircuitGraphPayload
    {
        public List<CircuitNodePayload> nodes = new List<CircuitNodePayload>();
        public List<CircuitEdgePayload> edges = new List<CircuitEdgePayload>();
    }

    [Serializable]
    public sealed class AnalyzeRequestPayload
    {
        public string session_id;
        public string mission_id;
        public CircuitGraphPayload circuit_graph;
        public string learner_explanation;
        public PredictionPayload prediction = new PredictionPayload();
        public List<SessionEventPayload> manipulation_log = new List<SessionEventPayload>();
        public string locale = "ko-KR";
    }

    [Serializable]
    public sealed class PredictionPayload
    {
        public string brightness = "unknown";
        public string topology = "unknown";
    }

    [Serializable]
    public sealed class SessionEventPayload
    {
        public string session_id;
        public string mission_id;
        public string event_type;
        public int timestamp_ms;
        public string payload_json;
    }

    public sealed class CircuitGraphBuilder : MonoBehaviour
    {
        [SerializeField] private string sessionId = "xr-demo-session";
        [SerializeField] private string missionId = "M2_SERIES_PARALLEL";
        [SerializeField] private string learnerExplanation = "";
        [SerializeField] private string predictedBrightness = "unknown";
        [SerializeField] private string predictedTopology = "unknown";

        private readonly List<SessionEventPayload> sessionEvents = new List<SessionEventPayload>();

        public string SessionId
        {
            get => sessionId;
            set => sessionId = value;
        }

        public string MissionId
        {
            get => missionId;
            set => missionId = value;
        }

        public void SetLearnerExplanation(string explanation)
        {
            learnerExplanation = explanation ?? string.Empty;
        }

        public void SetPrediction(string brightness, string topology)
        {
            predictedBrightness = brightness ?? "unknown";
            predictedTopology = topology ?? "unknown";
        }

        public CircuitGraphPayload BuildGraph()
        {
            var graph = new CircuitGraphPayload();
            foreach (var node in FindObjectsByType<XRComponentNode>(FindObjectsSortMode.None))
            {
                graph.nodes.Add(node.ToPayload());
            }
            foreach (var wire in FindObjectsByType<XRWireConnection>(FindObjectsSortMode.None))
            {
                var edge = wire.ToPayload();
                if (!string.IsNullOrEmpty(edge.from) && !string.IsNullOrEmpty(edge.to))
                {
                    graph.edges.Add(edge);
                }
            }
            return graph;
        }

        public string BuildAnalyzeJson()
        {
            var request = new AnalyzeRequestPayload
            {
                session_id = sessionId,
                mission_id = missionId,
                circuit_graph = BuildGraph(),
                learner_explanation = learnerExplanation,
                prediction = new PredictionPayload { brightness = predictedBrightness, topology = predictedTopology },
                manipulation_log = sessionEvents,
                locale = "ko-KR"
            };
            return JsonUtility.ToJson(request);
        }

        public void RecordEvent(string eventType, string payloadJson = "{}")
        {
            sessionEvents.Add(new SessionEventPayload
            {
                session_id = sessionId,
                mission_id = missionId,
                event_type = eventType,
                timestamp_ms = Mathf.RoundToInt(Time.realtimeSinceStartup * 1000f),
                payload_json = payloadJson
            });
        }
    }
}
