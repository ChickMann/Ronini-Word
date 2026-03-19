#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System;
using Commersion.ScriptableObjects;

namespace Commersion.EditorScripts
{
    [Serializable]
public class CommersionLegal
{
    [SerializeField] public TextAsset TOSText;
    [SerializeField] public TextAsset PrivacyText;
}

[CustomEditor(typeof(CommersionSettings))]
public class CommersionSettingsEditor : Editor
{
    private CommersionSettings settings;
    private SerializedProperty saveLegalsLocallyProp;
    private SerializedProperty preLoaderSettingsProp;
    private SerializedProperty legalsProp;
    
    // Conditions Properties
    private SerializedProperty useTimerProp;
    private SerializedProperty timerProp;
    private SerializedProperty sceneProp;
    
    // PreLoader Settings Properties
    private SerializedProperty useCopyRightTextProp;
    private SerializedProperty useLegalDisclaimerProp;
    private SerializedProperty backgroundColorProp;
    private SerializedProperty textColorProp;
    private SerializedProperty copyrightTextProp;
    private SerializedProperty legalDisclaimerProp;
    private SerializedProperty logoProp;
    private SerializedProperty logoHeightProp;
    private SerializedProperty logoWidthProp;
    
    // Legal Documents Properties
    private SerializedProperty tosTextProp;
    private SerializedProperty privacyTextProp;
    
    // Dark theme colors
    private static readonly Color darkBackground = new Color(0.22f, 0.22f, 0.22f, 1f);
    private static readonly Color darkHeaderBackground = new Color(0.18f, 0.18f, 0.18f, 1f);
    private static readonly Color accentColor = new Color(0.3f, 0.7f, 1f, 1f);
    private static readonly Color successColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    private static readonly Color warningColor = new Color(1f, 0.7f, 0.2f, 1f);
    private static readonly Color textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    private static readonly Color separatorColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    
    // GUI Styles - will be initialized in OnInspectorGUI
    private GUIStyle headerStyle;
    private GUIStyle sectionStyle;
    private GUIStyle labelStyle;
    private GUIStyle toggleStyle;
    private GUIStyle boxStyle;
    private GUIStyle helpBoxStyle;
    private GUIStyle textAreaStyle;
    
    private Texture2D backgroundTex;
    private Texture2D headerBackgroundTex;
    
    // Foldout states
    private bool conditionsFoldout = true;
    private bool legalSettingsFoldout = true;
    private bool legalDocumentsFoldout = true;
    private bool preLoaderSettingsFoldout = true;
    private bool preLoaderTextsFoldout = true;
    private bool preLoaderLogoFoldout = true;
    
    private void OnEnable()
    {
        settings = (CommersionSettings)target;
        
        // Main properties
        saveLegalsLocallyProp = serializedObject.FindProperty("saveLegalsLocally");
        preLoaderSettingsProp = serializedObject.FindProperty("preLoaderSettings");
        legalsProp = serializedObject.FindProperty("legals");
        
        // Conditions properties
        useTimerProp = preLoaderSettingsProp.FindPropertyRelative("useTimer");
        timerProp = preLoaderSettingsProp.FindPropertyRelative("timer");
        sceneProp = preLoaderSettingsProp.FindPropertyRelative("nextScene");
        
        // PreLoader Settings properties
        useCopyRightTextProp = preLoaderSettingsProp.FindPropertyRelative("useCopyRightText");
        useLegalDisclaimerProp = preLoaderSettingsProp.FindPropertyRelative("useLegalDisclaimer");
        backgroundColorProp = preLoaderSettingsProp.FindPropertyRelative("_backgroundColor");
        textColorProp = preLoaderSettingsProp.FindPropertyRelative("_textColor");
        copyrightTextProp = preLoaderSettingsProp.FindPropertyRelative("copyrightText");
        legalDisclaimerProp = preLoaderSettingsProp.FindPropertyRelative("legalDisclaimer");
        logoProp = preLoaderSettingsProp.FindPropertyRelative("logo");
        logoHeightProp = preLoaderSettingsProp.FindPropertyRelative("logoHeight");
        logoWidthProp = preLoaderSettingsProp.FindPropertyRelative("logoWidth");
        
        // Legal Documents properties
        if (legalsProp != null)
        {
            tosTextProp = legalsProp.FindPropertyRelative("TOSText");
            privacyTextProp = legalsProp.FindPropertyRelative("PrivacyText");
        }
    }
    
    private void OnDisable()
    {
        // Clean up textures
        if (backgroundTex != null) DestroyImmediate(backgroundTex);
        if (headerBackgroundTex != null) DestroyImmediate(headerBackgroundTex);
    }
    
    private void InitializeStyles()
    {
        if (headerStyle != null) return; // Already initialized
        
        // Create textures
        backgroundTex = MakeTex(2, 2, darkBackground);
        headerBackgroundTex = MakeTex(2, 2, darkHeaderBackground);
        
        // Header Style
        headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 16;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.normal.textColor = textColor;
        headerStyle.margin = new RectOffset(0, 0, 10, 15);
        headerStyle.wordWrap = true;
        
        // Section Style
        sectionStyle = new GUIStyle(EditorStyles.foldout);
        sectionStyle.fontSize = 13;
        sectionStyle.fontStyle = FontStyle.Bold;
        sectionStyle.normal.textColor = accentColor;
        sectionStyle.onNormal.textColor = accentColor;
        sectionStyle.hover.textColor = accentColor;
        sectionStyle.onHover.textColor = accentColor;
        sectionStyle.focused.textColor = accentColor;
        sectionStyle.onFocused.textColor = accentColor;
        sectionStyle.active.textColor = accentColor;
        sectionStyle.onActive.textColor = accentColor;
        sectionStyle.margin = new RectOffset(0, 0, 5, 5);
        sectionStyle.padding = new RectOffset(15, 0, 2, 2);
        sectionStyle.wordWrap = true;
        
        // Label Style
        labelStyle = new GUIStyle(EditorStyles.label);
        labelStyle.normal.textColor = textColor;
        labelStyle.fontStyle = FontStyle.Normal;
        
        // Toggle Style
        toggleStyle = new GUIStyle(EditorStyles.toggle);
        toggleStyle.normal.textColor = textColor;
        toggleStyle.onNormal.textColor = successColor;
        
        // Box Style
        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = headerBackgroundTex;
        boxStyle.normal.textColor = textColor;
        boxStyle.border = new RectOffset(1, 1, 1, 1);
        boxStyle.padding = new RectOffset(15, 15, 10, 10);
        boxStyle.margin = new RectOffset(5, 5, 5, 5);
        
        // Help Box Style
        helpBoxStyle = new GUIStyle(EditorStyles.helpBox);
        helpBoxStyle.normal.background = MakeTex(2, 2, new Color(0.15f, 0.15f, 0.25f, 0.8f));
        helpBoxStyle.normal.textColor = new Color(0.8f, 0.8f, 1f, 1f);
        helpBoxStyle.padding = new RectOffset(12, 12, 8, 8);
        helpBoxStyle.margin = new RectOffset(5, 5, 10, 5);
        
        // Text Area Style
        textAreaStyle = new GUIStyle(EditorStyles.textArea);
        textAreaStyle.normal.textColor = textColor;
        textAreaStyle.normal.background = MakeTex(2, 2, new Color(0.15f, 0.15f, 0.15f, 1f));
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Initialize styles in OnInspectorGUI where GUI calls are allowed
        InitializeStyles();
        
        // Custom background
        var rect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true));
        rect.height = 1100; // Increased height for new section
        rect.y -= 10;
        EditorGUI.DrawRect(rect, darkBackground);
        
        GUILayout.Space(20);
        
        // Header
        DrawHeader();
        
        GUILayout.Space(10);
        
        // Conditions Section
        DrawConditions();
        
        GUILayout.Space(10);
        
        // Legal Settings Section
        DrawLegalSettings();
        
        GUILayout.Space(10);
        
        // Legal Documents Section
        DrawLegalDocuments();
        
        GUILayout.Space(10);
        
        // PreLoader Settings Section
        DrawPreLoaderSettings();
        
        GUILayout.Space(10);
        
        // Info Section
        DrawInfoSection();
        
        GUILayout.Space(20);
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawHeader()
    {
        EditorGUILayout.BeginVertical(boxStyle);
        
        // Title with icon
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("⚙️", GUILayout.Width(30), GUILayout.Height(20));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.Label("Commersion Settings", headerStyle, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        // Separator line
        DrawSeparator();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawConditions()
    {
        EditorGUILayout.BeginVertical(boxStyle);
        
        // Foldout for Conditions
        EditorGUILayout.Space(5);
        conditionsFoldout = EditorGUILayout.Foldout(conditionsFoldout, "⏱️ Conditions", true, sectionStyle);
        
        if (conditionsFoldout)
        {
            GUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();
            
            // Use Timer Toggle
            EditorGUILayout.BeginHorizontal();
            var newUseTimerValue = EditorGUILayout.Toggle(useTimerProp.boolValue, toggleStyle, GUILayout.Width(20));
            if (newUseTimerValue != useTimerProp.boolValue)
            {
                useTimerProp.boolValue = newUseTimerValue;
            }
            
            GUILayout.Label("Use Timer", labelStyle);
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            // Timer Field (only show if useTimer is enabled)
            if (useTimerProp.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20); // Indent for sub-option
                GUILayout.Label("Timer Duration:", labelStyle, GUILayout.Width(100));

                
                var newTimerValue = EditorGUILayout.FloatField(timerProp.floatValue, GUILayout.Width(80));
                if (newTimerValue != timerProp.floatValue)
                {
                    timerProp.floatValue = Mathf.Max(0f, newTimerValue); // Ensure non-negative
                }
                
                GUILayout.Label("seconds", labelStyle);
                EditorGUILayout.EndHorizontal();
                
                // Help text for timer
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(40);
                var oldColor = GUI.contentColor;
                GUI.contentColor = successColor;
                GUILayout.Label($"✅ Timer set to {timerProp.floatValue:F1} seconds", EditorStyles.miniLabel);
                GUI.contentColor = oldColor;
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20); // Indent for sub-option
                GUILayout.Label("Next Scene:", labelStyle, GUILayout.Width(100));
                var newSceneValue = EditorGUILayout.IntField(sceneProp.intValue, GUILayout.Width(80));
                if (newSceneValue != sceneProp.intValue)
                {
                    sceneProp.intValue = newSceneValue; // Ensure non-negative
                }
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(40);
                GUI.contentColor = successColor;
                if (SceneManager.GetSceneByBuildIndex(sceneProp.intValue) != null)
                {
                    string nextScene = SceneManager.GetSceneByBuildIndex(sceneProp.intValue).name;
                    GUILayout.Label($"✅ Scene {nextScene}", EditorStyles.miniLabel);
                }

                GUI.contentColor = oldColor;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                // Help text when timer is disabled
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(30);
                var oldColor = GUI.contentColor;
                GUI.contentColor = warningColor;
                GUILayout.Label("❌ Timer is disabled", EditorStyles.miniLabel);
                GUI.contentColor = oldColor;
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawLegalSettings()
    {
        EditorGUILayout.BeginVertical(boxStyle);
        
        // Foldout for Legal Settings
        EditorGUILayout.Space(5);
        legalSettingsFoldout = EditorGUILayout.Foldout(legalSettingsFoldout, "📋 Legal Settings", true, sectionStyle);
        
        if (legalSettingsFoldout)
        {
            GUILayout.Space(5);
            
            // Save Legals Locally Toggle
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.BeginHorizontal();
            var newValue = EditorGUILayout.Toggle(saveLegalsLocallyProp.boolValue, toggleStyle, GUILayout.Width(20));
            if (newValue != saveLegalsLocallyProp.boolValue)
            {
                saveLegalsLocallyProp.boolValue = newValue;
            }
            
            GUILayout.Label("Save Legal Terms Acceptance Locally", labelStyle);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            // Help text
            var helpText = saveLegalsLocallyProp.boolValue 
                ? "✅ Legal acceptance will be saved using PlayerPrefs" 
                : "❌ Legal acceptance will not be saved locally";
                
            var helpColor = saveLegalsLocallyProp.boolValue ? successColor : warningColor;
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(30);
            var oldColor = GUI.contentColor;
            GUI.contentColor = helpColor;
            GUILayout.Label(helpText, EditorStyles.miniLabel);
            GUI.contentColor = oldColor;
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawLegalDocuments()
    {
        EditorGUILayout.BeginVertical(boxStyle);
        
        // Foldout for Legal Documents
        EditorGUILayout.Space(5);
        legalDocumentsFoldout = EditorGUILayout.Foldout(legalDocumentsFoldout, "📄 Legal Documents", true, sectionStyle);
        
        if (legalDocumentsFoldout)
        {
            GUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();
            
            if (legalsProp != null)
            {
                // Terms of Service
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Terms of Service:", labelStyle, GUILayout.Width(130));
                EditorGUILayout.PropertyField(tosTextProp, GUIContent.none);
                EditorGUILayout.EndHorizontal();
                
                // Status indicator for TOS
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                var oldColor = GUI.contentColor;
                if (tosTextProp != null && tosTextProp.objectReferenceValue != null)
                {
                    GUI.contentColor = successColor;
                    GUILayout.Label("✅ TOS document assigned", EditorStyles.miniLabel);
                }
                else
                {
                    GUI.contentColor = warningColor;
                    GUILayout.Label("❌ No TOS document assigned", EditorStyles.miniLabel);
                }
                GUI.contentColor = oldColor;
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(10);
                
                // Privacy Policy
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Privacy Policy:", labelStyle, GUILayout.Width(130));
                EditorGUILayout.PropertyField(privacyTextProp, GUIContent.none);
                EditorGUILayout.EndHorizontal();
                
                // Status indicator for Privacy Policy
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                if (privacyTextProp != null && privacyTextProp.objectReferenceValue != null)
                {
                    GUI.contentColor = successColor;
                    GUILayout.Label("✅ Privacy Policy document assigned", EditorStyles.miniLabel);
                }
                else
                {
                    GUI.contentColor = warningColor;
                    GUILayout.Label("❌ No Privacy Policy document assigned", EditorStyles.miniLabel);
                }
                GUI.contentColor = oldColor;
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                // Show warning if legals property is not found
                var oldColor = GUI.contentColor;
                GUI.contentColor = warningColor;
                GUILayout.Label("❌ Legal documents property not found. Make sure 'legals' field exists in CommersionSettings.", EditorStyles.wordWrappedMiniLabel);
                GUI.contentColor = oldColor;
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawPreLoaderSettings()
    {
        EditorGUILayout.BeginVertical(boxStyle);
        
        // Foldout for PreLoader Settings
        EditorGUILayout.Space(5);
        preLoaderSettingsFoldout = EditorGUILayout.Foldout(preLoaderSettingsFoldout, "🚀 PreLoader Settings", true, sectionStyle);
        
        if (preLoaderSettingsFoldout)
        {
            GUILayout.Space(10);
            
            // General Settings
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();
            
            // Background Color
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Background Color:", labelStyle, GUILayout.Width(120));
            EditorGUILayout.PropertyField(backgroundColorProp, GUIContent.none);
            EditorGUILayout.EndHorizontal();
            
            // Text color
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Text color:", labelStyle, GUILayout.Width(120));
            EditorGUILayout.PropertyField(textColorProp, GUIContent.none);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            
            // Use Copyright Text Toggle
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(useCopyRightTextProp, GUIContent.none, GUILayout.Width(20));
            GUILayout.Label("Use Copyright Text", labelStyle);
            EditorGUILayout.EndHorizontal();
            
            // Use Legal Disclaimer Toggle
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(useLegalDisclaimerProp, GUIContent.none, GUILayout.Width(20));
            GUILayout.Label("Use Legal Disclaimer", labelStyle);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            DrawSeparator();
            
            // Text Settings Foldout
            EditorGUILayout.Space(5);
            preLoaderTextsFoldout = EditorGUILayout.Foldout(preLoaderTextsFoldout, "📝 Text Settings", true, sectionStyle);
            
            if (preLoaderTextsFoldout)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();
                
                // Copyright Text
                GUILayout.Label("Copyright Text:", labelStyle);
                if (string.IsNullOrEmpty(copyrightTextProp.stringValue))
                {
                    copyrightTextProp.stringValue = "© {YEAR} {COMPANY}. All rights reserved.";
                }
                copyrightTextProp.stringValue = EditorGUILayout.TextArea(copyrightTextProp.stringValue, textAreaStyle, GUILayout.Height(40));
                
                GUILayout.Space(5);
                
                // Legal Disclaimer
                GUILayout.Label("Legal Disclaimer:", labelStyle);
                if (string.IsNullOrEmpty(legalDisclaimerProp.stringValue))
                {
                    legalDisclaimerProp.stringValue = "All trademarks, logos, and brand names are the property of {COMPANY} or their respective owners.\nUnauthorized use, reproduction, or distribution of any content is strictly prohibited.";
                }
                legalDisclaimerProp.stringValue = EditorGUILayout.TextArea(legalDisclaimerProp.stringValue, textAreaStyle, GUILayout.Height(60));
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            
            GUILayout.Space(10);
            DrawSeparator();
            
            // Logo Settings Foldout
            EditorGUILayout.Space(5);
            preLoaderLogoFoldout = EditorGUILayout.Foldout(preLoaderLogoFoldout, "🖼️ Logo Settings", true, sectionStyle);
            
            if (preLoaderLogoFoldout)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();
                
                // Logo Sprite
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Logo:", labelStyle, GUILayout.Width(80));
                EditorGUILayout.PropertyField(logoProp, GUIContent.none);
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                
                // Logo Dimensions
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Width:", labelStyle, GUILayout.Width(80));
                EditorGUILayout.PropertyField(logoWidthProp, GUIContent.none, GUILayout.Width(80));
                GUILayout.Space(20);
                GUILayout.Label("Height:", labelStyle, GUILayout.Width(80));
                EditorGUILayout.PropertyField(logoHeightProp, GUIContent.none, GUILayout.Width(80));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawInfoSection()
    {
        EditorGUILayout.BeginVertical(helpBoxStyle);
        
        GUILayout.Label("ℹ️ Information", sectionStyle);
        
        var infoText = "CommersionSettings manages user preferences, legal compliance settings, timer conditions, and preloader configuration. " +
                      "Configure legal terms handling, timer-based conditions, legal document references (TOS/Privacy Policy), and customize your application's preloader appearance with logo, colors, and legal texts.";
        
        EditorGUILayout.LabelField(infoText, EditorStyles.wordWrappedLabel);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawSeparator()
    {
        GUILayout.Space(5);
        var rect = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, separatorColor);
        GUILayout.Space(5);
    }
    
    private Texture2D MakeTex(int width, int height, Color color)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = color;
            
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
}
#endif