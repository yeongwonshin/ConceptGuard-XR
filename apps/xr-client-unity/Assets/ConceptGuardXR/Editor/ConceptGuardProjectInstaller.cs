#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;

namespace ConceptGuardXR.Editor
{
    [InitializeOnLoad]
    public static class ConceptGuardProjectInstaller
    {
        private const string OpenXrLoaderType = "UnityEngine.XR.OpenXR.OpenXRLoader";
        private const string SessionKey = "ConceptGuardXR.ProjectPrepared";

        static ConceptGuardProjectInstaller()
        {
            EditorApplication.delayCall += PrepareOncePerEditorSession;
        }

        [MenuItem("ConceptGuard XR/Prepare Unity Project")]
        public static void PrepareProject()
        {
            PlayerSettings.companyName = "ConceptGuard";
            PlayerSettings.productName = "ConceptGuard XR";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.conceptguard.xr");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;

            EnsureFolder("Assets/XR");
            EnsureFolder("Assets/XR/Settings");
            var activeTarget = EditorUserBuildSettings.activeBuildTarget;
            var activeGroup = BuildPipeline.GetBuildTargetGroup(activeTarget);
            if (activeGroup != BuildTargetGroup.Unknown)
            {
                ConfigureOpenXr(activeTarget, activeGroup);
            }
            if (activeGroup != BuildTargetGroup.Android)
            {
                ConfigureOpenXr(BuildTarget.Android, BuildTargetGroup.Android);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("ConceptGuard XR project settings prepared. OpenXR is assigned for Standalone and Android.");
        }

        private static void PrepareOncePerEditorSession()
        {
            if (SessionState.GetBool(SessionKey, false))
            {
                return;
            }

            SessionState.SetBool(SessionKey, true);
            PrepareProject();
        }

        private static void ConfigureOpenXr(BuildTarget target, BuildTargetGroup group)
        {
            var generalSettings = XRGeneralSettingsPerBuildTarget.SettingsForBuildTarget(target);
            if (generalSettings == null)
            {
                Debug.LogError($"Could not create XR settings for {target}.");
                return;
            }

            if (generalSettings.AssignedSettings == null)
            {
                var assetPath = $"Assets/XR/Settings/{group} XR Manager Settings.asset";
                var manager = AssetDatabase.LoadAssetAtPath<XRManagerSettings>(assetPath);
                if (manager == null)
                {
                    manager = ScriptableObject.CreateInstance<XRManagerSettings>();
                    manager.name = $"{group} XR Manager Settings";
                    AssetDatabase.CreateAsset(manager, assetPath);
                }
                generalSettings.AssignedSettings = manager;
            }

            generalSettings.InitManagerOnStart = true;
            EditorUtility.SetDirty(generalSettings);

            if (!XRPackageMetadataStore.IsLoaderAssigned(OpenXrLoaderType, group))
            {
                var assigned = XRPackageMetadataStore.AssignLoader(
                    generalSettings.AssignedSettings,
                    OpenXrLoaderType,
                    group
                );
                if (!assigned)
                {
                    Debug.LogError($"OpenXR loader assignment failed for {group}. Use Edit > Project Settings > XR Plug-in Management to enable OpenXR manually.");
                }
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var folder = Path.GetFileName(path);
            if (!string.IsNullOrWhiteSpace(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
#endif
