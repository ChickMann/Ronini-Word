using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EF.Database;
using UnityEditor;
using UnityEngine;
using EF.Generic;

namespace EF.Editor
{
    [CustomPropertyDrawer(typeof(SimpleData))]
    public class EF_SimpleDataEditor : PropertyDrawer
    {
        private const string PREFIX = "CLOUPT - ";
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Define padding and layout measurements
            float padding = 5f;
            float headerHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            // Determine the field height (default popup height)
            float fieldHeight = EditorGUIUtility.singleLineHeight;
            // In case of warning, the HelpBox height will be larger (approximate)
            float helpBoxHeight = 40f;
            // We'll use the maximum of the two for consistent layout
            float contentFieldHeight = Mathf.Max(fieldHeight, helpBoxHeight);
            // Total content height: header + spacing + field (or helpbox)
            float contentHeight = headerHeight + spacing + contentFieldHeight;
            // Total height including padding (top and bottom)
            float totalHeight = padding + contentHeight + padding;
            
            // Draw a dark background box over the entire property area
            Color originalBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark color for box background
            GUI.Box(new Rect(position.x, position.y, position.width, totalHeight), GUIContent.none);
            GUI.backgroundColor = originalBackgroundColor; // Restore original background color

            // Define the inner content rectangle with padding applied
            Rect contentRect = new Rect(position.x + padding, position.y + padding, position.width - 2 * padding, contentHeight);

            // Draw header label with white bold text
            string headerText = PREFIX + property.name;
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.normal.textColor = Color.white;
            Rect headerRect = new Rect(contentRect.x, contentRect.y, contentRect.width, headerHeight);
            EditorGUI.LabelField(headerRect, headerText, headerStyle);

            // Retrieve the 'selectedTag' serialized property
            SerializedProperty selectedTagProperty = property.FindPropertyRelative("selectedTag");

            // Define the rectangle for the popup or warning message
            Rect fieldRect = new Rect(contentRect.x, contentRect.y + headerHeight + spacing, contentRect.width, contentFieldHeight);

            // Retrieve EFSettings from Resources
            EFSettings settings = EFSettings.Instance;
            if (settings != null)
            {
                // Check if there are any items in settings
                if (settings.dataItems == null || settings.dataItems.Count == 0)
                {
                    // Display a warning message if no items exist
                    EditorGUI.HelpBox(fieldRect, "No items found in EF-Settings. Please add an item.", MessageType.Warning);
                }
                else
                {
                    // Create lists for popup options from settings dataItems
                    List<string> tagOptions = new List<string>();
                    List<string> prefixOptions = new List<string>();

                    foreach (var dataItem in settings.dataItems)
                    {
                        tagOptions.Add(dataItem.title);
                        if (!prefixOptions.Contains(dataItem.prefix))
                        {
                            prefixOptions.Add(dataItem.prefix);
                        }
                    }

                    // Determine the current index for the popup
                    int selectedIndex = tagOptions.IndexOf(selectedTagProperty.stringValue);
                    // Display the popup field
                    selectedIndex = EditorGUI.Popup(fieldRect, "Select Tag", selectedIndex, tagOptions.ToArray());

                    // Update the serialized property if a valid selection is made
                    if (selectedIndex >= 0 && selectedIndex < tagOptions.Count)
                    {
                        selectedTagProperty.stringValue = tagOptions[selectedIndex];

                        // Update currentDataType property based on the selected tag
                        SerializedProperty currentDataTypeProp = property.FindPropertyRelative("currentDataType");
                        if (currentDataTypeProp != null)
                        {
                            // Find the matching DataItem in EFSettings
                            var matchingDataItem = settings.dataItems.FirstOrDefault(di => di.title == tagOptions[selectedIndex]);
                            if (matchingDataItem != null)
                            {
                                // Update the enum value of currentDataType to match the selected DataItem
                                currentDataTypeProp.enumValueIndex = (int)matchingDataItem.dataType;
                            }
                        }
                    }
                }
            }
            else
            {
                // Display error message if EFSettings resource is not found
                EditorGUI.HelpBox(fieldRect, "EFSettings not found in Resources/Settings.", MessageType.Error);
            }

            EditorGUI.EndProperty();

            // Apply changes if any modifications have occurred
            if (GUI.changed)
            {
                property.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float padding = 5f;
            float headerHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            // Field height for the popup is default singleLineHeight,
            // but for the HelpBox we consider an approximate height of 40.
            float fieldHeight = Mathf.Max(EditorGUIUtility.singleLineHeight, 40f);
            // Total height = top padding + header + spacing + field/HelpBox + bottom padding
            return padding + headerHeight + spacing + fieldHeight + padding;
        }
    }
}
