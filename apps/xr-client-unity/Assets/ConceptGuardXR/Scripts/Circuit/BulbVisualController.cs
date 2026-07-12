using UnityEngine;

namespace ConceptGuardXR
{
    public sealed class BulbVisualController : MonoBehaviour
    {
        private XRComponentNode node;
        private Renderer bulbRenderer;
        private Light bulbLight;
        private Color offColor;
        private Color onColor;

        public void Configure(
            XRComponentNode componentNode,
            Renderer renderer,
            Light lightSource,
            Color inactiveColor,
            Color activeColor)
        {
            node = componentNode;
            bulbRenderer = renderer;
            bulbLight = lightSource;
            offColor = inactiveColor;
            onColor = activeColor;
            SetBrightness("off", 0f);
        }

        public void Apply(AnalyzeResponseDto response)
        {
            if (node == null || response?.electrical_values?.nodes == null)
            {
                return;
            }

            foreach (var value in response.electrical_values.nodes)
            {
                if (value.node_id == node.ComponentId)
                {
                    SetBrightness(value.brightness, value.power_w);
                    return;
                }
            }

            SetBrightness("off", 0f);
        }

        private void SetBrightness(string brightness, float power)
        {
            var intensity = brightness switch
            {
                "bright" => 3.2f,
                "medium" => 2.0f,
                "dim" => 0.9f,
                _ => 0f
            };
            intensity = Mathf.Max(intensity, Mathf.Clamp(power * 3f, 0f, 3.5f));

            if (bulbRenderer != null)
            {
                var material = bulbRenderer.material;
                var color = intensity > 0.01f ? onColor : offColor;
                material.color = color;
                if (material.HasProperty("_EmissionColor"))
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", onColor * intensity);
                }
            }

            if (bulbLight != null)
            {
                bulbLight.enabled = intensity > 0.01f;
                bulbLight.intensity = intensity;
            }
        }
    }
}
