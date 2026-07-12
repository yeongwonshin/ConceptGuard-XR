using UnityEngine;

namespace ConceptGuardXR
{
    public sealed class CircuitTerminal : MonoBehaviour
    {
        private XRComponentNode owner;
        private Renderer terminalRenderer;
        private Material normalMaterial;
        private Material selectedMaterial;

        public XRComponentNode Owner => owner;

        public void Configure(
            XRComponentNode ownerNode,
            Renderer renderer,
            Material normal,
            Material selected)
        {
            owner = ownerNode;
            terminalRenderer = renderer;
            normalMaterial = normal;
            selectedMaterial = selected;
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (terminalRenderer != null)
            {
                terminalRenderer.sharedMaterial = selected ? selectedMaterial : normalMaterial;
            }
        }
    }
}
