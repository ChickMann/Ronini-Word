#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using EF.Generic;
using UnityEditor;
using UnityEngine;

namespace EF.Editor
{
    [InitializeOnLoad]
    public class EasyStartup : EditorWindow
    {
        public static readonly Version CURRENT_VERSION = new Version(2, 0, 0);
        private const string SETTINGS_EXIST_KEY = "settingExist";

        private static bool isSettingExist;

        // Dark theme colors
        private static readonly Color DarkBackground = new Color(0.2f, 0.2f, 0.2f, 1f);
        private static readonly Color DarkerBackground = new Color(0.15f, 0.15f, 0.15f, 1f);
        private static readonly Color AccentBlue = new Color(0.2f, 0.6f, 1f, 1f);
        private static readonly Color AccentBlueHover = new Color(0.3f, 0.7f, 1f, 1f);
        private static readonly Color TextPrimary = new Color(0.9f, 0.9f, 0.9f, 1f);
        private static readonly Color TextSecondary = new Color(0.7f, 0.7f, 0.7f, 1f);
        private static readonly Color BorderColor = new Color(0.4f, 0.4f, 0.4f, 1f);

        [MenuItem("Tools/Easy Firebase/Setup")]
        private static void CheckForSettingsOnStartup()
        {
            EasyStartup[] existingWindows = Resources.FindObjectsOfTypeAll<EasyStartup>();
            if (existingWindows.Length > 0)
            {
                existingWindows[0].Focus();
                return;
            }

            var window = GetWindow<EasyStartup>("Easy Firebase Setup");
            window.minSize = new Vector2(400, 350);
            window.maxSize = new Vector2(500, 400);
        }

        private static bool CheckForSettings()
        {
            if (EditorPrefs.GetInt(SETTINGS_EXIST_KEY) != 1)
            {
                return false;
            }
            return true;
        }

        private void OnValidate()
        {
            isSettingExist = CheckForSettings();
            if (isSettingExist == false)
            {
                CheckForSettingsOnStartup();
            }
        }

        private void OnGUI()
        {
            // Set background color
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), DarkBackground);

            GUILayout.BeginVertical();

            // Header section with gradient background - starts from top
            DrawHeader();

            GUILayout.Space(20);

            // Content section
            DrawContent();

            GUILayout.Space(20);

            // Action buttons section
            DrawActionButtons();

            GUILayout.EndVertical();
        }

        private void DrawHeader()
        {
            // Header background - full width from top
            Rect headerRect = GUILayoutUtility.GetRect(0, 90, GUILayout.ExpandWidth(true));
            headerRect.x = 0;
            headerRect.width = position.width;
            EditorGUI.DrawRect(headerRect, DarkerBackground);

            // Draw border
            EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.y + headerRect.height - 1, headerRect.width, 1), BorderColor);

            GUILayout.BeginArea(headerRect);
            GUILayout.Space(20);

            // Title with custom style
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = TextPrimary },
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            GUILayout.Label($"Easy Firebase v{CURRENT_VERSION}", titleStyle);

            // Subtitle
            GUIStyle subtitleStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = AccentBlue },
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Italic
            };

            GUILayout.Label("Setup & Configuration", subtitleStyle);

            GUILayout.EndArea();
        }

        private void DrawContent()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical();

            // Welcome message
            GUIStyle welcomeStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = TextPrimary },
                fontSize = 14,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter
            };

            GUILayout.Label("Thanks for importing CLOUPT's EF package.", welcomeStyle);
            GUILayout.Space(15);

            // Instructions with icons
            DrawInstructionItem("🔗", "Please add related SDK to your project", () => {
                Application.OpenURL("https://firebase.google.com/download/unity");
            });

            GUILayout.Space(8);
            DrawInstructionItem("⚙️", "Please create config file", null);

            GUILayout.Space(8);
            DrawInstructionItem("📋", "Last updated for Firebase 12.10.1 and Unity 6", null);

            GUILayout.Space(8);
            DrawInstructionItem("⭐", "Your review is invaluable to us", null);

            GUILayout.EndVertical();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
        }

        private void DrawInstructionItem(string icon, string text, System.Action onClick = null)
        {
            GUILayout.BeginHorizontal();

            // Icon
            GUIStyle iconStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = AccentBlue },
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = 25
            };
            GUILayout.Label(icon, iconStyle);

            // Text (clickable if action provided)
            GUIStyle textStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = onClick != null ? AccentBlue : TextSecondary },
                fontSize = 12,
                wordWrap = true
            };

            if (onClick != null)
            {
                if (GUILayout.Button(text, textStyle))
                {
                    onClick.Invoke();
                }
                EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            }
            else
            {
                GUILayout.Label(text, textStyle);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawActionButtons()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical();

            // Setup button
            if (DrawStyledButton("🚀 Setup Easy Firebase", AccentBlue, AccentBlueHover, 16))
            {
                CreateEFSettings();
                EditorPrefs.SetInt(SETTINGS_EXIST_KEY, 1);
                ShowNotification(new GUIContent("✅ Setup completed successfully!"));
            }

            GUILayout.Space(10);

            // Review button
            if (DrawStyledButton("⭐ Leave a Review", new Color(0.8f, 0.4f, 0.2f), new Color(0.9f, 0.5f, 0.3f), 14))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/network/easy-integration-tool-for-firebase-208355#reviews");
            }

            GUILayout.EndVertical();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            GUILayout.Space(20);
        }

        private bool DrawStyledButton(string text, Color normalColor, Color hoverColor, int fontSize)
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                normal = {
                    background = MakeTex(2, 2, normalColor),
                    textColor = Color.white
                },
                hover = {
                    background = MakeTex(2, 2, hoverColor),
                    textColor = Color.white
                },
                active = {
                    background = MakeTex(2, 2, normalColor * 0.8f),
                    textColor = Color.white
                },
                fontSize = fontSize,
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true,
                padding = new RectOffset(15, 15, 12, 12),
                border = new RectOffset(8, 8, 8, 8),
                fontStyle = FontStyle.Bold
            };

            // Add subtle shadow effect
            Rect buttonRect = GUILayoutUtility.GetRect(0, 45, GUILayout.ExpandWidth(true));
            Rect shadowRect = new Rect(buttonRect.x + 2, buttonRect.y + 2, buttonRect.width, buttonRect.height);
            EditorGUI.DrawRect(shadowRect, new Color(0, 0, 0, 0.3f));

            return GUI.Button(buttonRect, text, buttonStyle);
        }

        // Helper method to create texture for button background
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pix);
            texture.Apply();
            return texture;
        }

        private void CreateEFSettings()
        {
            string folderPath = "Assets/Resources/NarrowNetwork";
            string assetPathAndName = folderPath + "/EF-Settings.asset";

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "NarrowNetwork");
            }

            EFSettings asset = ScriptableObject.CreateInstance<EFSettings>();

            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            EFManager manager;
#if UNITY_6000_0_OR_NEWER
            manager = GameObject.FindAnyObjectByType<EFManager>();
#endif
#if UNITY_2020_1_OR_NEWER && !UNITY_6000_0_OR_NEWER
            manager = GameObject.FindObjectOfType<EFManager>();
#endif

            if (manager)
            {
                manager.Settings = asset;
            }
            else
            {
                new GameObject("Easy Firebase").AddComponent<EFManager>().Settings = asset;
            }
        }
    }
}

#endif