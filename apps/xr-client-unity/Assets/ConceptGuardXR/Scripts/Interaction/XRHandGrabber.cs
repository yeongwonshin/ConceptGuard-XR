using UnityEngine;
using UnityEngine.XR;

namespace ConceptGuardXR
{
    public sealed class XRHandGrabber : MonoBehaviour
    {
        private XRNode node;
        private float grabRadius = 0.12f;
        private bool previousGrip;
        private XRGrabbable grabbedObject;

        public void Configure(XRNode xrNode, float radius)
        {
            node = xrNode;
            grabRadius = Mathf.Max(0.04f, radius);
        }

        private void Update()
        {
            var device = InputDevices.GetDeviceAtXRNode(node);
            if (!device.isValid || !device.TryGetFeatureValue(CommonUsages.gripButton, out var gripPressed))
            {
                return;
            }

            if (gripPressed && !previousGrip)
            {
                TryGrab();
            }
            else if (!gripPressed && previousGrip)
            {
                Release();
            }

            previousGrip = gripPressed;
        }

        private void TryGrab()
        {
            var colliders = Physics.OverlapSphere(transform.position, grabRadius, ~0, QueryTriggerInteraction.Collide);
            XRGrabbable closest = null;
            var closestDistance = float.MaxValue;
            foreach (var collider in colliders)
            {
                var candidate = collider.GetComponentInParent<XRGrabbable>();
                if (candidate == null || candidate.IsGrabbed)
                {
                    continue;
                }

                var distance = Vector3.SqrMagnitude(candidate.transform.position - transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = candidate;
                }
            }

            grabbedObject = closest;
            grabbedObject?.BeginGrab(transform);
        }

        private void Release()
        {
            grabbedObject?.EndGrab();
            grabbedObject = null;
        }

        private void OnDisable()
        {
            Release();
        }
    }
}
