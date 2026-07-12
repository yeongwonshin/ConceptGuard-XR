using System;
using System.Collections;
using UnityEngine;

namespace ConceptGuardXR
{
    public sealed class ConceptGuardApplication : MonoBehaviour
    {
        private LabRuntimeBindings bindings;
        private ConceptGuardRuntimeConfig config;
        private bool apiHealthy;

        private IEnumerator Start()
        {
            Application.runInBackground = true;
            DontDestroyOnLoad(gameObject);

            bindings = EducationalLabBuilder.Build(transform);
            bindings.CoachPanel.ShowSystemState(
                "ConceptGuard XR",
                "환경 설정을 불러오고 API 연결을 확인하고 있습니다.",
                "연결 중"
            );

            string configurationError = null;
            yield return ConceptGuardConfigLoader.Load(
                loaded => config = loaded,
                error => configurationError = error
            );

            if (!string.IsNullOrWhiteSpace(configurationError))
            {
                FailStartup(configurationError);
                yield break;
            }

            var sessionId = $"{config.session_id_prefix}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";
            sessionId = sessionId.Substring(0, Math.Min(sessionId.Length, 64));

            bindings.GraphBuilder.Configure(sessionId, config.mission_id, config.locale);
            bindings.EventLogger.Configure(config.api_base_url, config.request_timeout_seconds, bindings.GraphBuilder);
            bindings.WireTool.Configure(bindings.GraphBuilder, bindings.EventLogger, bindings.Palette);
            bindings.ApiClient.Configure(
                config.api_base_url,
                config.request_timeout_seconds,
                bindings.GraphBuilder,
                bindings.FlowVisualizer,
                bindings.CoachPanel
            );

            bindings.AnalyzeButton.SetInteractable(false);
            bindings.ApiClient.AnalysisReceived += HandleAnalysis;
            bindings.ApiClient.HealthCheckSucceeded += HandleHealthSuccess;
            bindings.ApiClient.AnalysisFailed += HandleAnalysisFailure;
            bindings.ApiClient.CheckHealth();
        }

        private void HandleHealthSuccess(HealthResponseDto health)
        {
            apiHealthy = true;
            bindings.AnalyzeButton.SetInteractable(true);
            bindings.CoachPanel.ShowSystemState(
                "교육 세션 준비 완료",
                "부품은 Grip으로 옮기고, 오른손 Trigger로 단자를 두 번 선택해 연결하세요. 회로를 만든 뒤 분석 버튼을 누르세요.",
                $"API 연결됨 · {health.version}"
            );
        }

        private void HandleAnalysis(AnalyzeResponseDto response)
        {
            bindings.OverlayPresenter.Apply(response);
            foreach (var bulb in FindObjectsByType<BulbVisualController>(FindObjectsSortMode.None))
            {
                bulb.Apply(response);
            }
        }

        private void HandleAnalysisFailure(string message)
        {
            bindings.AnalyzeButton.SetInteractable(apiHealthy);
            Debug.LogError(message);
        }

        private void FailStartup(string message)
        {
            bindings.AnalyzeButton.SetInteractable(false);
            bindings.CoachPanel.ShowError(message);
            Debug.LogError(message);
        }

        private void OnDestroy()
        {
            if (bindings?.ApiClient == null)
            {
                return;
            }

            bindings.ApiClient.AnalysisReceived -= HandleAnalysis;
            bindings.ApiClient.HealthCheckSucceeded -= HandleHealthSuccess;
            bindings.ApiClient.AnalysisFailed -= HandleAnalysisFailure;
        }
    }
}
