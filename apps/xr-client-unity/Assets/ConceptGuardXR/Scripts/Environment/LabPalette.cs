using System.Collections.Generic;
using UnityEngine;

namespace ConceptGuardXR
{
    public sealed class LabPalette
    {
        private readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();

        public Color Background { get; } = new Color(0.035f, 0.055f, 0.09f);
        public Color Navy { get; } = new Color(0.07f, 0.11f, 0.18f);
        public Color Panel { get; } = new Color(0.92f, 0.95f, 0.98f);
        public Color PanelMuted { get; } = new Color(0.70f, 0.77f, 0.84f);
        public Color Cyan { get; } = new Color(0.05f, 0.82f, 0.93f);
        public Color Accent { get; } = new Color(1f, 0.68f, 0.18f);
        public Color Success { get; } = new Color(0.18f, 0.86f, 0.58f);
        public Color Danger { get; } = new Color(1f, 0.28f, 0.36f);
        public Color Purple { get; } = new Color(0.55f, 0.36f, 0.95f);
        public Color Copper { get; } = new Color(0.82f, 0.40f, 0.16f);
        public Color DarkText { get; } = new Color(0.06f, 0.09f, 0.14f);
        public Color LightText { get; } = new Color(0.95f, 0.98f, 1f);

        public Material Get(string key, Color color, float metallic = 0f, float smoothness = 0.45f, bool emission = false)
        {
            if (materials.TryGetValue(key, out var existing))
            {
                return existing;
            }

            var shader = Shader.Find("Standard");
            var material = new Material(shader)
            {
                name = $"ConceptGuard {key}",
                color = color
            };
            material.SetFloat("_Metallic", Mathf.Clamp01(metallic));
            material.SetFloat("_Glossiness", Mathf.Clamp01(smoothness));
            if (emission)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 1.8f);
            }

            materials[key] = material;
            return material;
        }

        public Material CreateInstance(string key)
        {
            return new Material(materials[key]);
        }
    }
}
