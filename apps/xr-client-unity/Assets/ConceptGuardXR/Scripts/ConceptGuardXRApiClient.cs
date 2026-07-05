using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ConceptGuardXR
{
    [Serializable]
    public sealed class AnalyzeResponseDto
    {
        public string session_id;
        public string mission_id;
        public bool closed_circuit;
        public string topology;
        public string feedback_mode;
        public string feedback_text;
        public XRSceneDto xr_scene;
        public TeacherSummaryDto teacher_summary;
    }

    [Serializable]
    public sealed class TeacherSummaryDto
    {
        public string risk_level;
        public string headline;
        public int misconception_count;
        public string recommended_next_action;
    }

    [Serializable]
    public sealed class XRSceneDto
    {
        public string scene_state;
        public string severity;
        public string feedback_mode;
        public string coach_message;
        public List<CurrentFlowDto> current_flow;
        public List<OverlayDto> overlays;
        public List<GhostActionDto> ghost_actions;
    }

    [Serializable]
    public sealed class CurrentFlowDto
    {
        public List<string> node_ids;
        public float current_a;
        public float voltage_v;
        public float pulse_speed;
        public string label;
    }

    [Serializable]
    public sealed class OverlayDto
    {
        public string target_id;
        public string label;
        public string severity;
    }

    [Serializable]
    public sealed class GhostActionDto
    {
        public string type;
        public string label;
    }

    public sealed class ConceptGuardXRApiClient : MonoBehaviour
    {
        [Header("Backend")]
        [SerializeField] private string apiBaseUrl = "http://localhost:8000";
        [SerializeField] private float requestTimeoutSeconds = 5f;

        [Header("Scene Bindings")]
        [SerializeField] private CircuitGraphBuilder graphBuilder;
        [SerializeField] private CurrentFlowVisualizer flowVisualizer;
        [SerializeField] private MisconceptionCoachPanel coachPanel;

        public event Action<AnalyzeResponseDto> AnalysisReceived;
        public event Action<string> AnalysisFailed;

        public void AnalyzeCurrentCircuit()
        {
            if (graphBuilder == null)
            {
                AnalysisFailed?.Invoke("CircuitGraphBuilder is not assigned.");
                return;
            }
            StartCoroutine(PostAnalyze(graphBuilder.BuildAnalyzeJson()));
        }

        private IEnumerator PostAnalyze(string json)
        {
            using var request = new UnityWebRequest($"{apiBaseUrl.TrimEnd('/')}/analyze", "POST");
            var body = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = Mathf.CeilToInt(requestTimeoutSeconds);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                var message = $"ConceptGuard API error: {request.error} / {request.downloadHandler.text}";
                coachPanel?.ShowError(message);
                AnalysisFailed?.Invoke(message);
                yield break;
            }

            var raw = request.downloadHandler.text;
            var response = JsonUtility.FromJson<AnalyzeResponseDto>(raw);
            coachPanel?.Apply(response);
            flowVisualizer?.Apply(response);
            AnalysisReceived?.Invoke(response);
        }
    }
}
