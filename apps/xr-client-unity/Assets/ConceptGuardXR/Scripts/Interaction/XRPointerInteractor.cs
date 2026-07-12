using UnityEngine;
using UnityEngine.XR;

namespace ConceptGuardXR
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class XRPointerInteractor : MonoBehaviour
    {
        private XRNode node = XRNode.RightHand;
        private XRWireAuthoringTool wireTool;
        private float maxDistance = 6f;
        private bool previousTrigger;
        private bool previousCancel;
        private LineRenderer line;
        private XRWorldButton hoveredButton;

        public void Configure(XRNode xrNode, XRWireAuthoringTool tool, Material lineMaterial, float distance)
        {
            node = xrNode;
            wireTool = tool;
            maxDistance = Mathf.Max(1f, distance);
            line = GetComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startWidth = 0.008f;
            line.endWidth = 0.002f;
            line.material = lineMaterial;
            line.numCapVertices = 4;
        }

        private void Update()
        {
            if (line == null)
            {
                return;
            }

            var origin = transform.position;
            var direction = transform.forward;
            var endpoint = origin + direction * maxDistance;
            RaycastHit hit;
            var hasHit = Physics.Raycast(origin, direction, out hit, maxDistance, ~0, QueryTriggerInteraction.Collide);
            if (hasHit)
            {
                endpoint = hit.point;
            }

            line.SetPosition(0, origin);
            line.SetPosition(1, endpoint);
            UpdateHover(hasHit ? hit.collider.GetComponentInParent<XRWorldButton>() : null);

            var device = InputDevices.GetDeviceAtXRNode(node);
            if (!device.isValid)
            {
                return;
            }

            device.TryGetFeatureValue(CommonUsages.triggerButton, out var trigger);
            device.TryGetFeatureValue(CommonUsages.secondaryButton, out var cancel);

            if (trigger && !previousTrigger && hasHit)
            {
                Activate(hit.collider);
            }

            if (cancel && !previousCancel)
            {
                wireTool?.CancelPendingConnection();
            }

            previousTrigger = trigger;
            previousCancel = cancel;
        }

        private void Activate(Collider hitCollider)
        {
            var button = hitCollider.GetComponentInParent<XRWorldButton>();
            if (button != null)
            {
                button.Press();
                return;
            }

            var terminal = hitCollider.GetComponentInParent<CircuitTerminal>();
            if (terminal != null)
            {
                wireTool?.SelectTerminal(terminal);
                return;
            }

            var switchToggle = hitCollider.GetComponentInParent<XRSwitchToggle>();
            switchToggle?.Toggle();
        }

        private void UpdateHover(XRWorldButton next)
        {
            if (hoveredButton == next)
            {
                return;
            }

            hoveredButton?.SetHovered(false);
            hoveredButton = next;
            hoveredButton?.SetHovered(true);
        }

        private void OnDisable()
        {
            hoveredButton?.SetHovered(false);
            hoveredButton = null;
        }
    }
}
