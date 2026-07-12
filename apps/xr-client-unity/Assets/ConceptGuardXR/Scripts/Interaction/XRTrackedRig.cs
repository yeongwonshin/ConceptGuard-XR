using UnityEngine;
using UnityEngine.XR;

namespace ConceptGuardXR
{
    public sealed class XRTrackedRig : MonoBehaviour
    {
        private Transform head;
        private Transform leftHand;
        private Transform rightHand;

        public Transform Head => head;
        public Transform LeftHand => leftHand;
        public Transform RightHand => rightHand;

        public void Configure(Transform headTransform, Transform leftHandTransform, Transform rightHandTransform)
        {
            head = headTransform;
            leftHand = leftHandTransform;
            rightHand = rightHandTransform;
        }

        private void Update()
        {
            UpdatePose(XRNode.CenterEye, head);
            UpdatePose(XRNode.LeftHand, leftHand);
            UpdatePose(XRNode.RightHand, rightHand);
        }

        private static void UpdatePose(XRNode node, Transform target)
        {
            if (target == null)
            {
                return;
            }

            var device = InputDevices.GetDeviceAtXRNode(node);
            if (!device.isValid)
            {
                return;
            }

            if (device.TryGetFeatureValue(CommonUsages.devicePosition, out var position))
            {
                target.localPosition = position;
            }

            if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out var rotation))
            {
                target.localRotation = rotation;
            }
        }
    }
}
