using UnityEngine;
using UnityEditor;
using EF.Generic;

namespace EF.Editor
{
    [CustomPropertyDrawer(typeof(SimpleAuth))]
    public class EF_SimpleAuthEditor : PropertyDrawer
    {
        private static readonly Color darkBackground = new Color(0.2f, 0.2f, 0.2f, 1f);
        private static readonly Color darkAccent = new Color(0.3f, 0.3f, 0.3f, 1f);
        private static readonly Color primaryColor = new Color(0.4f, 0.7f, 1f, 1f);
        private static readonly Color successColor = new Color(0.4f, 0.8f, 0.4f, 1f);
        private static readonly Color warningColor = new Color(1f, 0.7f, 0.3f, 1f);
        private static readonly Color errorColor = new Color(1f, 0.4f, 0.4f, 1f);

        private bool isExpanded = true;
        private bool showEmailSettings = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var authTypeProperty = property.FindPropertyRelative("authType");
            var signInWithEmailProperty = property.FindPropertyRelative("signInWithEmail");

            float yOffset = position.y;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            // Main Header with Dark Theme
            Rect headerRect = new Rect(position.x, yOffset, position.width, lineHeight + 8);
            DrawDarkBox(headerRect, darkBackground);

            // Foldout with custom styling
            Rect foldoutRect = new Rect(headerRect.x + 8, headerRect.y + 4, headerRect.width - 16, lineHeight);
            var originalColor = GUI.color;
            GUI.color = primaryColor;
            isExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, new GUIContent("🔐 Firebase Authentication", "Configure Firebase authentication settings"), true, GetFoldoutStyle());
            GUI.color = originalColor;

            yOffset += headerRect.height + spacing;

            if (isExpanded)
            {
                // Content Background
                float contentHeight = CalculateContentHeight(authTypeProperty, signInWithEmailProperty);
                Rect contentRect = new Rect(position.x, yOffset, position.width, contentHeight);
                DrawDarkBox(contentRect, darkAccent);

                // Indent content
                float indentX = position.x + 12;
                float contentWidth = position.width - 24;
                yOffset += 8;

                // Auth Type Selection with Icons
                Rect authTypeRect = new Rect(indentX, yOffset, contentWidth, lineHeight);
                DrawPropertyWithIcon(authTypeRect, authTypeProperty, "🔑", "Authentication Type");
                yOffset += lineHeight + spacing * 2;

                // Auth Type specific settings
                var authType = (SimpleAuth.AuthType)authTypeProperty.enumValueIndex;

                switch (authType)
                {
                    case SimpleAuth.AuthType.EmailSignIn:
                        yOffset = DrawEmailAuthSettings(indentX, yOffset, contentWidth, signInWithEmailProperty);
                        break;
                    case SimpleAuth.AuthType.EmailSignUp:
                        yOffset = DrawEmailSignUpAuthSettings(indentX, yOffset, contentWidth, signInWithEmailProperty);
                        break;

                    case SimpleAuth.AuthType.Google:
                        yOffset = DrawGoogleAuthSettings(indentX, yOffset, contentWidth);
                        break;

                    case SimpleAuth.AuthType.Anonymous:
                        yOffset = DrawAnonymousAuthSettings(indentX, yOffset, contentWidth);
                        break;
                }

                // Status and Actions
                yOffset += spacing;
                DrawStatusSection(indentX, yOffset, contentWidth);
            }

            EditorGUI.EndProperty();
        }

        private float DrawEmailAuthSettings(float x, float y, float width, SerializedProperty signInWithEmailProperty)
        {
            GoogleSignInDefineSetter.DisableGoogleSignInDefine();
            // Email Settings Header
            Rect emailHeaderRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
            var originalColor = GUI.color;
            GUI.color = successColor;
            showEmailSettings = EditorGUI.Foldout(emailHeaderRect, showEmailSettings, new GUIContent("📧 Email Authentication Settings"), true, GetSubFoldoutStyle());
            GUI.color = originalColor;
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (showEmailSettings)
            {
                EditorGUI.indentLevel++;

                var useRepeatEmailProp = signInWithEmailProperty.FindPropertyRelative("useRepeatEmail");
                var emailInputFieldProp = signInWithEmailProperty.FindPropertyRelative("emailInputField");
                var repeatEmailInputFieldProp = signInWithEmailProperty.FindPropertyRelative("repeatEmailInputField");
                var passwordInputFieldProp = signInWithEmailProperty.FindPropertyRelative("passwordInputField");


                // Email Input Field
                Rect emailRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
                DrawPropertyWithIcon(emailRect, emailInputFieldProp, "✉️", "Email Input Field");
                y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;


                // Password Input Field
                Rect passwordRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
                DrawPropertyWithIcon(passwordRect, passwordInputFieldProp, "🔒", "Password Input Field");
                y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.indentLevel--;

                // Validation warnings
                y = DrawValidationWarnings(x, y, width, emailInputFieldProp, passwordInputFieldProp, useRepeatEmailProp.boolValue ? repeatEmailInputFieldProp : null);
            }

            return y;
        }       
        
        private float DrawEmailSignUpAuthSettings(float x, float y, float width, SerializedProperty signInWithEmailProperty)
        {
            GoogleSignInDefineSetter.DisableGoogleSignInDefine();
            // Email Settings Header
            Rect emailHeaderRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
            var originalColor = GUI.color;
            GUI.color = successColor;
            showEmailSettings = EditorGUI.Foldout(emailHeaderRect, showEmailSettings, new GUIContent("📧 Email Authentication Settings"), true, GetSubFoldoutStyle());
            GUI.color = originalColor;
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (showEmailSettings)
            {
                EditorGUI.indentLevel++;

                var useRepeatEmailProp = signInWithEmailProperty.FindPropertyRelative("useRepeatEmail");
                var emailInputFieldProp = signInWithEmailProperty.FindPropertyRelative("emailInputField");
                var repeatEmailInputFieldProp = signInWithEmailProperty.FindPropertyRelative("repeatEmailInputField");
                var passwordInputFieldProp = signInWithEmailProperty.FindPropertyRelative("passwordInputField");

                // Use Repeat Email toggle
                Rect toggleRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
                DrawPropertyWithIcon(toggleRect, useRepeatEmailProp, "🔄", "Use Email Confirmation");
                y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // Email Input Field
                Rect emailRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
                DrawPropertyWithIcon(emailRect, emailInputFieldProp, "✉️", "Email Input Field");
                y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // Repeat Email Input Field (only if enabled)
                if (useRepeatEmailProp.boolValue)
                {
                    Rect repeatEmailRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
                    DrawPropertyWithIcon(repeatEmailRect, repeatEmailInputFieldProp, "✉️", "Repeat Email Input Field");
                    y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                // Password Input Field
                Rect passwordRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
                DrawPropertyWithIcon(passwordRect, passwordInputFieldProp, "🔒", "Password Input Field");
                y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.indentLevel--;

                // Validation warnings
                y = DrawValidationWarnings(x, y, width, emailInputFieldProp, passwordInputFieldProp, useRepeatEmailProp.boolValue ? repeatEmailInputFieldProp : null);
            }

            return y;
        }

        private float DrawGoogleAuthSettings(float x, float y, float width)
        {
            GoogleSignInDefineSetter.EnableGoogleSignInDefine();
            Rect infoRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight * 2);
            DrawInfoBox(infoRect, "🔍 Google Sign-In",
                "You should install the Google Sign-In plugin for Unity. Click here",
                successColor);

            EditorGUIUtility.AddCursorRect(infoRect, MouseCursor.Link);

            if (infoRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp)
            {
                Application.OpenURL("https://github.com/googlesamples/google-signin-unity/releases/tag/v1.0.4");
            }
            return y + infoRect.height + EditorGUIUtility.standardVerticalSpacing;
        }

        private float DrawAnonymousAuthSettings(float x, float y, float width)
        {
            GoogleSignInDefineSetter.DisableGoogleSignInDefine();
            Rect infoRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight * 2);
            DrawInfoBox(infoRect, "👤 Anonymous Sign-In", "Users will be authenticated anonymously without credentials.", warningColor);
            return y + infoRect.height + EditorGUIUtility.standardVerticalSpacing;
        }

        private float DrawValidationWarnings(float x, float y, float width, SerializedProperty emailField, SerializedProperty passwordField, SerializedProperty repeatEmailField = null)
        {

#pragma warning disable CS0219
            bool hasWarnings = false;
#pragma warning restore CS0219

            if (emailField.objectReferenceValue == null)
            {
                Rect warningRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
                DrawWarningBox(warningRect, "⚠️ Email input field is not assigned!");
                y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                hasWarnings = true;
            }

            if (passwordField.objectReferenceValue == null)
            {
                Rect warningRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
                DrawWarningBox(warningRect, "⚠️ Password input field is not assigned!");
                y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                hasWarnings = true;
            }

            if (repeatEmailField != null && repeatEmailField.objectReferenceValue == null)
            {
                Rect warningRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
                DrawWarningBox(warningRect, "⚠️ Repeat email input field is not assigned!");
                y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                hasWarnings = true;
            }

            return y;
        }

        private void DrawStatusSection(float x, float y, float width)
        {
            Rect statusRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight * 2);
            bool isConfigured = false;
#if UNITY_6000_0_OR_NEWER
            isConfigured = GameObject.FindAnyObjectByType<EFManager>()?.Settings;
#endif
#if UNITY_2020_1_OR_NEWER && !UNITY_6000_0_OR_NEWER
            // For Unity versions 2020.1 and higher, we can use FindObjectOfType
            // to check if EFManager is configured.
           isConfigured = GameObject.FindObjectOfType<EFManager>()?.Settings != null;
#endif
            if (isConfigured)
            {
                DrawInfoBox(statusRect, "✅ Ready", "Firebase Authentication is configured and ready to use.", successColor);
            }
            else
            {
                DrawInfoBox(statusRect, "❌ Not Configured", "Firebase Authentication needs to be configured in your project or use Unity 2020_1 or higher.", errorColor);
            }
        }

        private void DrawPropertyWithIcon(Rect rect, SerializedProperty property, string icon, string tooltip)
        {
            // Icon
            Rect iconRect = new Rect(rect.x, rect.y, 20, rect.height);
            var iconStyle = new GUIStyle(GUI.skin.label);
            iconStyle.normal.textColor = primaryColor;
            iconStyle.fontSize = 12;
            iconStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(iconRect, icon, iconStyle);

            // Property
            Rect propRect = new Rect(rect.x + 22, rect.y, rect.width - 22, rect.height);
            EditorGUI.PropertyField(propRect, property, new GUIContent(tooltip));
        }

        private void DrawDarkBox(Rect rect, Color color)
        {
            var originalColor = GUI.color;
            GUI.color = color;
            GUI.Box(rect, "", GetDarkBoxStyle());
            GUI.color = originalColor;
        }

        private void DrawInfoBox(Rect rect, string title, string message, Color accentColor)
        {
            // Background
            DrawDarkBox(rect, new Color(accentColor.r * 0.2f, accentColor.g * 0.2f, accentColor.b * 0.2f, 0.5f));

            // Title
            Rect titleRect = new Rect(rect.x + 8, rect.y + 2, rect.width - 16, EditorGUIUtility.singleLineHeight);
            var titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.normal.textColor = accentColor;
            titleStyle.fontSize = 11;
            GUI.Label(titleRect, title, titleStyle);

            // Message
            Rect messageRect = new Rect(rect.x + 8, rect.y + EditorGUIUtility.singleLineHeight, rect.width - 16, EditorGUIUtility.singleLineHeight);
            var messageStyle = new GUIStyle(EditorStyles.label);
            messageStyle.normal.textColor = Color.white * 0.8f;
            messageStyle.fontSize = 10;
            messageStyle.wordWrap = true;
            GUI.Label(messageRect, message, messageStyle);
        }

        private void DrawWarningBox(Rect rect, string message)
        {
            DrawDarkBox(rect, new Color(warningColor.r * 0.2f, warningColor.g * 0.2f, warningColor.b * 0.2f, 0.5f));

            Rect labelRect = new Rect(rect.x + 8, rect.y + 2, rect.width - 16, rect.height - 4);
            var warningStyle = new GUIStyle(EditorStyles.label);
            warningStyle.normal.textColor = warningColor;
            warningStyle.fontSize = 10;
            warningStyle.alignment = TextAnchor.MiddleLeft;
            GUI.Label(labelRect, message, warningStyle);
        }

        private float CalculateContentHeight(SerializedProperty authTypeProperty, SerializedProperty signInWithEmailProperty)
        {
            float height = EditorGUIUtility.singleLineHeight * 3; // Base height

            var authType = (SimpleAuth.AuthType)authTypeProperty.enumValueIndex;

            switch (authType)
            {
                case SimpleAuth.AuthType.EmailSignIn:
                    height += EditorGUIUtility.singleLineHeight * 4; // Base email fields
                    if (showEmailSettings)
                    {
                        var useRepeatEmailProp = signInWithEmailProperty.FindPropertyRelative("useRepeatEmail");
                        if (useRepeatEmailProp != null && useRepeatEmailProp.boolValue)
                        {
                            height += EditorGUIUtility.singleLineHeight; // Repeat email field
                        }
                        height += EditorGUIUtility.singleLineHeight * 2; // Validation warnings space
                    }
                    break;
                case SimpleAuth.AuthType.EmailSignUp:
                    height += EditorGUIUtility.singleLineHeight * 4; // Base email fields
                    if (showEmailSettings)
                    {
                        var useRepeatEmailProp = signInWithEmailProperty.FindPropertyRelative("useRepeatEmail");
                        if (useRepeatEmailProp != null && useRepeatEmailProp.boolValue)
                        {
                            height += EditorGUIUtility.singleLineHeight; // Repeat email field
                        }
                        height += EditorGUIUtility.singleLineHeight * 2; // Validation warnings space
                    }
                    break;

                case SimpleAuth.AuthType.Google:
                case SimpleAuth.AuthType.Anonymous:
                    height += EditorGUIUtility.singleLineHeight * 2; // Info box
                    break;
            }

            height += EditorGUIUtility.singleLineHeight * 3; // Status section
            height += EditorGUIUtility.standardVerticalSpacing * 8; // Spacing

            return height;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!isExpanded)
                return EditorGUIUtility.singleLineHeight + 8 + EditorGUIUtility.standardVerticalSpacing;

            var authTypeProperty = property.FindPropertyRelative("authType");
            var signInWithEmailProperty = property.FindPropertyRelative("signInWithEmail");

            return EditorGUIUtility.singleLineHeight + 8 + EditorGUIUtility.standardVerticalSpacing +
                   CalculateContentHeight(authTypeProperty, signInWithEmailProperty) + 16;
        }

        #region Styles
        private GUIStyle GetDarkBoxStyle()
        {
            var style = new GUIStyle(GUI.skin.box);
            style.normal.background = MakeTex(2, 2, Color.clear);
            return style;
        }

        private GUIStyle GetFoldoutStyle()
        {
            var style = new GUIStyle(EditorStyles.foldout);
            style.normal.textColor = Color.white;
            style.onNormal.textColor = Color.white;
            style.hover.textColor = primaryColor;
            style.onHover.textColor = primaryColor;
            style.focused.textColor = primaryColor;
            style.onFocused.textColor = primaryColor;
            style.active.textColor = primaryColor;
            style.onActive.textColor = primaryColor;
            style.fontStyle = FontStyle.Bold;
            return style;
        }

        private GUIStyle GetSubFoldoutStyle()
        {
            var style = new GUIStyle(EditorStyles.foldout);
            style.normal.textColor = Color.white * 0.9f;
            style.onNormal.textColor = Color.white * 0.9f;
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 11;
            return style;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
        #endregion
    }
}