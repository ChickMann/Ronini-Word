using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using System.IO;

#pragma warning disable CS0414

namespace CLOUPT.Core.Editor
{
    /// <summary>
    /// CLOUPT Setup Window - Modern, clean UI inspired by cloupt.com website.
    /// Features a light theme with green accents and minimal design.
    /// </summary>
    [InitializeOnLoad]
    public class CLOUPTSetupWindow : EditorWindow
    {
        private const string SETTINGS_ASSET_PATH = "Assets/CLOUPT/Core/Resources/CLOUPTSettings.asset";
        private const string RESOURCES_FOLDER_PATH = "Assets/CLOUPT/Core/Resources";
        private const string VERSION = "1.0.0";
        private const string API_BASE_URL = "https://api.cloupt.com";
        private const string VALIDATE_ENDPOINT = "/api/v1/validate-app/";
        
        // Onboarding & Warning Constants
        private const string CLOUPT_WEBSITE_URL = "https://cloupt.com";
        private const string CLOUPT_DASHBOARD_URL = "https://cloupt.com/dashboard/apps";
        private const string CLOUPT_REGISTER_URL = "https://cloupt.com/login";
        private const string CLOUPT_DOCS_URL = "https://cloupt.com/getting-started";
        private const string PREFS_LAST_WARNING_TIME = "CLOUPT_LastWarningTime";
        private const string PREFS_WARNING_DISMISSED = "CLOUPT_WarningDismissed";
        private const string PREFS_ONBOARDING_SHOWN = "CLOUPT_OnboardingShown";
        private const double WARNING_INTERVAL_SECONDS = 300.0; // 5 minutes

        private CLOUPTSettings _settings;
        private string _publicAppId = "";
        private string _apiKey = "";
        private bool _showApiKey = false;
        private bool _debugMode = false;
        private Vector2 _scrollPosition;
        private bool _stylesInitialized = false;

        // Validation state
        private bool _isValidating = false;
        private string _validationStatus = "";
        private bool _isAppIdValid = false;
        private UnityWebRequest _activeRequest;

        // Modern Light Theme Colors (matching cloupt.com)
        private static readonly Color BgColor = new Color(0.976f, 0.980f, 0.984f, 1f);           // #F9FAFB - Light gray bg
        private static readonly Color CardBgColor = new Color(1f, 1f, 1f, 1f);                   // #FFFFFF - White cards
        private static readonly Color BorderColor = new Color(0.906f, 0.914f, 0.937f, 1f);      // #E7E9EF - Light border
        private static readonly Color PrimaryGreen = new Color(0.063f, 0.725f, 0.506f, 1f);     // #10B981 - Emerald green
        private static readonly Color PrimaryGreenLight = new Color(0.82f, 0.95f, 0.90f, 1f);   // Light green bg
        private static readonly Color TextPrimary = new Color(0.11f, 0.137f, 0.188f, 1f);       // #1C2330 - Dark text
        private static readonly Color TextSecondary = new Color(0.42f, 0.45f, 0.52f, 1f);       // #6B7280 - Gray text
        private static readonly Color TextMuted = new Color(0.62f, 0.65f, 0.70f, 1f);           // #9CA3AF - Muted text
        private static readonly Color InputBgColor = new Color(0.969f, 0.973f, 0.976f, 1f);     // #F7F8F9 - Input bg
        private static readonly Color InputBorderColor = new Color(0.85f, 0.87f, 0.90f, 1f);    // Input border
        private static readonly Color WarningColor = new Color(0.96f, 0.62f, 0.04f, 1f);        // #F59E0B - Amber
        private static readonly Color WarningBgColor = new Color(1f, 0.98f, 0.92f, 1f);         // Amber light bg
        private static readonly Color ErrorColor = new Color(0.94f, 0.27f, 0.27f, 1f);          // #EF4444 - Red
        private static readonly Color HeaderGradientStart = new Color(0.063f, 0.725f, 0.506f, 1f);
        private static readonly Color HeaderGradientEnd = new Color(0.05f, 0.65f, 0.55f, 1f);

        // Cached Textures
        private Texture2D _whiteTex;
        private Texture2D _cardTex;
        private Texture2D _inputTex;
        private Texture2D _greenTex;
        private Texture2D _headerGradient;

        // Styles
        private GUIStyle _logoStyle;
        private GUIStyle _taglineStyle;
        private GUIStyle _sectionTitleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _inputStyle;
        private GUIStyle _mutedTextStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _buttonPrimaryStyle;
        private GUIStyle _buttonSecondaryStyle;
        private GUIStyle _badgeStyle;
        private GUIStyle _statusDotStyle;

        // Static constructor for InitializeOnLoad
        static CLOUPTSetupWindow()
        {
            EditorApplication.delayCall += OnEditorStartup;
            EditorApplication.update += CheckPeriodicWarning;
        }

        /// <summary>
        /// Called when the editor starts up. Shows onboarding if App ID is not configured.
        /// </summary>
        private static void OnEditorStartup()
        {
            if (!IsAppIdConfigured())
            {
                // Show onboarding on first run or if not dismissed recently
                bool onboardingShown = EditorPrefs.GetBool(PREFS_ONBOARDING_SHOWN, false);
                
                if (!onboardingShown)
                {
                    ShowOnboardingDialog();
                    EditorPrefs.SetBool(PREFS_ONBOARDING_SHOWN, true);
                }
                else
                {
                    // Show persistent warning
                    ShowConfigurationWarning();
                }
            }
        }

        /// <summary>
        /// Periodically checks and warns if App ID is not configured.
        /// </summary>
        private static void CheckPeriodicWarning()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                return;

            if (!IsAppIdConfigured())
            {
                double lastWarning = EditorPrefs.GetFloat(PREFS_LAST_WARNING_TIME, 0f);
                double currentTime = EditorApplication.timeSinceStartup;

                if (currentTime - lastWarning > WARNING_INTERVAL_SECONDS)
                {
                    EditorPrefs.SetFloat(PREFS_LAST_WARNING_TIME, (float)currentTime);
                    ShowConfigurationWarning();
                }
            }
        }

        /// <summary>
        /// Checks if App ID is properly configured.
        /// </summary>
        private static bool IsAppIdConfigured()
        {
            var settings = AssetDatabase.LoadAssetAtPath<CLOUPTSettings>(SETTINGS_ASSET_PATH);
            if (settings == null) return false;
            
            string appId = settings.GetRawAppIdForEditor();
            return !string.IsNullOrEmpty(appId) && appId.Length >= 4;
        }

        /// <summary>
        /// Shows the initial onboarding dialog for first-time users.
        /// </summary>
        private static void ShowOnboardingDialog()
        {
            int choice = EditorUtility.DisplayDialogComplex(
                "🚀 Welcome to CLOUPT SDK!",
                "CLOUPT SDK requires an App ID to function.\n\n" +
                "To get started:\n" +
                "1. Create a free account at cloupt.com\n" +
                "2. Create a new app in your dashboard\n" +
                "3. Copy your App ID and paste it here\n\n" +
                "Without an App ID, the SDK features will NOT work!",
                "Get App ID (Open Website)",
                "Setup Later",
                "Open Setup Window"
            );

            switch (choice)
            {
                case 0: // Get App ID
                    Application.OpenURL(CLOUPT_REGISTER_URL);
                    ShowWindow();
                    break;
                case 1: // Setup Later - do nothing but warn
                    Debug.LogWarning("[CLOUPT] ⚠️ SDK is not configured! Go to Tools > CLOUPT > Setup to configure your App ID.");
                    break;
                case 2: // Open Setup Window
                    ShowWindow();
                    break;
            }
        }

        /// <summary>
        /// Shows a persistent warning about missing App ID configuration.
        /// </summary>
        private static void ShowConfigurationWarning()
        {
            bool openSetup = EditorUtility.DisplayDialog(
                "⚠️ CLOUPT: Configuration Required",
                "Your CLOUPT App ID is not configured!\n\n" +
                "The SDK will NOT function without a valid App ID.\n\n" +
                "Get your free App ID at cloupt.com",
                "Open Setup",
                "Remind Me Later"
            );

            if (openSetup)
            {
                ShowWindow();
            }
            else
            {
                Debug.LogWarning("[CLOUPT] ⚠️ REMINDER: Configure your App ID at Tools > CLOUPT > Setup. Visit https://cloupt.com to get your App ID.");
            }
        }

        [MenuItem("Tools/CLOUPT/Setup", false, 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<CLOUPTSetupWindow>("CLOUPT Setup");
            window.minSize = new Vector2(480, 680);
            window.maxSize = new Vector2(520, 780);
        }

        [MenuItem("Tools/CLOUPT/Get App ID (Website)", false, 2)]
        public static void OpenWebsite()
        {
            Application.OpenURL(CLOUPT_DASHBOARD_URL);
        }

        [MenuItem("Tools/CLOUPT/Documentation", false, 3)]
        public static void OpenDocs()
        {
            Application.OpenURL(CLOUPT_DOCS_URL);
        }

        private void OnEnable()
        {
            LoadSettings();
            CreateTextures();
        }

        private void OnDisable()
        {
            DestroyTextures();
        }

        private void CreateTextures()
        {
            _whiteTex = MakeTex(2, 2, CardBgColor);
            _cardTex = MakeTex(2, 2, CardBgColor);
            _inputTex = MakeTex(2, 2, InputBgColor);
            _greenTex = MakeTex(2, 2, PrimaryGreen);
            _headerGradient = CreateHeaderGradient(512, 140);
        }

        private void DestroyTextures()
        {
            if (_whiteTex) DestroyImmediate(_whiteTex);
            if (_cardTex) DestroyImmediate(_cardTex);
            if (_inputTex) DestroyImmediate(_inputTex);
            if (_greenTex) DestroyImmediate(_greenTex);
            if (_headerGradient) DestroyImmediate(_headerGradient);
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            var pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private Texture2D CreateHeaderGradient(int width, int height)
        {
            var tex = new Texture2D(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float t = (float)x / width;
                    // Subtle gradient with slight curve pattern
                    float wave = Mathf.Sin((float)x / width * Mathf.PI * 2f + (float)y / height * 3f) * 0.03f;
                    Color c = Color.Lerp(HeaderGradientStart, HeaderGradientEnd, t + wave);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            _logoStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 28,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(0, 0, 0, 0)
            };

            _taglineStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 1f, 1f, 0.9f) }
            };

            _sectionTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                normal = { textColor = TextPrimary },
                margin = new RectOffset(0, 0, 0, 8)
            };

            _labelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = TextSecondary },
                margin = new RectOffset(0, 0, 0, 4)
            };

            _inputStyle = new GUIStyle(EditorStyles.textField)
            {
                fontSize = 13,
                fixedHeight = 40,
                padding = new RectOffset(14, 14, 10, 10),
                normal = { background = _inputTex, textColor = TextPrimary },
                focused = { background = _inputTex, textColor = TextPrimary },
                border = new RectOffset(4, 4, 4, 4)
            };

            _mutedTextStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                normal = { textColor = TextMuted },
                wordWrap = true
            };

            _cardStyle = new GUIStyle()
            {
                normal = { background = _cardTex },
                padding = new RectOffset(20, 20, 16, 16),
                margin = new RectOffset(16, 16, 8, 8)
            };

            _buttonPrimaryStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                fixedHeight = 44,
                normal = { background = _greenTex, textColor = Color.white },
                hover = { background = _greenTex, textColor = Color.white },
                active = { background = _greenTex, textColor = Color.white },
                border = new RectOffset(6, 6, 6, 6)
            };

            _buttonSecondaryStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                fixedHeight = 36,
                normal = { background = _whiteTex, textColor = TextPrimary },
                hover = { background = _inputTex, textColor = TextPrimary },
                active = { background = _inputTex, textColor = TextPrimary }
            };

            _badgeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(8, 8, 3, 3)
            };

            _statusDotStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft
            };

            _stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitializeStyles();

            // Main background
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), BgColor);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUIStyle.none, GUIStyle.none);

            DrawHeader();
            DrawStatusCard();
            DrawAuthenticationCard();
            DrawApiKeyCard();
            DrawAdvancedCard();
            DrawActionButtons();
            DrawFooter();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            // Header with gradient
            Rect headerRect = GUILayoutUtility.GetRect(position.width, 120);
            GUI.DrawTexture(headerRect, _headerGradient, ScaleMode.StretchToFill);

            // Logo - "CLOUPT" with C highlighted
            GUILayout.BeginArea(new Rect(headerRect.x, headerRect.y + 30, headerRect.width, 40));
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            // C in bold/different style
            var cStyle = new GUIStyle(_logoStyle) { fontStyle = FontStyle.Bold };
            var louptStyle = new GUIStyle(_logoStyle) { fontStyle = FontStyle.Normal };
            
            GUILayout.Label("CLOUPT", _logoStyle);
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();

            // Tagline
            GUILayout.BeginArea(new Rect(headerRect.x, headerRect.y + 75, headerRect.width, 25));
            GUILayout.Label("Powerful Backend Tools for Unity Developers", _taglineStyle);
            GUILayout.EndArea();
        }

        private void DrawStatusCard()
        {
            EditorGUILayout.Space(8);
            
            bool isConfigured = _settings != null && _settings.IsValid();
            
            // If not configured, show attention-grabbing alert banner
            if (!isConfigured)
            {
                DrawAttentionBanner();
            }
            
            // Simple status bar (not a full card)
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            
            // Status dot and text
            var dotStyle = new GUIStyle(_statusDotStyle);
            dotStyle.normal.textColor = isConfigured ? PrimaryGreen : WarningColor;
            GUILayout.Label("●", dotStyle, GUILayout.Width(16));
            
            var statusTextStyle = new GUIStyle(_mutedTextStyle);
            statusTextStyle.fontStyle = FontStyle.Bold;
            statusTextStyle.normal.textColor = isConfigured ? PrimaryGreen : WarningColor;
            GUILayout.Label(isConfigured ? "Ready" : "Configuration Required", statusTextStyle);
            
            GUILayout.FlexibleSpace();
            
            // Environment badge
            DrawBadge("PRODUCTION", PrimaryGreen, PrimaryGreenLight);
            
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(4);
        }

        /// <summary>
        /// Draws an attention-grabbing alert banner when App ID is not configured.
        /// </summary>
        private void DrawAttentionBanner()
        {
            EditorGUILayout.Space(4);
            
            // Pulsing animation effect
            float pulse = Mathf.Abs(Mathf.Sin((float)EditorApplication.timeSinceStartup * 2f));
            Color alertBg = Color.Lerp(new Color(1f, 0.9f, 0.9f), new Color(1f, 0.95f, 0.95f), pulse);
            Color alertBorder = Color.Lerp(ErrorColor, WarningColor, pulse * 0.3f);
            
            // Alert container
            var alertStyle = new GUIStyle()
            {
                padding = new RectOffset(16, 16, 12, 12),
                margin = new RectOffset(16, 16, 4, 8)
            };
            
            EditorGUILayout.BeginVertical(alertStyle);
            
            // Get rect and draw background
            var bgRect = GUILayoutUtility.GetRect(0, 85, GUILayout.ExpandWidth(true));
            bgRect.x -= 16;
            bgRect.width += 32;
            bgRect.y -= 12;
            bgRect.height += 24;
            
            EditorGUI.DrawRect(bgRect, alertBg);
            DrawBorder(bgRect, alertBorder, 2);
            
            EditorGUILayout.EndVertical();
            
            // Draw content on top
            var contentRect = new Rect(bgRect.x + 16, bgRect.y + 12, bgRect.width - 32, bgRect.height - 24);
            
            // Warning icon and title
            var iconStyle = new GUIStyle(EditorStyles.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
            var titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 13, normal = { textColor = ErrorColor }, alignment = TextAnchor.MiddleLeft };
            var descStyle = new GUIStyle(EditorStyles.label) { fontSize = 11, normal = { textColor = TextSecondary }, wordWrap = true };
            
            GUI.Label(new Rect(contentRect.x, contentRect.y, 28, 24), "🚨", iconStyle);
            GUI.Label(new Rect(contentRect.x + 32, contentRect.y, contentRect.width - 32, 24), "ACTION REQUIRED: App ID Not Configured", titleStyle);
            GUI.Label(new Rect(contentRect.x, contentRect.y + 26, contentRect.width, 18), "CLOUPT SDK features will NOT work without a valid App ID.", descStyle);
            
            // CTA Button
            var btnRect = new Rect(contentRect.x, contentRect.y + 50, 180, 28);
            var btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white, background = _greenTex },
                hover = { textColor = Color.white, background = _greenTex },
                active = { textColor = Color.white, background = _greenTex }
            };
            
            if (GUI.Button(btnRect, "🌐 Get Free App ID", btnStyle))
            {
                Application.OpenURL(CLOUPT_REGISTER_URL);
            }
            
            // Secondary link
            var linkRect = new Rect(contentRect.x + 190, contentRect.y + 54, 120, 20);
            var linkStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 10,
                normal = { textColor = PrimaryGreen },
                fontStyle = FontStyle.Bold
            };
            
            if (GUI.Button(linkRect, "Already have one?", linkStyle))
            {
                // Focus on the App ID field - just scroll to it
                EditorGUI.FocusTextInControl("AppIdField");
            }
            EditorGUIUtility.AddCursorRect(linkRect, MouseCursor.Link);
            
            // Force repaint for animation
            Repaint();
        }

        private void DrawAuthenticationCard()
        {
            EditorGUILayout.BeginVertical(_cardStyle);
            
            // Title row with importance indicator when not configured
            EditorGUILayout.BeginHorizontal();
            
            bool hasAppId = !string.IsNullOrEmpty(_publicAppId) && _publicAppId.Length >= 4;
            string icon = hasAppId ? "🔑" : "⚠️";
            
            GUILayout.Label(icon, GUILayout.Width(22));
            GUILayout.Label("Authentication", _sectionTitleStyle);
            GUILayout.FlexibleSpace();
            
            if (hasAppId)
            {
                DrawBadge("AES-256", PrimaryGreen, PrimaryGreenLight);
            }
            else
            {
                DrawBadge("REQUIRED", ErrorColor, new Color(1f, 0.9f, 0.9f));
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(12);
            
            // App ID field with name for focus
            GUILayout.Label("PUBLIC APP ID", _labelStyle);
            
            EditorGUILayout.BeginHorizontal();
            var inputRect = EditorGUILayout.GetControlRect(GUILayout.Height(40));
            
            // Draw input background with border
            DrawInputField(inputRect);
            
            // The actual text field
            var textRect = new Rect(inputRect.x + 4, inputRect.y + 4, inputRect.width - 8, inputRect.height - 8);
            _publicAppId = EditorGUI.TextField(textRect, _publicAppId, _inputStyle);
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(4);
            
            // Help text with link
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Your unique identifier from the", _mutedTextStyle);
            
            var linkStyle = new GUIStyle(_mutedTextStyle)
            {
                normal = { textColor = PrimaryGreen },
                fontStyle = FontStyle.Bold
            };
            if (GUILayout.Button("CLOUPT Dashboard", linkStyle, GUILayout.ExpandWidth(false)))
            {
                Application.OpenURL("https://cloupt.com/dashboard/apps");
            }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(8);
            EditorGUILayout.EndVertical();
        }

        private void DrawApiKeyCard()
        {
            EditorGUILayout.BeginVertical(_cardStyle);
            
            // Title row with warning
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("🔐", GUILayout.Width(22));
            GUILayout.Label("API Key", _sectionTitleStyle);
            GUILayout.FlexibleSpace();
            DrawBadge("⚠ EDITOR ONLY", WarningColor, WarningBgColor);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(12);
            
            // API Key field
            GUILayout.Label("SECRET API KEY", _labelStyle);
            
            EditorGUILayout.BeginHorizontal();
            var inputRect = EditorGUILayout.GetControlRect(GUILayout.Height(40));
            
            // Adjust for button
            var fieldRect = new Rect(inputRect.x, inputRect.y, inputRect.width - 44, inputRect.height);
            var btnRect = new Rect(inputRect.x + inputRect.width - 40, inputRect.y, 40, inputRect.height);
            
            DrawInputField(fieldRect);
            
            var textRect = new Rect(fieldRect.x + 4, fieldRect.y + 4, fieldRect.width - 8, fieldRect.height - 8);
            
            EditorGUI.BeginChangeCheck();
            if (_showApiKey)
                _apiKey = EditorGUI.TextField(textRect, _apiKey, _inputStyle);
            else
                _apiKey = EditorGUI.PasswordField(textRect, _apiKey, _inputStyle);
            
            if (EditorGUI.EndChangeCheck())
            {
                CLOUPTSettings.ApiKey = _apiKey;
            }
            
            // Eye button
            var eyeStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fixedHeight = 40
            };
            if (GUI.Button(btnRect, _showApiKey ? "👁" : "👁‍🗨", eyeStyle))
            {
                _showApiKey = !_showApiKey;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(4);
            GUILayout.Label("Auto-saved • Your secret API Key from the CLOUPT Dashboard", _mutedTextStyle);
            
            // Warning box
            EditorGUILayout.Space(12);
            DrawWarningBox();
            
            EditorGUILayout.Space(8);
            EditorGUILayout.EndVertical();
        }

        private void DrawWarningBox()
        {
            // Warning box with proper layout
            var warningBoxStyle = new GUIStyle()
            {
                padding = new RectOffset(12, 12, 10, 10),
                margin = new RectOffset(0, 0, 0, 0)
            };
            
            // Get rect and draw background
            EditorGUILayout.BeginVertical(warningBoxStyle);
            
            var bgRect = GUILayoutUtility.GetRect(0, 52, GUILayout.ExpandWidth(true));
            bgRect.x -= 12;
            bgRect.width += 24;
            bgRect.y -= 10;
            bgRect.height += 20;
            
            // Background
            EditorGUI.DrawRect(bgRect, WarningBgColor);
            // Border
            DrawBorder(bgRect, new Color(WarningColor.r, WarningColor.g, WarningColor.b, 0.3f), 1);
            
            EditorGUILayout.EndVertical();
            
            // Draw content on top
            var contentRect = new Rect(bgRect.x + 12, bgRect.y + 8, bgRect.width - 24, bgRect.height - 16);
            
            // Title
            var iconStyle = new GUIStyle(EditorStyles.label) { fontSize = 13, normal = { textColor = WarningColor } };
            var titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 11, normal = { textColor = WarningColor } };
            var descStyle = new GUIStyle(EditorStyles.label) { fontSize = 10, normal = { textColor = TextSecondary }, wordWrap = true };
            
            GUI.Label(new Rect(contentRect.x, contentRect.y, 20, 18), "⚠", iconStyle);
            GUI.Label(new Rect(contentRect.x + 22, contentRect.y, 200, 18), "NEVER INCLUDED IN BUILD", titleStyle);
            GUI.Label(new Rect(contentRect.x, contentRect.y + 18, contentRect.width, 14), "This key is stored in EditorPrefs (your machine only).", descStyle);
            GUI.Label(new Rect(contentRect.x, contentRect.y + 32, contentRect.width, 14), "It will NOT be included in any build or shared with your team.", descStyle);
        }

        private void DrawAdvancedCard()
        {
            EditorGUILayout.BeginVertical(_cardStyle);
            
            // Title row
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("⚙️", GUILayout.Width(22));
            GUILayout.Label("Advanced Settings", _sectionTitleStyle);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(12);
            
            // Debug toggle
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Debug Mode", _labelStyle, GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            
            // Custom toggle
            var toggleRect = EditorGUILayout.GetControlRect(GUILayout.Width(60), GUILayout.Height(28));
            _debugMode = DrawToggle(toggleRect, _debugMode);
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(4);
            GUILayout.Label("Enable detailed logging for development and debugging", _mutedTextStyle);
            
            EditorGUILayout.Space(8);
            EditorGUILayout.EndVertical();
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.Space(4);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(16);
            
            EditorGUILayout.BeginVertical();
            
            // Primary save button
            bool canSave = !string.IsNullOrEmpty(_publicAppId) && _publicAppId.Length >= 4;
            
            GUI.enabled = canSave;
            GUI.backgroundColor = canSave ? PrimaryGreen : new Color(0.85f, 0.85f, 0.85f);
            
            if (GUILayout.Button("Save Configuration", _buttonPrimaryStyle))
            {
                SaveSettings();
            }
            
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
            
            EditorGUILayout.Space(8);
            
            // Secondary buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Reset", _buttonSecondaryStyle))
            {
                if (EditorUtility.DisplayDialog("Reset Configuration",
                    "Are you sure you want to reset all settings?", "Reset", "Cancel"))
                {
                    ResetSettings();
                }
            }
            
            if (GUILayout.Button("Validate", _buttonSecondaryStyle))
            {
                ValidateConfiguration();
            }
            
            if (GUILayout.Button("Refresh", _buttonSecondaryStyle))
            {
                LoadSettings();
                _stylesInitialized = false;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(16);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(8);
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space(8);
            
            // Quick Links Section
            DrawQuickLinksSection();
            
            EditorGUILayout.Space(8);
            
            // Divider
            var divRect = EditorGUILayout.GetControlRect(GUILayout.Height(1));
            EditorGUI.DrawRect(new Rect(divRect.x + 20, divRect.y, divRect.width - 40, 1), BorderColor);
            
            EditorGUILayout.Space(8);
            
            var footerStyle = new GUIStyle(_mutedTextStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10
            };
            GUILayout.Label($"© {System.DateTime.Now.Year} CLOUPT. All rights reserved. • v{VERSION}", footerStyle);
            
            EditorGUILayout.Space(12);
        }

        /// <summary>
        /// Draws quick links section for easy access to CLOUPT resources.
        /// </summary>
        private void DrawQuickLinksSection()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            
            var linkStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                fixedHeight = 24,
                normal = { textColor = PrimaryGreen }
            };
            
            if (GUILayout.Button("🌐 Website", linkStyle))
            {
                Application.OpenURL(CLOUPT_WEBSITE_URL);
            }
            
            if (GUILayout.Button("📊 Dashboard", linkStyle))
            {
                Application.OpenURL(CLOUPT_DASHBOARD_URL);
            }
            
            if (GUILayout.Button("📚 Docs", linkStyle))
            {
                Application.OpenURL(CLOUPT_DOCS_URL);
            }
            
            if (GUILayout.Button("💬 Support", linkStyle))
            {
                Application.OpenURL("https://cloupt.com/dashboard/tickets");
            }
            
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();
        }

        // Helper drawing methods
        private void DrawBadge(string text, Color textColor, Color bgColor)
        {
            var content = new GUIContent(text);
            var size = _badgeStyle.CalcSize(content);
            var rect = GUILayoutUtility.GetRect(size.x + 12, 22);
            
            // Rounded background
            EditorGUI.DrawRect(rect, bgColor);
            
            _badgeStyle.normal.textColor = textColor;
            GUI.Label(rect, text, _badgeStyle);
        }

        private void DrawInputField(Rect rect)
        {
            // Background
            EditorGUI.DrawRect(rect, InputBgColor);
            // Border
            DrawBorder(rect, InputBorderColor, 1);
        }

        private void DrawBorder(Rect rect, Color color, int thickness)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color); // Top
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), color); // Bottom
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color); // Left
            EditorGUI.DrawRect(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), color); // Right
        }

        private bool DrawToggle(Rect rect, bool value)
        {
            // Track background
            Color trackColor = value ? PrimaryGreen : new Color(0.8f, 0.8f, 0.8f);
            EditorGUI.DrawRect(rect, trackColor);
            
            // Thumb
            float thumbX = value ? rect.x + rect.width - 26 : rect.x + 2;
            var thumbRect = new Rect(thumbX, rect.y + 2, 24, rect.height - 4);
            EditorGUI.DrawRect(thumbRect, Color.white);
            
            // Click detection
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                return !value;
            }
            
            return value;
        }

        // Settings management
        private void LoadSettings()
        {
            _settings = AssetDatabase.LoadAssetAtPath<CLOUPTSettings>(SETTINGS_ASSET_PATH);
            
            if (_settings != null)
            {
                _publicAppId = _settings.GetRawAppIdForEditor();
                _debugMode = _settings.DebugMode;
            }
            
            _apiKey = CLOUPTSettings.ApiKey;
        }

        private void SaveSettings()
        {
            if (!AssetDatabase.IsValidFolder(RESOURCES_FOLDER_PATH))
            {
                string parentFolder = Path.GetDirectoryName(RESOURCES_FOLDER_PATH).Replace("\\", "/");
                string folderName = Path.GetFileName(RESOURCES_FOLDER_PATH);
                AssetDatabase.CreateFolder(parentFolder, folderName);
            }

            if (_settings == null)
            {
                _settings = CreateInstance<CLOUPTSettings>();
                AssetDatabase.CreateAsset(_settings, SETTINGS_ASSET_PATH);
            }

            _settings.SetPublicAppId(_publicAppId);
            _settings.SetDebugMode(_debugMode);
            CLOUPTSettings.ApiKey = _apiKey;

            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[CLOUPT] ✓ Configuration saved!");
            EditorUtility.DisplayDialog("Success", "Configuration saved successfully!", "OK");
        }

        private void ResetSettings()
        {
            _publicAppId = "";
            _apiKey = "";
            _debugMode = false;
            CLOUPTSettings.ApiKey = "";

            if (_settings != null)
            {
                _settings.SetPublicAppId("");
                _settings.SetDebugMode(false);
                EditorUtility.SetDirty(_settings);
                AssetDatabase.SaveAssets();
            }
        }

        private void ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(_publicAppId) || _publicAppId.Length < 4)
            {
                EditorUtility.DisplayDialog("Validation", "Please enter a valid App ID.", "OK");
                return;
            }

            ValidateAppIdAsync();
        }

        private void ValidateAppIdAsync()
        {
            if (_isValidating) return;

            _isValidating = true;
            string url = $"{API_BASE_URL}{VALIDATE_ENDPOINT}{_publicAppId}";
            _activeRequest = UnityWebRequest.Get(url);
            _activeRequest.timeout = 10;

            var operation = _activeRequest.SendWebRequest();
            operation.completed += OnValidationComplete;
        }

        private void OnValidationComplete(AsyncOperation operation)
        {
            _isValidating = false;

            if (_activeRequest == null) return;

            if (_activeRequest.result == UnityWebRequest.Result.Success && _activeRequest.responseCode == 200)
            {
                _isAppIdValid = true;
                EditorUtility.DisplayDialog("Success", "✓ App ID is valid!", "OK");
            }
            else if (_activeRequest.responseCode == 404)
            {
                _isAppIdValid = false;
                EditorUtility.DisplayDialog("Invalid", "App ID not found.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", $"Validation failed: {_activeRequest.error}", "OK");
            }

            _activeRequest.Dispose();
            _activeRequest = null;
            Repaint();
        }
    }
}

#pragma warning restore CS0414
