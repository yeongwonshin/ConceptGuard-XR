using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ConceptGuardXR
{
    [Serializable]
    internal sealed class SessionEventRequestDto
    {
        public string session_id;
        public string mission_id;
        public string event_type;
        public int timestamp_ms;
        public SessionEventBodyDto payload = new SessionEventBodyDto();
    }

    [Serializable]
    internal sealed class SessionEventBodyDto
    {
        public string raw_json;
    }

    public sealed class SessionEventLogger : MonoBehaviour
    {
        private string apiBaseUrl;
        private int requestTimeoutSeconds;
        private CircuitGraphBuilder graphBuilder;

        public void Configure(string baseUrl, int timeoutSeconds, CircuitGraphBuilder builder)
        {
            apiBaseUrl = baseUrl?.TrimEnd('/');
            requestTimeoutSeconds = Mathf.Max(1, timeoutSeconds);
            graphBuilder = builder;
        }

        public void Log(string eventType, string payloadJson = "{}")
        {
            if (graphBuilder == null || string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                Debug.LogError("SessionEventLogger is not configured.");
                return;
            }

            graphBuilder.RecordEvent(eventType, payloadJson);
            StartCoroutine(PostEvent(eventType, payloadJson));
        }

        private IEnumerator PostEvent(string eventType, string payloadJson)
        {
            var dto = new SessionEventRequestDto
            {
                session_id = graphBuilder.SessionId,
                mission_id = graphBuilder.MissionId,
                event_type = eventType,
                timestamp_ms = Mathf.RoundToInt(Time.realtimeSinceStartup * 1000f),
                payload = new SessionEventBodyDto { raw_json = payloadJson ?? "{}" }
            };

            using var request = new UnityWebRequest($"{apiBaseUrl}/sessions/events", UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(dto)));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = requestTimeoutSeconds;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Session event upload failed: {request.error} {request.downloadHandler.text}");
            }
        }
    }
}
