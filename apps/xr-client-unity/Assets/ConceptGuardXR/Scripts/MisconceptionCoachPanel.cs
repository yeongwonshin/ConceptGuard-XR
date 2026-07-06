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
                titleText.text = response.closed_circuit
                    ? $"{response.topology} circuit analysis"
                    : "Open circuit detected";
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
                    ? $"Risk level: {response.teacher_summary.risk_level} / Next action: {response.teacher_summary.recommended_next_action}"
                    : $"Feedback mode: {response.feedback_mode}";
            }

            SetBadges(response.xr_scene?.severity);
        }

        public void ShowError(string message)
        {
            if (titleText != null) titleText.text = "ConceptGuard connection error";
            if (bodyText != null) bodyText.text = message;
            if (riskText != null) riskText.text = "Check the backend API URL and CORS settings.";
            SetBadges("danger");
        }

        private void SetBadges(string severity)
        {
            if (warningBadge != null) warningBadge.SetActive(severity == "warning");
            if (dangerBadge != null) dangerBadge.SetActive(severity == "danger");
        }
    }
}