using UnityEngine;

namespace ConceptGuardXR
{
    public static class ConceptGuardRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateApplication()
        {
            if (Object.FindObjectOfType<ConceptGuardApplication>() != null)
            {
                return;
            }

            var root = new GameObject("ConceptGuard XR Application");
            root.AddComponent<ConceptGuardApplication>();
        }
    }
}
