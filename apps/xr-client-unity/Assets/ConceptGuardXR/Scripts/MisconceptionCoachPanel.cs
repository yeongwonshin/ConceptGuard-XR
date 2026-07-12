using UnityEngine;

namespace ConceptGuardXR
{
    public sealed class MisconceptionCoachPanel : MonoBehaviour
    {
        private TextMesh titleText;
        private TextMesh bodyText;
        private TextMesh riskText;
        private Renderer statusBar;
        private LabPalette palette;

        public void Configure(TextMesh title, TextMesh body, TextMesh risk, Renderer bar, LabPalette labPalette)
        {
            titleText = title;
            bodyText = body;
            riskText = risk;
            statusBar = bar;
            palette = labPalette;
        }

        public void Apply(AnalyzeResponseDto response)
        {
            if (response == null)
            {
                return;
            }

            var title = response.closed_circuit
                ? $"{FormatTopology(response.topology)} 회로 분석"
                : "열린 회로가 감지되었습니다";
            var body = string.IsNullOrWhiteSpace(response.feedback_text)
                ? response.xr_scene?.coach_message
                : response.feedback_text;
            var risk = response.teacher_summary != null
                ? $"위험도 {FormatRisk(response.teacher_summary.risk_level)} · 다음 단계: {response.teacher_summary.recommended_next_action}"
                : $"피드백 방식: {response.feedback_mode}";

            SetText(title, body, risk);
            SetSeverity(response.xr_scene?.severity);
        }

        public void ShowSystemState(string title, string body, string status)
        {
            SetText(title, body, status);
            SetSeverity("success");
        }

        public void ShowError(string message)
        {
            SetText(
                "ConceptGuard 연결 오류",
                message,
                "FastAPI 실행 상태와 StreamingAssets의 API URL을 확인하세요."
            );
            SetSeverity("danger");
        }

        private void SetText(string title, string body, string risk)
        {
            if (titleText != null)
            {
                titleText.text = WorldTextFactory.Wrap(title ?? string.Empty, 28);
            }

            if (bodyText != null)
            {
                bodyText.text = WorldTextFactory.Wrap(body ?? string.Empty, 42);
            }

            if (riskText != null)
            {
                riskText.text = WorldTextFactory.Wrap(risk ?? string.Empty, 52);
            }
        }

        private void SetSeverity(string severity)
        {
            if (statusBar == null || palette == null)
            {
                return;
            }

            var color = severity switch
            {
                "danger" => palette.Danger,
                "warning" => palette.Accent,
                _ => palette.Success
            };
            statusBar.material.color = color;
            if (statusBar.material.HasProperty("_EmissionColor"))
            {
                statusBar.material.EnableKeyword("_EMISSION");
                statusBar.material.SetColor("_EmissionColor", color * 1.6f);
            }
        }

        private static string FormatTopology(string topology)
        {
            return topology switch
            {
                "series" => "직렬",
                "parallel" => "병렬",
                "single_load" => "단일 부하",
                "mixed" => "혼합",
                _ => topology
            };
        }

        private static string FormatRisk(string risk)
        {
            return risk switch
            {
                "high" => "높음",
                "medium" => "중간",
                "low" => "낮음",
                _ => risk
            };
        }
    }
}
