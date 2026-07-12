using UnityEngine;

namespace ConceptGuardXR
{
    public sealed class XRGrabbable : MonoBehaviour
    {
        private Transform originalParent;
        private Vector3 homePosition;
        private Quaternion homeRotation;
        private bool grabbed;
        private float benchHeight = 1.08f;
        private Vector2 xBounds = new Vector2(-1.55f, 1.55f);
        private Vector2 zBounds = new Vector2(-0.55f, 0.72f);

        public bool IsGrabbed => grabbed;

        public void Configure(float surfaceHeight, Vector2 horizontalBounds, Vector2 depthBounds)
        {
            benchHeight = surfaceHeight;
            xBounds = horizontalBounds;
            zBounds = depthBounds;
            originalParent = transform.parent;
            homePosition = transform.position;
            homeRotation = transform.rotation;
        }

        public void BeginGrab(Transform hand)
        {
            if (grabbed || hand == null)
            {
                return;
            }

            grabbed = true;
            transform.SetParent(hand, true);
        }

        public void EndGrab()
        {
            if (!grabbed)
            {
                return;
            }

            grabbed = false;
            transform.SetParent(originalParent, true);
            var position = transform.position;
            position.y = benchHeight;
            position.x = Mathf.Clamp(position.x, xBounds.x, xBounds.y);
            position.z = Mathf.Clamp(position.z, zBounds.x, zBounds.y);
            transform.position = position;
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        }

        public void ResetToHome()
        {
            grabbed = false;
            transform.SetParent(originalParent, true);
            transform.SetPositionAndRotation(homePosition, homeRotation);
        }
    }
}
