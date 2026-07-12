using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace ConceptGuardXR
{
    public static class ConceptGuardConfigLoader
    {
        private const string FileName = "conceptguard_xr_config.json";

        public static IEnumerator Load(Action<ConceptGuardRuntimeConfig> onSuccess, Action<string> onFailure)
        {
            var path = Path.Combine(Application.streamingAssetsPath, FileName);
            string json;

            if (path.Contains("://") || path.Contains(":///"))
            {
                using var request = UnityWebRequest.Get(path);
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onFailure?.Invoke($"Failed to load {FileName}: {request.error}");
                    yield break;
                }

                json = request.downloadHandler.text;
            }
            else
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        onFailure?.Invoke($"Required configuration file not found: {path}");
                        yield break;
                    }

                    json = File.ReadAllText(path);
                }
                catch (Exception exception)
                {
                    onFailure?.Invoke($"Failed to read {FileName}: {exception.Message}");
                    yield break;
                }
            }

            try
            {
                var config = JsonUtility.FromJson<ConceptGuardRuntimeConfig>(json);
                if (config == null)
                {
                    throw new InvalidOperationException("The configuration JSON could not be parsed.");
                }

                config.Validate();
                onSuccess?.Invoke(config);
            }
            catch (Exception exception)
            {
                onFailure?.Invoke($"Invalid {FileName}: {exception.Message}");
            }
        }
    }
}
