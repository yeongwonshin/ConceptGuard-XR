using System.Collections.Generic;
using UnityEngine;

namespace ConceptGuardXR
{
    public sealed class CurrentFlowVisualizer : MonoBehaviour
    {
        private readonly List<LineRenderer> activeLines = new List<LineRenderer>();
        private readonly List<float> pulseSpeeds = new List<float>();
        private Material successMaterial;
        private Material warningMaterial;
        private Material dangerMaterial;
        private Texture2D pulseTexture;
        private float verticalOffset = 0.09f;

        public void Configure(LabPalette palette)
        {
            pulseTexture = CreatePulseTexture();
            successMaterial = CreateFlowMaterial("Current Success", palette.Success);
            warningMaterial = CreateFlowMaterial("Current Warning", palette.Accent);
            dangerMaterial = CreateFlowMaterial("Current Danger", palette.Danger);
        }

        private void Update()
        {
            for (var i = 0; i < activeLines.Count; i++)
            {
                var line = activeLines[i];
                if (line == null || line.material == null)
                {
                    continue;
                }

                var speed = i < pulseSpeeds.Count ? pulseSpeeds[i] : 1f;
                line.material.mainTextureOffset = new Vector2(-Time.time * speed, 0f);
            }
        }

        public void Apply(AnalyzeResponseDto response)
        {
            Clear();
            if (response?.xr_scene?.current_flow == null || response.xr_scene.current_flow.Count == 0)
            {
                return;
            }

            foreach (var flow in response.xr_scene.current_flow)
            {
                var points = ResolveNodePositions(flow.node_ids);
                if (points.Count < 2)
                {
                    continue;
                }

                var lineObject = new GameObject($"Current Flow - {flow.label}");
                lineObject.transform.SetParent(transform, false);
                var line = lineObject.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.positionCount = points.Count;
                line.loop = response.closed_circuit && points.Count > 2;
                line.textureMode = LineTextureMode.Tile;
                line.numCapVertices = 8;
                line.numCornerVertices = 6;
                line.widthMultiplier = Mathf.Clamp(0.018f + flow.current_a * 0.025f, 0.018f, 0.07f);
                line.material = new Material(SelectMaterial(response.xr_scene.severity));

                for (var i = 0; i < points.Count; i++)
                {
                    line.SetPosition(i, points[i] + Vector3.up * verticalOffset);
                }

                activeLines.Add(line);
                pulseSpeeds.Add(Mathf.Max(0.2f, flow.pulse_speed));
            }
        }

        public void Clear()
        {
            foreach (var line in activeLines)
            {
                if (line != null)
                {
                    Destroy(line.gameObject);
                }
            }

            activeLines.Clear();
            pulseSpeeds.Clear();
        }

        private List<Vector3> ResolveNodePositions(List<string> nodeIds)
        {
            var points = new List<Vector3>();
            if (nodeIds == null)
            {
                return points;
            }

            var nodes = FindObjectsByType<XRComponentNode>(FindObjectsSortMode.None);
            foreach (var id in nodeIds)
            {
                foreach (var node in nodes)
                {
                    if (node.ComponentId == id)
                    {
                        points.Add(node.transform.position);
                        break;
                    }
                }
            }

            return points;
        }

        private Material SelectMaterial(string severity)
        {
            return severity switch
            {
                "success" => successMaterial,
                "danger" => dangerMaterial,
                _ => warningMaterial
            };
        }

        private Material CreateFlowMaterial(string materialName, Color color)
        {
            var material = new Material(Shader.Find("Sprites/Default"))
            {
                name = materialName,
                color = color,
                mainTexture = pulseTexture
            };
            return material;
        }

        private static Texture2D CreatePulseTexture()
        {
            var texture = new Texture2D(32, 1, TextureFormat.RGBA32, false)
            {
                name = "ConceptGuard Current Pulse",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };

            for (var x = 0; x < texture.width; x++)
            {
                var phase = x / (float)(texture.width - 1);
                var alpha = Mathf.Pow(Mathf.Sin(phase * Mathf.PI), 2f);
                texture.SetPixel(x, 0, new Color(1f, 1f, 1f, alpha));
            }

            texture.Apply();
            return texture;
        }
    }
}
