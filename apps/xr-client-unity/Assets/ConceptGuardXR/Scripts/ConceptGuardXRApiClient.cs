using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ConceptGuardXR
{
    [Serializable]
    public sealed class HealthResponseDto
    {
        public string status;
        public string service;
        public string version;
    }

    [Serializable]
    public sealed class AnalyzeResponseDto
    {
        public string session_id;
        public string mission_id;
        public bool closed_circuit;
        public string topology;
        public List<string> misconceptions;
        public string feedback_mode;
        public string feedback_text;
        public ElectricalValuesDto electrical_values;
        public XRSceneDto xr_scene;
        public TeacherSummaryDto teacher_summary;
    }

    [Serializable]
    public sealed class ElectricalValuesDto
    {
        public float source_voltage_v;
        public float total_current_a;
        public float equivalent_resistance_ohm;
        public List<ElectricalNodeDto> nodes;
        public List<string> notes;
        public List<string> missing_connections;
    }

    [Serializable]
    public sealed class ElectricalNodeDto
    {
        public string node_id;
        public string node_type;
        public float voltage_v;
        public float current_a;
        public float power_w;
        public string brightness;
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
        public List<string> highlight;
        public string component_type;
    }

    public sealed class ConceptGuardXRApiClient : MonoBehaviour
    {
        private string apiBaseUrl;
        private int requestTimeoutSeconds;
        private CircuitGraphBuilder graphBuilder;
        private CurrentFlowVisualizer flowVisualizer;
        private MisconceptionCoachPanel coachPanel;
        private bool requestInFlight;

        public event Action<HealthResponseDto> HealthCheckSucceeded;
        public event Action<AnalyzeResponseDto> AnalysisReceived;
        public event Action<string> AnalysisFailed;

        public void Configure(
            string baseUrl,
            int timeoutSeconds,
            CircuitGraphBuilder builder,
            CurrentFlowVisualizer visualizer,
            MisconceptionCoachPanel panel)
        {
            apiBaseUrl = baseUrl?.TrimEnd('/');
            requestTimeoutSeconds = Mathf.Max(1, timeoutSeconds);
            graphBuilder = builder;
            flowVisualizer = visualizer;
            coachPanel = panel;
        }

        public void CheckHealth()
        {
            if (!ValidateConfiguration())
            {
                return;
            }

            StartCoroutine(GetHealth());
        }

        public void AnalyzeCurrentCircuit()
        {
            if (requestInFlight)
            {
                return;
            }

            if (!ValidateConfiguration())
            {
                return;
            }

            string json;
            try
            {
                graphBuilder.RecordEvent("analyze");
                json = graphBuilder.BuildAnalyzeJson();
            }
            catch (Exception exception)
            {
                Fail($"분석 요청을 만들 수 없습니다: {exception.Message}");
                return;
            }

            StartCoroutine(PostAnalyze(json));
        }

        private IEnumerator GetHealth()
        {
            using var request = UnityWebRequest.Get($"{apiBaseUrl}/health");
            request.timeout = requestTimeoutSeconds;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Fail(BuildRequestError("API 상태 확인 실패", request));
                yield break;
            }

            var response = JsonUtility.FromJson<HealthResponseDto>(request.downloadHandler.text);
            if (response == null || !string.Equals(response.status, "ok", StringComparison.OrdinalIgnoreCase))
            {
                Fail("API /health 응답이 올바르지 않습니다.");
                yield break;
            }

            HealthCheckSucceeded?.Invoke(response);
        }

        private IEnumerator PostAnalyze(string json)
        {
            requestInFlight = true;
            coachPanel?.ShowSystemState("회로 분석 중", "FastAPI 서버가 회로 구조와 학습자 상태를 분석하고 있습니다.", "요청 처리 중");

            using var request = new UnityWebRequest($"{apiBaseUrl}/analyze", UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = requestTimeoutSeconds;

            yield return request.SendWebRequest();
            requestInFlight = false;

            if (request.result != UnityWebRequest.Result.Success)
            {
                Fail(BuildRequestError("회로 분석 실패", request));
                yield break;
            }

            AnalyzeResponseDto response;
            try
            {
                response = JsonUtility.FromJson<AnalyzeResponseDto>(request.downloadHandler.text);
            }
            catch (Exception exception)
            {
                Fail($"분석 응답 JSON 파싱 실패: {exception.Message}");
                yield break;
            }

            if (response == null || response.xr_scene == null)
            {
                Fail("분석 응답에 xr_scene 데이터가 없습니다.");
                yield break;
            }

            coachPanel?.Apply(response);
            flowVisualizer?.Apply(response);
            AnalysisReceived?.Invoke(response);
        }

        private bool ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                Fail("API URL이 설정되지 않았습니다.");
                return false;
            }

            if (graphBuilder == null)
            {
                Fail("CircuitGraphBuilder가 연결되지 않았습니다.");
                return false;
            }

            return true;
        }

        private void Fail(string message)
        {
            requestInFlight = false;
            coachPanel?.ShowError(message);
            AnalysisFailed?.Invoke(message);
        }

        private static string BuildRequestError(string prefix, UnityWebRequest request)
        {
            var serverBody = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
            return string.IsNullOrWhiteSpace(serverBody)
                ? $"{prefix}: {request.error}"
                : $"{prefix}: {request.error}\n{serverBody}";
        }
    }
}
