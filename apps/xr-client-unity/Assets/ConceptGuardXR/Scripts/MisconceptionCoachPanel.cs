using TMPro;
using UnityEngine;

namespace ConceptGuardXR
{
    public sealed class MisconceptionCoachPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private TMP_Text riskText;
        [SerializeField] private GameObject warningBadge;
        [SerializeField] private GameObject dangerBadge;

        public void Apply(AnalyzeResponseDto response)
        {
            if (response == null) return;
            if (titleText != null)
            {
                titleText.text = response.closed_circuit ? $"{response.topology} 회로 분석" : "열린 회로 감지";
            }
            if (bodyText != null)
            {
                bodyText.text = string.IsNullOrWhiteSpace(response.feedback_text)
                    ? response.xr_scene?.coach_message
                    : response.feedback_text;
            }
            if (riskText != null)
            {
                riskText.text = response.teacher_summary != null
                    ? $"위험도: {response.teacher_summary.risk_level} / 다음 행동: {response.teacher_summary.recommended_next_action}"
                    : $"피드백 모드: {response.feedback_mode}";
            }
            SetBadges(response.xr_scene?.severity);
        }

        public void ShowError(string message)
        {
            if (titleText != null) titleText.text = "ConceptGuard 연결 오류";
            if (bodyText != null) bodyText.text = message;
            if (riskText != null) riskText.text = "백엔드 API 주소와 CORS 설정을 확인하세요.";
            SetBadges("danger");
        }

        private void SetBadges(string severity)
        {
            if (warningBadge != null) warningBadge.SetActive(severity == "warning");
            if (dangerBadge != null) dangerBadge.SetActive(severity == "danger");
        }
    }
}
