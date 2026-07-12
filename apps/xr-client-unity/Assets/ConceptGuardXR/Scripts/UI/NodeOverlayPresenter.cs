using System.Collections.Generic;
using UnityEngine;

namespace ConceptGuardXR
{
    public sealed class NodeOverlayPresenter : MonoBehaviour
    {
        private readonly List<GameObject> overlays = new List<GameObject>();
        private LabPalette palette;

        public void Configure(LabPalette labPalette)
        {
            palette = labPalette;
        }

        public void Apply(AnalyzeResponseDto response)
        {
            Clear();
            if (response?.xr_scene?.overlays == null)
            {
                return;
            }

            var nodes = FindObjectsByType<XRComponentNode>(FindObjectsSortMode.None);
            foreach (var overlay in response.xr_scene.overlays)
            {
                XRComponentNode target = null;
                foreach (var node in nodes)
                {
                    if (node.ComponentId == overlay.target_id)
                    {
                        target = node;
                        break;
                    }
                }

                if (target == null)
                {
                    continue;
                }

                overlays.Add(CreateOverlay(target.transform, overlay));
            }
        }

        public void Clear()
        {
            foreach (var overlay in overlays)
            {
                if (overlay != null)
                {
                    Destroy(overlay);
                }
            }
            overlays.Clear();
        }

        private GameObject CreateOverlay(Transform target, OverlayDto overlay)
        {
            var root = new GameObject($"Overlay - {overlay.target_id}");
            root.transform.SetParent(target, false);
            root.transform.localPosition = new Vector3(0f, 0.36f, 0f);
            root.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "Overlay Panel";
            panel.transform.SetParent(root.transform, false);
            panel.transform.localScale = new Vector3(0.68f, 0.18f, 0.018f);
            panel.GetComponent<Collider>().enabled = false;
            var color = overlay.severity switch
            {
                "danger" => palette.Danger,
                "warning" => palette.Accent,
                _ => palette.Success
            };
            panel.GetComponent<Renderer>().sharedMaterial = palette.Get($"Overlay {overlay.severity}", color, 0f, 0.6f, true);

            var text = WorldTextFactory.Create(
                root.transform,
                "Overlay Text",
                WorldTextFactory.Wrap(overlay.label, 34),
                new Vector3(0f, 0f, -0.014f),
                0.028f,
                44,
                Color.white,
                TextAnchor.MiddleCenter,
                TextAlignment.Center
            );
            text.transform.localRotation = Quaternion.identity;
            return root;
        }
    }
}
