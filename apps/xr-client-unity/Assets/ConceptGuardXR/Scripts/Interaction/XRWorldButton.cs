using System;
using UnityEngine;

namespace ConceptGuardXR
{
    public sealed class XRWorldButton : MonoBehaviour
    {
        private Action onPressed;
        private Renderer surface;
        private TextMesh label;
        private Material normalMaterial;
        private Material hoverMaterial;
        private Material disabledMaterial;
        private bool interactable = true;
        private Vector3 baseScale;

        public void Configure(
            TextMesh buttonLabel,
            Renderer buttonSurface,
            Material normal,
            Material hover,
            Material disabled,
            Action pressed)
        {
            label = buttonLabel;
            surface = buttonSurface;
            normalMaterial = normal;
            hoverMaterial = hover;
            disabledMaterial = disabled;
            onPressed = pressed;
            baseScale = transform.localScale;
            RefreshVisual(false);
        }

        public void Press()
        {
            if (!interactable)
            {
                return;
            }

            onPressed?.Invoke();
        }

        public void SetHovered(bool hovered)
        {
            RefreshVisual(hovered && interactable);
        }

        public void SetInteractable(bool value)
        {
            interactable = value;
            RefreshVisual(false);
        }

        private void RefreshVisual(bool hovered)
        {
            if (surface != null)
            {
                surface.sharedMaterial = !interactable
                    ? disabledMaterial
                    : hovered ? hoverMaterial : normalMaterial;
            }

            transform.localScale = baseScale * (hovered ? 1.035f : 1f);
            if (label != null)
            {
                label.color = interactable ? Color.white : new Color(0.65f, 0.68f, 0.72f);
            }
        }
    }
}
