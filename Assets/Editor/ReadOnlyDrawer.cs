using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Lưu trạng thái GUI hiện tại
        bool previousGUIState = GUI.enabled;

        // Vô hiệu hóa GUI (làm mờ và không cho sửa)
        GUI.enabled = false;

        // Vẽ trường thuộc tính như bình thường nhưng ở trạng thái bị khóa
        EditorGUI.PropertyField(position, property, label);

        // Khôi phục trạng thái GUI để các biến khác bên dưới không bị khóa theo
        GUI.enabled = previousGUIState;
    }
}