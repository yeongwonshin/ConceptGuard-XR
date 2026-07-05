using System.Collections.Generic;
using UnityEngine;

namespace ConceptGuardXR
{
    public sealed class CurrentFlowVisualizer : MonoBehaviour
    {
        [SerializeField] private LineRenderer currentLinePrefab;
        [SerializeField] private Material successMaterial;
        [SerializeField] private Material warningMaterial;
        [SerializeField] private Material dangerMaterial;
        [SerializeField] private float verticalOffset = 0.05f;

        private readonly List<LineRenderer> activeLines = new List<LineRenderer>();
        private AnalyzeResponseDto latestResponse;

        private void Update()
        {
            if (latestResponse?.xr_scene?.current_flow == null) return;
            for (var i = 0; i < activeLines.Count; i++)
            {
                var line = activeLines[i];
                if (line == null || line.material == null) continue;
                var speed = i < latestResponse.xr_scene.current_flow.Count ? latestResponse.xr_scene.current_flow[i].pulse_speed : 1f;
                line.material.mainTextureOffset = new Vector2(Time.time * speed, 0f);
            }
        }

        public void Apply(AnalyzeResponseDto response)
        {
            latestResponse = response;
            Clear();
            if (response?.xr_scene?.current_flow == null || response.xr_scene.current_flow.Count == 0)
            {
                return;
            }

            foreach (var flow in response.xr_scene.current_flow)
            {
                var points = ResolveNodePositions(flow.node_ids);
                if (points.Count < 2) continue;

                var line = currentLinePrefab != null
                    ? Instantiate(currentLinePrefab, transform)
                    : new GameObject("CurrentFlowLine").AddComponent<LineRenderer>();
                line.positionCount = points.Count;
                line.widthMultiplier = Mathf.Clamp(0.01f + flow.current_a * 0.02f, 0.01f, 0.08f);
                line.material = SelectMaterial(response.xr_scene.severity);
                for (var i = 0; i < points.Count; i++)
                {
                    line.SetPosition(i, points[i] + Vector3.up * verticalOffset);
                }
                activeLines.Add(line);
            }
        }

        public void Clear()
        {
            foreach (var line in activeLines)
            {
                if (line != null) Destroy(line.gameObject);
            }
            activeLines.Clear();
        }

        private List<Vector3> ResolveNodePositions(List<string> nodeIds)
        {
            var points = new List<Vector3>();
            if (nodeIds == null) return points;
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
            switch (severity)
            {
                case "success": return successMaterial != null ? successMaterial : warningMaterial;
                case "danger": return dangerMaterial != null ? dangerMaterial : warningMaterial;
                default: return warningMaterial;
            }
        }
    }
}
