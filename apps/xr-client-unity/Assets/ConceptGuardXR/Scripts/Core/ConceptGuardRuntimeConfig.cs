using System;

namespace ConceptGuardXR
{
    [Serializable]
    public sealed class ConceptGuardRuntimeConfig
    {
        public string api_base_url;
        public int request_timeout_seconds = 10;
        public string session_id_prefix = "conceptguard";
        public string mission_id = "M2_SERIES_PARALLEL";
        public string locale = "ko-KR";

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(api_base_url))
            {
                throw new InvalidOperationException("api_base_url is required in conceptguard_xr_config.json.");
            }

            if (!Uri.TryCreate(api_base_url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new InvalidOperationException("api_base_url must be an absolute HTTP or HTTPS URL.");
            }

            if (request_timeout_seconds < 1)
            {
                throw new InvalidOperationException("request_timeout_seconds must be at least 1.");
            }

            if (string.IsNullOrWhiteSpace(mission_id))
            {
                throw new InvalidOperationException("mission_id is required in conceptguard_xr_config.json.");
            }

            if (string.IsNullOrWhiteSpace(locale))
            {
                locale = "en-US";
            }

            if (string.IsNullOrWhiteSpace(session_id_prefix))
            {
                session_id_prefix = "conceptguard";
            }
        }
    }
}
