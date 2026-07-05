using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ConceptGuardXR
{
    public sealed class SessionEventLogger : MonoBehaviour
    {
        [SerializeField] private string apiBaseUrl = "http://localhost:8000";
        [SerializeField] private CircuitGraphBuilder graphBuilder;

        public void Log(string eventType, string payloadJson = "{}")
        {
            graphBuilder?.RecordEvent(eventType, payloadJson);
            StartCoroutine(PostEvent(eventType, payloadJson));
        }

        private IEnumerator PostEvent(string eventType, string payloadJson)
        {
            if (graphBuilder == null) yield break;
            var json = $"{{\"session_id\":\"{graphBuilder.SessionId}\",\"mission_id\":\"{graphBuilder.MissionId}\",\"event_type\":\"{eventType}\",\"timestamp_ms\":{Mathf.RoundToInt(Time.realtimeSinceStartup * 1000f)},\"payload\":{{\"raw\":\"{Escape(payloadJson)}\"}}}}";
            using var request = new UnityWebRequest($"{apiBaseUrl.TrimEnd('/')}/sessions/events", "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
