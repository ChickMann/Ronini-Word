using UnityEngine;

namespace CLOUPT.Core
{
    /// <summary>
    /// CLOUPT SDK Settings - Contains all configuration required for the API system.
    /// This ScriptableObject stores persistent settings across the project.
    /// App ID is stored encrypted for build protection.
    /// </summary>
    [CreateAssetMenu(fileName = "CLOUPTSettings", menuName = "CLOUPT/Settings", order = 1)]
    public class CLOUPTSettings : ScriptableObject
    {
        private const string SETTINGS_PATH = "CLOUPTSettings";
        private static CLOUPTSettings _instance;

        [Header("Application Configuration (Encrypted)")]
        [Tooltip("Encrypted App ID - Do not modify manually.")]
        [SerializeField]
        private string _encryptedAppId = "";

        [Tooltip("Hash for integrity verification.")]
        [SerializeField]
        private string _appIdHash = "";

        [Header("Environment Settings")]
        [Tooltip("Enable debug mode for detailed logging.")]
        [SerializeField]
        private bool _debugMode = false;

        // Cached decrypted App ID (not serialized)
        private string _cachedAppId = null;
        private bool _cacheValid = false;

        /// <summary>
        /// Gets the Public App ID used for API authentication.
        /// Decrypts the stored value on first access.
        /// </summary>
        public string PublicAppId
        {
            get
            {
                if (!_cacheValid)
                {
                    _cachedAppId = DecryptAppId();
                    _cacheValid = true;
                }
                return _cachedAppId ?? "";
            }
        }

        /// <summary>
        /// Gets whether debug mode is enabled.
        /// </summary>
        public bool DebugMode => _debugMode;

        /// <summary>
        /// Gets the singleton instance of CLOUPT Settings.
        /// Loads from Resources folder automatically.
        /// </summary>
        public static CLOUPTSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<CLOUPTSettings>(SETTINGS_PATH);

                    if (_instance == null)
                    {
                        Debug.LogWarning("[CLOUPT] Settings not found. Please run CLOUPT Setup from Tools > CLOUPT > Setup.");
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Validates if the settings are properly configured.
        /// Also verifies integrity of the stored App ID.
        /// </summary>
        /// <returns>True if all required settings are valid.</returns>
        public bool IsValid()
        {
            string appId = PublicAppId;
            if (string.IsNullOrEmpty(appId))
                return false;

            // Verify integrity
            if (!VerifyIntegrity())
            {
                Debug.LogError("[CLOUPT] Settings integrity check failed. App ID may have been tampered with.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Verifies the integrity of the stored App ID using hash comparison.
        /// </summary>
        public bool VerifyIntegrity()
        {
            if (string.IsNullOrEmpty(_appIdHash))
                return true; // No hash stored (legacy or first setup)

            string decryptedId = PublicAppId;
            return CLOUPTSecurity.VerifyHash(decryptedId, _appIdHash);
        }

        /// <summary>
        /// Logs the current configuration status.
        /// </summary>
        public void LogStatus()
        {
            string appId = PublicAppId;
            if (!string.IsNullOrEmpty(appId))
            {
                string maskedId = appId.Length > 8 
                    ? appId.Substring(0, 4) + "****" + appId.Substring(appId.Length - 4) 
                    : "****";
                Debug.Log($"[CLOUPT] Settings loaded. App ID: {maskedId} | Integrity: {(VerifyIntegrity() ? "OK" : "FAILED")}");
            }
            else
            {
                Debug.LogError("[CLOUPT] Invalid configuration. Please check your Public App ID.");
            }
        }

        /// <summary>
        /// Decrypts the stored App ID.
        /// </summary>
        private string DecryptAppId()
        {
            if (string.IsNullOrEmpty(_encryptedAppId))
                return string.Empty;

            // Check if it's encrypted (Base64 format)
            if (CLOUPTSecurity.IsEncrypted(_encryptedAppId))
            {
                string decrypted = CLOUPTSecurity.Decrypt(_encryptedAppId);
                
                // If decryption returned empty but we have encrypted data, it's corrupted
                if (string.IsNullOrEmpty(decrypted) && !string.IsNullOrEmpty(_encryptedAppId))
                {
                    Debug.LogWarning("[CLOUPT] Encrypted App ID appears corrupted. Please re-enter your App ID in CLOUPT Setup.");
                    // Clear the corrupted data to prevent repeated errors
                    #if UNITY_EDITOR
                    _encryptedAppId = string.Empty;
                    _appIdHash = string.Empty;
                    UnityEditor.EditorUtility.SetDirty(this);
                    #endif
                    return string.Empty;
                }
                
                return decrypted;
            }

            // Legacy: plain text stored (migrate on next save)
            return _encryptedAppId;
        }

        private void OnEnable()
        {
            // Invalidate cache when asset is loaded/reloaded
            _cacheValid = false;
            _cachedAppId = null;
        }

#if UNITY_EDITOR
        // EditorPrefs key for API Key (never included in build)
        private const string API_KEY_PREF = "CLOUPT_ApiKey";

        /// <summary>
        /// Gets the API Key from EditorPrefs.
        /// This is NEVER included in builds - Editor only.
        /// </summary>
        public static string ApiKey
        {
            get => UnityEditor.EditorPrefs.GetString(API_KEY_PREF, "");
            set => UnityEditor.EditorPrefs.SetString(API_KEY_PREF, value);
        }

        /// <summary>
        /// Checks if API Key is configured.
        /// </summary>
        public static bool HasApiKey => !string.IsNullOrEmpty(ApiKey);

        /// <summary>
        /// Clears the stored API Key.
        /// </summary>
        public static void ClearApiKey()
        {
            UnityEditor.EditorPrefs.DeleteKey(API_KEY_PREF);
        }

        /// <summary>
        /// Sets the Public App ID. Encrypts and stores securely.
        /// Only available in Editor.
        /// </summary>
        /// <param name="appId">The Public App ID to set.</param>
        public void SetPublicAppId(string appId)
        {
            if (string.IsNullOrEmpty(appId))
            {
                _encryptedAppId = "";
                _appIdHash = "";
            }
            else
            {
                // Encrypt the App ID
                _encryptedAppId = CLOUPTSecurity.Encrypt(appId);
                
                // Store hash for integrity verification
                _appIdHash = CLOUPTSecurity.Hash(appId);
            }

            // Invalidate cache
            _cacheValid = false;
            _cachedAppId = null;
        }

        /// <summary>
        /// Gets the raw App ID for display in Editor (masked).
        /// </summary>
        public string GetMaskedAppId()
        {
            string appId = PublicAppId;
            if (string.IsNullOrEmpty(appId) || appId.Length < 8)
                return appId;

            return appId.Substring(0, 4) + new string('*', appId.Length - 8) + appId.Substring(appId.Length - 4);
        }

        /// <summary>
        /// Gets the raw decrypted App ID for Editor display only.
        /// </summary>
        public string GetRawAppIdForEditor()
        {
            return PublicAppId;
        }

        /// <summary>
        /// Sets the debug mode. Only available in Editor.
        /// </summary>
        /// <param name="enabled">Whether debug mode should be enabled.</param>
        public void SetDebugMode(bool enabled)
        {
            _debugMode = enabled;
        }
#endif
    }
}
