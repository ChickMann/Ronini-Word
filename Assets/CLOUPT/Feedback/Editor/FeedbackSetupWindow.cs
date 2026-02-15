using UnityEngine;
using UnityEditor;
using CLOUPT.Core;

namespace CLOUPT.Feedback.Editor
{
    /// <summary>
    /// CLOUPT Feedback Setup Window - Modern, clean UI inspired by cloupt.com website.
    /// Features a light theme with green accents and minimal design.
    /// </summary>
    public class FeedbackSetupWindow : EditorWindow
    {
        private const string VERSION = "1.0.0";

        private Vector2 _scrollPosition;
        private bool _stylesInitialized = false;

        // Test feedback fields
        private FeedbackType _testType = FeedbackType.Feedback;
        private FeedbackPriority _testPriority = FeedbackPriority.Medium;
        private string _testHeader = "";
        private string _testMessage = "";
        private int _testRating = 5;

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
        private GUIStyle _codeBoxStyle;

        [MenuItem("Tools/CLOUPT/Feedback", false, 2)]
        public static void ShowWindow()
        {
            var window = GetWindow<FeedbackSetupWindow>("CLOUPT Feedback");
            window.minSize = new Vector2(480, 620);
            window.maxSize = new Vector2(520, 720);
        }

        private void OnEnable()
        {
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
            _headerGradient = CreateHeaderGradient(512, 120);
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
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                fontStyle = FontStyle.Bold
            };

            _taglineStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
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
                fontSize = 12,
                padding = new RectOffset(10, 10, 8, 8),
                normal = { background = _inputTex, textColor = TextPrimary },
                focused = { background = _inputTex, textColor = TextPrimary }
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
                active = { background = _greenTex, textColor = Color.white }
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

            _codeBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 11,
                padding = new RectOffset(10, 10, 8, 8),
                normal = { textColor = TextPrimary }
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
            DrawTestSection();
            DrawQuickStartSection();
            DrawFooter();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            Rect headerRect = GUILayoutUtility.GetRect(position.width, 100);
            GUI.DrawTexture(headerRect, _headerGradient, ScaleMode.StretchToFill);

            // Logo
            GUILayout.BeginArea(new Rect(headerRect.x, headerRect.y + 25, headerRect.width, 35));
            GUILayout.Label("CLOUPT Feedback", _logoStyle);
            GUILayout.EndArea();

            // Tagline
            GUILayout.BeginArea(new Rect(headerRect.x, headerRect.y + 62, headerRect.width, 25));
            GUILayout.Label("Collect player feedback directly from your game", _taglineStyle);
            GUILayout.EndArea();

            // Version badge
            var versionStyle = new GUIStyle(_taglineStyle) { alignment = TextAnchor.MiddleRight, fontSize = 10 };
            GUILayout.BeginArea(new Rect(headerRect.width - 70, headerRect.y + 8, 60, 20));
            GUILayout.Label($"v{VERSION}", versionStyle);
            GUILayout.EndArea();
        }

        private void DrawStatusCard()
        {
            EditorGUILayout.Space(8);

            // Status bar
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);

            bool isConfigured = CLOUPTSettings.Instance != null && CLOUPTSettings.Instance.IsValid();

            // Status dot
            var dotStyle = new GUIStyle(_statusDotStyle);
            dotStyle.normal.textColor = isConfigured ? PrimaryGreen : WarningColor;
            GUILayout.Label("●", dotStyle, GUILayout.Width(16));

            // Status text
            var statusTextStyle = new GUIStyle(_mutedTextStyle);
            statusTextStyle.fontStyle = FontStyle.Bold;
            statusTextStyle.normal.textColor = isConfigured ? PrimaryGreen : WarningColor;
            GUILayout.Label(isConfigured ? "Ready to collect feedback" : "SDK not configured", statusTextStyle);

            GUILayout.FlexibleSpace();

            if (!isConfigured)
            {
                if (GUILayout.Button("Configure SDK", _buttonSecondaryStyle, GUILayout.Width(100), GUILayout.Height(28)))
                {
                    EditorApplication.ExecuteMenuItem("Tools/CLOUPT/Setup");
                }
            }
            else
            {
                DrawBadge("ACTIVE", PrimaryGreen, PrimaryGreenLight);
            }

            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();

            if (isConfigured)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(36);
                GUILayout.Label($"App ID: {CLOUPTSettings.Instance.PublicAppId.Substring(0, Mathf.Min(12, CLOUPTSettings.Instance.PublicAppId.Length))}...", _mutedTextStyle);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(4);
        }

        private void DrawTestSection()
        {
            EditorGUILayout.BeginVertical(_cardStyle);

            // Title row
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("🧪", GUILayout.Width(22));
            GUILayout.Label("Test Feedback", _sectionTitleStyle);
            GUILayout.FlexibleSpace();
            DrawBadge("EDITOR", TextSecondary, InputBgColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);
            DrawDivider();
            EditorGUILayout.Space(12);

            // Type selection
            GUILayout.Label("FEEDBACK TYPE", _labelStyle);
            EditorGUILayout.Space(4);
            
            var enumRect = EditorGUILayout.GetControlRect(GUILayout.Height(40));
            DrawCustomDropdown(enumRect, ref _testType);

            EditorGUILayout.Space(12);

            // Priority selection
            GUILayout.Label("PRIORITY", _labelStyle);
            EditorGUILayout.Space(4);
            
            var priorityRect = EditorGUILayout.GetControlRect(GUILayout.Height(40));
            DrawPriorityDropdown(priorityRect, ref _testPriority);

            EditorGUILayout.Space(12);

            // Header
            GUILayout.Label("HEADER (OPTIONAL)", _labelStyle);
            EditorGUILayout.Space(4);
            
            var headerRect = EditorGUILayout.GetControlRect(GUILayout.Height(36));
            DrawInputField(headerRect);
            var headerInnerRect = new Rect(headerRect.x + 4, headerRect.y + 4, headerRect.width - 8, headerRect.height - 8);
            var headerStyle = new GUIStyle(EditorStyles.textField)
            {
                fontSize = 12,
                padding = new RectOffset(8, 8, 6, 6)
            };
            _testHeader = EditorGUI.TextField(headerInnerRect, _testHeader, headerStyle);

            EditorGUILayout.Space(12);

            // Message
            GUILayout.Label("MESSAGE", _labelStyle);
            EditorGUILayout.Space(4);
            
            var textAreaRect = EditorGUILayout.GetControlRect(GUILayout.Height(80));
            DrawInputField(textAreaRect);
            var textInnerRect = new Rect(textAreaRect.x + 4, textAreaRect.y + 4, textAreaRect.width - 8, textAreaRect.height - 8);
            var textAreaStyle = new GUIStyle(EditorStyles.textArea)
            {
                fontSize = 12,
                padding = new RectOffset(8, 8, 8, 8),
                wordWrap = true
            };
            _testMessage = EditorGUI.TextArea(textInnerRect, _testMessage, textAreaStyle);

            EditorGUILayout.Space(12);

            // Rating (only for Feedback type)
            if (_testType == FeedbackType.Feedback)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("RATING", _labelStyle, GUILayout.Width(60));
                GUILayout.FlexibleSpace();
                
                // Star rating display
                var starStyle = new GUIStyle(EditorStyles.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
                for (int i = 1; i <= 5; i++)
                {
                    starStyle.normal.textColor = i <= _testRating ? PrimaryGreen : TextMuted;
                    if (GUILayout.Button(i <= _testRating ? "★" : "☆", starStyle, GUILayout.Width(24), GUILayout.Height(24)))
                    {
                        _testRating = i;
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(12);
            }

            // Send button
            bool canSend = CLOUPTSettings.Instance != null && CLOUPTSettings.Instance.IsValid() 
                && !string.IsNullOrEmpty(_testMessage) && _testMessage.Length >= 5;
            
            GUI.enabled = canSend;
            GUI.backgroundColor = canSend ? PrimaryGreen : new Color(0.85f, 0.85f, 0.85f);

            if (GUILayout.Button("📤  Send Test Feedback", _buttonPrimaryStyle))
            {
                SendTestFeedback();
            }

            GUI.backgroundColor = Color.white;
            GUI.enabled = true;

            // Validation message
            if (!string.IsNullOrEmpty(_testMessage) && _testMessage.Length < 5)
            {
                EditorGUILayout.Space(8);
                DrawInfoBox("Message must be at least 5 characters.", WarningColor, WarningBgColor);
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.EndVertical();
        }

        private void DrawQuickStartSection()
        {
            EditorGUILayout.BeginVertical(_cardStyle);

            // Title row
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("📋", GUILayout.Width(22));
            GUILayout.Label("Quick Start", _sectionTitleStyle);
            GUILayout.FlexibleSpace();
            DrawBadge("API", PrimaryGreen, PrimaryGreenLight);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);
            DrawDivider();
            EditorGUILayout.Space(12);

            // Code examples
            DrawCodeExample("Submit feedback:", "Cloupt.Feedback.Rate(\"Great game!\", 5);");
            EditorGUILayout.Space(8);
            DrawCodeExample("Submit bug report:", "Cloupt.Feedback.Bug(\"Player stuck on level 3\");");
            EditorGUILayout.Space(8);
            DrawCodeExample("Submit feature request:", "Cloupt.Feedback.Feature(\"Add multiplayer mode\");");
            EditorGUILayout.Space(8);
            DrawCodeExample("Submit crash report:", "Cloupt.Feedback.Crash(exception);");

            EditorGUILayout.Space(4);
            EditorGUILayout.EndVertical();
        }

        private void DrawCodeExample(string label, string code)
        {
            GUILayout.Label(label, _labelStyle);
            EditorGUILayout.Space(2);
            
            var codeRect = EditorGUILayout.GetControlRect(GUILayout.Height(32));
            EditorGUI.DrawRect(codeRect, InputBgColor);
            DrawBorder(codeRect, InputBorderColor, 1);
            
            var codeStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Normal,
                normal = { textColor = TextPrimary },
                padding = new RectOffset(10, 10, 8, 8)
            };
            
            EditorGUI.SelectableLabel(codeRect, code, codeStyle);
        }

        private void DrawFooter()
        {
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

        // Helper drawing methods
        private void DrawBadge(string text, Color textColor, Color bgColor)
        {
            var content = new GUIContent(text);
            var size = _badgeStyle.CalcSize(content);
            var rect = GUILayoutUtility.GetRect(size.x + 12, 22);

            EditorGUI.DrawRect(rect, bgColor);
            _badgeStyle.normal.textColor = textColor;
            GUI.Label(rect, text, _badgeStyle);
        }

        private void DrawInputField(Rect rect)
        {
            EditorGUI.DrawRect(rect, InputBgColor);
            DrawBorder(rect, InputBorderColor, 1);
        }

        private void DrawBorder(Rect rect, Color color, int thickness)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), color);
        }

        private void DrawDivider()
        {
            var divRect = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(divRect, BorderColor);
        }

        private void DrawCustomDropdown(Rect rect, ref FeedbackType value)
        {
            // Background
            EditorGUI.DrawRect(rect, CardBgColor);
            DrawBorder(rect, InputBorderColor, 1);
            
            // Current value text
            var textStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 13,
                normal = { textColor = TextPrimary },
                padding = new RectOffset(14, 40, 0, 0),
                alignment = TextAnchor.MiddleLeft
            };
            
            string displayText = value.ToString();
            GUI.Label(rect, displayText, textStyle);
            
            // Dropdown arrow
            var arrowStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                normal = { textColor = TextSecondary },
                alignment = TextAnchor.MiddleCenter
            };
            var arrowRect = new Rect(rect.x + rect.width - 32, rect.y, 28, rect.height);
            GUI.Label(arrowRect, "▼", arrowStyle);
            
            // Click to open menu
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                
                var menu = new GenericMenu();
                foreach (FeedbackType type in System.Enum.GetValues(typeof(FeedbackType)))
                {
                    FeedbackType captured = type;
                    menu.AddItem(new GUIContent(type.ToString()), value == type, () => 
                    {
                        _testType = captured;
                    });
                }
                menu.DropDown(rect);
            }
        }

        private void DrawPriorityDropdown(Rect rect, ref FeedbackPriority value)
        {
            // Background
            EditorGUI.DrawRect(rect, CardBgColor);
            DrawBorder(rect, InputBorderColor, 1);
            
            // Current value text
            var textStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 13,
                normal = { textColor = TextPrimary },
                padding = new RectOffset(14, 40, 0, 0),
                alignment = TextAnchor.MiddleLeft
            };
            
            string displayText = value.ToString();
            GUI.Label(rect, displayText, textStyle);
            
            // Priority color indicator
            Color priorityColor = GetPriorityColor(value);
            var colorRect = new Rect(rect.x + 10, rect.y + rect.height / 2 - 4, 8, 8);
            EditorGUI.DrawRect(colorRect, priorityColor);
            
            // Dropdown arrow
            var arrowStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                normal = { textColor = TextSecondary },
                alignment = TextAnchor.MiddleCenter
            };
            var arrowRect = new Rect(rect.x + rect.width - 32, rect.y, 28, rect.height);
            GUI.Label(arrowRect, "▼", arrowStyle);
            
            // Click to open menu
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                
                var menu = new GenericMenu();
                foreach (FeedbackPriority priority in System.Enum.GetValues(typeof(FeedbackPriority)))
                {
                    FeedbackPriority captured = priority;
                    menu.AddItem(new GUIContent(priority.ToString()), value == priority, () => 
                    {
                        _testPriority = captured;
                    });
                }
                menu.DropDown(rect);
            }
        }

        private Color GetPriorityColor(FeedbackPriority priority)
        {
            switch (priority)
            {
                case FeedbackPriority.Low: return new Color(0.4f, 0.7f, 0.4f); // Green
                case FeedbackPriority.Medium: return new Color(0.9f, 0.7f, 0.2f); // Yellow
                case FeedbackPriority.High: return new Color(0.9f, 0.5f, 0.2f); // Orange
                case FeedbackPriority.Critical: return new Color(0.9f, 0.2f, 0.2f); // Red
                default: return TextMuted;
            }
        }

        private void DrawInfoBox(string message, Color textColor, Color bgColor)
        {
            var boxRect = EditorGUILayout.GetControlRect(GUILayout.Height(28));
            EditorGUI.DrawRect(boxRect, bgColor);
            DrawBorder(boxRect, new Color(textColor.r, textColor.g, textColor.b, 0.3f), 1);
            
            var labelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                normal = { textColor = textColor },
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 10, 0, 0)
            };
            GUI.Label(boxRect, "⚠ " + message, labelStyle);
        }

        private void SendTestFeedback()
        {
            Debug.Log($"[CLOUPT Feedback] Sending test {_testType} feedback...");
            StartCoroutine_Editor(SendTestFeedbackCoroutine());
        }

        private System.Collections.IEnumerator SendTestFeedbackCoroutine()
        {
            string url = "https://api.cloupt.com/api/v1/feedback";

            string deviceId = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(deviceId) || deviceId == "n/a")
            {
                deviceId = PlayerPrefs.GetString("CLOUPT_DeviceId", "");
                if (string.IsNullOrEmpty(deviceId))
                {
                    deviceId = System.Guid.NewGuid().ToString();
                    PlayerPrefs.SetString("CLOUPT_DeviceId", deviceId);
                    PlayerPrefs.Save();
                }
            }

            string typeStr = _testType.ToString().ToLower();
            int rating = _testType == FeedbackType.Feedback ? _testRating : 0;
            string priorityStr = _testPriority.ToString().ToLower();

            string json = $@"{{
                ""appId"": ""{EscapeJson(CLOUPTSettings.Instance.PublicAppId)}"",
                ""deviceId"": ""{EscapeJson(deviceId)}"",
                ""type"": ""{typeStr}"",
                ""header"": ""{EscapeJson(_testHeader)}"",
                ""message"": ""{EscapeJson(_testMessage)}"",
                ""rating"": {rating},
                ""priority"": ""{priorityStr}"",
                ""metadata"": {{
                    ""platform"": ""editor"",
                    ""os"": ""{EscapeJson(SystemInfo.operatingSystem)}"",
                    ""gameVersion"": ""{EscapeJson(Application.version)}"",
                    ""deviceModel"": ""{EscapeJson(SystemInfo.deviceModel)}"",
                    ""screenResolution"": ""{Screen.width}x{Screen.height}""
                }}
            }}";

            Debug.Log($"[CLOUPT Feedback] Request: {json}");

            using (var webRequest = new UnityEngine.Networking.UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                webRequest.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.timeout = 15;

                var operation = webRequest.SendWebRequest();

                while (!operation.isDone)
                    yield return null;

                string responseBody = webRequest.downloadHandler?.text ?? "";
                Debug.Log($"[CLOUPT Feedback] Response [{webRequest.responseCode}]: {responseBody}");

                if (webRequest.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var response = JsonUtility.FromJson<TestFeedbackResponse>(responseBody);
                        if (response.success)
                        {
                            Debug.Log($"[CLOUPT Feedback] ✓ Feedback sent! ID: {response.feedbackId}");
                            EditorUtility.DisplayDialog("Success", $"Feedback sent successfully!\n\nID: {response.feedbackId}", "OK");
                            _testMessage = "";
                            _testHeader = "";
                        }
                        else
                        {
                            Debug.LogError($"[CLOUPT Feedback] ✗ API Error: {response.error}");
                            EditorUtility.DisplayDialog("Error", $"API Error:\n{response.error}\n\nCode: {response.code}", "OK");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[CLOUPT Feedback] ✗ Parse error: {ex.Message}");
                        EditorUtility.DisplayDialog("Error", $"Failed to parse response:\n{responseBody}", "OK");
                    }
                }
                else
                {
                    string errorMsg = webRequest.error;
                    try
                    {
                        var errorResponse = JsonUtility.FromJson<TestFeedbackResponse>(responseBody);
                        if (!string.IsNullOrEmpty(errorResponse.error))
                        {
                            errorMsg = $"{errorResponse.error} ({errorResponse.code})";
                        }
                    }
                    catch { }

                    Debug.LogError($"[CLOUPT Feedback] ✗ Failed [{webRequest.responseCode}]: {errorMsg}");
                    EditorUtility.DisplayDialog("Error", $"Failed to send feedback:\n\n{errorMsg}", "OK");
                }
            }
        }

        private string EscapeJson(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        private void StartCoroutine_Editor(System.Collections.IEnumerator routine)
        {
            EditorApplication.CallbackFunction callback = null;
            callback = () =>
            {
                try
                {
                    if (!routine.MoveNext())
                    {
                        EditorApplication.update -= callback;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex);
                    EditorApplication.update -= callback;
                }
            };
            EditorApplication.update += callback;
        }

        [System.Serializable]
        private class TestFeedbackResponse
        {
            public bool success;
            public string feedbackId;
            public string error;
            public string code;
        }
    }
}
