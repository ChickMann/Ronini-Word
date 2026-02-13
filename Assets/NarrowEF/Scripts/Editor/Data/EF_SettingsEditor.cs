using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EF.Generic;

namespace EF.Editor
{
    [CustomEditor(typeof(EFSettings))]
    public class EF_SettingsEditor : UnityEditor.Editor
    {
        private GUIStyle darkBoxStyle;
        private GUIStyle darkHeaderStyle;
        private GUIStyle darkButtonStyle;
        private GUIStyle darkRemoveButtonStyle;
        private GUIStyle darkLabelStyle;
        private GUIStyle darkTextFieldStyle;
        private GUIStyle infoTextStyle;

        private void InitializeStyles()
        {
            if (darkBoxStyle == null)
            {
                darkBoxStyle = new GUIStyle(GUI.skin.box);
                darkBoxStyle.normal.background = MakeTex(2, 2, new Color(0.2f, 0.2f, 0.2f, 1f));
                darkBoxStyle.border = new RectOffset(3, 3, 3, 3);
                darkBoxStyle.margin = new RectOffset(4, 4, 4, 4);
                darkBoxStyle.padding = new RectOffset(8, 8, 8, 8);

                darkHeaderStyle = new GUIStyle(EditorStyles.boldLabel);
                darkHeaderStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                darkHeaderStyle.fontSize = 14;
                darkHeaderStyle.fontStyle = FontStyle.Bold;

                darkButtonStyle = new GUIStyle(GUI.skin.button);
                darkButtonStyle.normal.background = MakeTex(2, 2, new Color(0.3f, 0.5f, 0.7f, 1f));
                darkButtonStyle.hover.background = MakeTex(2, 2, new Color(0.4f, 0.6f, 0.8f, 1f));
                darkButtonStyle.active.background = MakeTex(2, 2, new Color(0.2f, 0.4f, 0.6f, 1f));
                darkButtonStyle.normal.textColor = Color.white;
                darkButtonStyle.hover.textColor = Color.white;
                darkButtonStyle.active.textColor = Color.white;
                darkButtonStyle.fontStyle = FontStyle.Bold;

                darkRemoveButtonStyle = new GUIStyle(GUI.skin.button);
                darkRemoveButtonStyle.normal.background = MakeTex(2, 2, new Color(0.7f, 0.2f, 0.2f, 1f));
                darkRemoveButtonStyle.hover.background = MakeTex(2, 2, new Color(0.8f, 0.3f, 0.3f, 1f));
                darkRemoveButtonStyle.active.background = MakeTex(2, 2, new Color(0.6f, 0.1f, 0.1f, 1f));
                darkRemoveButtonStyle.normal.textColor = Color.white;
                darkRemoveButtonStyle.hover.textColor = Color.white;
                darkRemoveButtonStyle.active.textColor = Color.white;
                darkRemoveButtonStyle.fontStyle = FontStyle.Bold;

                darkLabelStyle = new GUIStyle(GUI.skin.label);
                darkLabelStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                darkLabelStyle.fontStyle = FontStyle.Normal;

                darkTextFieldStyle = new GUIStyle(GUI.skin.textField);
                darkTextFieldStyle.normal.background = MakeTex(2, 2, Color.white);
                darkTextFieldStyle.focused.background = MakeTex(2, 2, new Color(0.95f, 0.95f, 0.95f, 1f));
                darkTextFieldStyle.normal.textColor = Color.white;
                darkTextFieldStyle.focused.textColor = Color.white;
                darkTextFieldStyle.border = new RectOffset(3, 3, 3, 3);

                infoTextStyle = new GUIStyle(GUI.skin.label);
                infoTextStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f, 1f);
                infoTextStyle.fontSize = 11;
                infoTextStyle.fontStyle = FontStyle.Italic;
                infoTextStyle.wordWrap = true;
            }
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

        public override void OnInspectorGUI()
        {
            InitializeStyles();

            Color originalBG = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);

            EditorGUILayout.BeginVertical(darkBoxStyle);

            EFSettings dataList = (EFSettings)target;

            GUILayout.Space(5);
            EditorGUILayout.LabelField(
    "ℹ️ Configure your data items below:\n" +
    "- Title: Represents the item name.\n" +
    "- Prefix: Defines the bucket.\n\n" +
    "Example:\n" +
    "If you have `string value1 = \"test\";` and save it with the title `value2` and prefix `values`,\n" +
    "it will appear in the database as:\n" +
    "values\n" +
    "└── value = test",
    infoTextStyle);

            GUILayout.Space(8);

            GUILayout.Space(15);
            EditorGUILayout.LabelField("🔐 Google Sign-In", darkHeaderStyle);
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Google ID Token (will be encrypted):", darkLabelStyle);
            dataList.googleIdToken = EditorGUILayout.TextField(dataList.googleIdToken, darkTextFieldStyle);

            if (GUILayout.Button("🔒 Encrypt & Store Securely", darkButtonStyle, GUILayout.Height(25)))
            {
                if (string.IsNullOrEmpty(dataList.googleIdToken))
                {
                    Debug.LogError("⚠️ Google ID Token is empty. Please enter a valid token before encrypting.");
                }
                else
                {
                    dataList.EncryptGoogleIdToken();
                    dataList.googleIdToken = "ENCRYPTED";
                    EditorUtility.SetDirty(dataList);
                    Debug.Log("✅ Google ID Token encrypted and stored securely.");
                }
            }



            GUILayout.Space(8);
            Rect separatorRect = GUILayoutUtility.GetRect(0, 2);
            EditorGUI.DrawRect(separatorRect, new Color(0.4f, 0.4f, 0.4f, 0.8f));
            GUILayout.Space(8);

            for (int i = 0; i < dataList.dataItems.Count; i++)
            {
                EditorGUILayout.BeginVertical(darkBoxStyle);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"📋 Item {i + 1}", darkHeaderStyle, GUILayout.Width(80));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Title:", darkLabelStyle, GUILayout.Width(40));
                dataList.dataItems[i].title = EditorGUILayout.TextField(dataList.dataItems[i].title, darkTextFieldStyle, GUILayout.Width(150));

                GUILayout.Space(20);

                EditorGUILayout.LabelField("Prefix:", darkLabelStyle, GUILayout.Width(45));
                dataList.dataItems[i].prefix = EditorGUILayout.TextField(dataList.dataItems[i].prefix, darkTextFieldStyle, GUILayout.Width(150));

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(8);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Type:", darkLabelStyle, GUILayout.Width(40));
                dataList.dataItems[i].dataType = (DataType)EditorGUILayout.EnumPopup(dataList.dataItems[i].dataType, GUILayout.Width(120));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(8);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("🗑️ Remove", darkRemoveButtonStyle, GUILayout.Width(100), GUILayout.Height(25)))
                {
                    dataList.dataItems.RemoveAt(i);
                    EditorUtility.SetDirty(dataList);
                    GUI.backgroundColor = originalBG;
                    return;
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                GUILayout.Space(8);
            }

            GUILayout.Space(15);

            if (GUILayout.Button("➕ Add New Data Item", darkButtonStyle, GUILayout.Height(30)))
            {
                dataList.dataItems.Add(new DataItem());
            }

            GUILayout.Space(5);
            EditorGUILayout.EndVertical();

            GUI.backgroundColor = originalBG;

            EditorUtility.SetDirty(dataList);
        }
    }
}