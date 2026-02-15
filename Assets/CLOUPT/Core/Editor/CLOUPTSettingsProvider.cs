using UnityEngine;
using UnityEditor;
using System.IO;

namespace CLOUPT.Core.Editor
{
    /// <summary>
    /// CLOUPT Settings Provider - Integrates CLOUPT settings into Unity's Project Settings window.
    /// Provides a seamless experience for configuring SDK settings.
    /// </summary>
    public class CLOUPTSettingsProvider : SettingsProvider
    {
        private const string SETTINGS_PATH = "Project/CLOUPT";
        private const string SETTINGS_ASSET_PATH = "Assets/CLOUPT/Core/Resources/CLOUPTSettings.asset";
        private const string RESOURCES_FOLDER_PATH = "Assets/CLOUPT/Core/Resources";
        
        // CLOUPT URLs
        private const string CLOUPT_WEBSITE_URL = "https://cloupt.com";
        private const string CLOUPT_DASHBOARD_URL = "https://cloupt.com/dashboard/apps";
        private const string CLOUPT_REGISTER_URL = "https://cloupt.com/login";
        private const string CLOUPT_DOCS_URL = "https://cloupt.com/getting-started";

        private SerializedObject _serializedSettings;
        private CLOUPTSettings _settings;

        /// <summary>
        /// Creates a new instance of the CLOUPT Settings Provider.
        /// </summary>
        public CLOUPTSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
            : base(path, scope)
        {
            label = "CLOUPT";
            keywords = new[] { "CLOUPT", "API", "SDK", "App ID", "Configuration" };
        }

        /// <summary>
        /// Called when the settings provider is activated.
        /// </summary>
        public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
        {
            LoadOrCreateSettings();
        }

        /// <summary>
        /// Draws the settings GUI in the Project Settings window.
        /// </summary>
        public override void OnGUI(string searchContext)
        {
            if (_settings == null)
            {
                LoadOrCreateSettings();
            }

            if (_settings == null)
            {
                DrawCreateSettingsButton();
                return;
            }

            if (_serializedSettings == null || _serializedSettings.targetObject == null)
            {
                _serializedSettings = new SerializedObject(_settings);
            }

            _serializedSettings.Update();

            EditorGUILayout.Space(10);

            // Header
            EditorGUILayout.LabelField("CLOUPT SDK Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            var publicAppIdProperty = _serializedSettings.FindProperty("_publicAppId");
            var debugModeProperty = _serializedSettings.FindProperty("_debugMode");
            
            // Check if App ID is configured
            bool isConfigured = !string.IsNullOrEmpty(publicAppIdProperty.stringValue) && 
                               publicAppIdProperty.stringValue.Length >= 4;
            
            if (!isConfigured)
            {
                // Draw prominent warning box
                DrawUnconfiguredWarning();
            }
            else
            {
                EditorGUILayout.HelpBox("✓ CLOUPT SDK is properly configured and ready to use.", MessageType.Info);
            }
            
            EditorGUILayout.Space(10);

            // Draw properties
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(publicAppIdProperty, new GUIContent("Public App ID", "Your unique application identifier provided by CLOUPT."));
            EditorGUILayout.PropertyField(debugModeProperty, new GUIContent("Debug Mode", "Enable detailed logging for development."));

            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10);

            // Quick Links
            EditorGUILayout.LabelField("Quick Links", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("🌐 Get App ID", GUILayout.Height(25)))
            {
                Application.OpenURL(CLOUPT_REGISTER_URL);
            }
            
            if (GUILayout.Button("📊 Dashboard", GUILayout.Height(25)))
            {
                Application.OpenURL(CLOUPT_DASHBOARD_URL);
            }
            
            if (GUILayout.Button("📚 Documentation", GUILayout.Height(25)))
            {
                Application.OpenURL(CLOUPT_DOCS_URL);
            }
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Action buttons
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Open Setup Window"))
            {
                CLOUPTSetupWindow.ShowWindow();
            }

            if (GUILayout.Button("Validate Settings"))
            {
                ValidateSettings();
            }

            EditorGUILayout.EndHorizontal();

            // Apply changes
            if (_serializedSettings.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_settings);
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Draws a prominent warning when App ID is not configured.
        /// </summary>
        private void DrawUnconfiguredWarning()
        {
            // Error box with strong messaging
            EditorGUILayout.HelpBox(
                "⚠️ ACTION REQUIRED: App ID Not Configured!\n\n" +
                "CLOUPT SDK will NOT function without a valid App ID.\n" +
                "Your API calls, authentication, and all SDK features will fail.\n\n" +
                "→ Click 'Get App ID' below to create your free account and get an App ID.",
                MessageType.Error
            );
            
            EditorGUILayout.Space(5);
            
            // Prominent CTA button
            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.063f, 0.725f, 0.506f, 1f); // Green
            
            if (GUILayout.Button("🚀 Get Your Free App ID Now", GUILayout.Height(35)))
            {
                // Show confirmation and open website
                bool openWebsite = EditorUtility.DisplayDialog(
                    "Get CLOUPT App ID",
                    "You'll be redirected to cloupt.com to:\n\n" +
                    "1. Create a free account (if you don't have one)\n" +
                    "2. Create a new app in your dashboard\n" +
                    "3. Copy your App ID\n\n" +
                    "Then paste it in the 'Public App ID' field above.",
                    "Open cloupt.com",
                    "Cancel"
                );
                
                if (openWebsite)
                {
                    Application.OpenURL(CLOUPT_REGISTER_URL);
                }
            }
            
            GUI.backgroundColor = originalColor;
        }

        /// <summary>
        /// Draws the button to create settings when they don't exist.
        /// </summary>
        private void DrawCreateSettingsButton()
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox("CLOUPT Settings asset not found. Click the button below to create one.", MessageType.Warning);
            EditorGUILayout.Space(10);

            if (GUILayout.Button("Create CLOUPT Settings", GUILayout.Height(30)))
            {
                CreateSettings();
            }
        }

        /// <summary>
        /// Loads existing settings or creates new ones if they don't exist.
        /// </summary>
        private void LoadOrCreateSettings()
        {
            _settings = AssetDatabase.LoadAssetAtPath<CLOUPTSettings>(SETTINGS_ASSET_PATH);

            if (_settings != null)
            {
                _serializedSettings = new SerializedObject(_settings);
            }
        }

        /// <summary>
        /// Creates a new settings asset.
        /// </summary>
        private void CreateSettings()
        {
            // Ensure Resources folder exists
            if (!AssetDatabase.IsValidFolder(RESOURCES_FOLDER_PATH))
            {
                string parentFolder = Path.GetDirectoryName(RESOURCES_FOLDER_PATH).Replace("\\", "/");
                string folderName = Path.GetFileName(RESOURCES_FOLDER_PATH);
                AssetDatabase.CreateFolder(parentFolder, folderName);
            }

            // Create settings asset
            _settings = ScriptableObject.CreateInstance<CLOUPTSettings>();
            AssetDatabase.CreateAsset(_settings, SETTINGS_ASSET_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _serializedSettings = new SerializedObject(_settings);

            Debug.Log("[CLOUPT] Settings asset created successfully!");
        }

        /// <summary>
        /// Validates the current settings configuration.
        /// </summary>
        private void ValidateSettings()
        {
            if (_settings == null)
            {
                EditorUtility.DisplayDialog("Validation Failed", "Settings asset not found.", "OK");
                return;
            }

            if (_settings.IsValid())
            {
                EditorUtility.DisplayDialog("Validation Successful", "All CLOUPT settings are properly configured!", "OK");
                Debug.Log("[CLOUPT] Settings validation passed.");
            }
            else
            {
                EditorUtility.DisplayDialog("Validation Failed", "Please ensure all required fields are filled in correctly.", "OK");
                Debug.LogWarning("[CLOUPT] Settings validation failed. Please check your configuration.");
            }
        }

        /// <summary>
        /// Registers the settings provider with Unity.
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider CreateCLOUPTSettingsProvider()
        {
            return new CLOUPTSettingsProvider(SETTINGS_PATH, SettingsScope.Project);
        }
    }
}
