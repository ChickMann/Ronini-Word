using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq; 
#endif

public enum StateVocab 
{
    None,
    NotPerfect,
    Perfect
}

[CreateAssetMenu(fileName = "NewVocabData", menuName = "Scriptable Objects/VocabData")]
public class VocabData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField, ReadOnly] private int _vocabID;
    public int VocabID => _vocabID;

    [Header("Data")]
    public string Meaning;
    public string Answer;

    private void Reset()
    {
#if UNITY_EDITOR
        //  chạy khi ID = 0 tạo mới
        if (_vocabID == 0) AssignSmartID(forceFill: false);
#endif
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        // chạy khi ID = 0
        if (_vocabID == 0) AssignSmartID(forceFill: false);
#endif
    }

#if UNITY_EDITOR
    [ContextMenu("Force Smart ID (Fill Gaps)")]
    private void AssignSmartID_Manual()
    {
        AssignSmartID(forceFill: true);
    }

    private void AssignSmartID(bool forceFill)
    {
        // Chuẩn bị danh sách ID đã dùng
        string myPath = AssetDatabase.GetAssetPath(this);
        string[] guids = AssetDatabase.FindAssets("t:VocabData");
        HashSet<int> usedIDs = new HashSet<int>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path == myPath) continue; 

            VocabData other = AssetDatabase.LoadAssetAtPath<VocabData>(path);
            if (other != null && other.VocabID > 0)
            {
                usedIDs.Add(other.VocabID);
            }
        }


        if (!forceFill && _vocabID > 0 && !usedIDs.Contains(_vocabID))
        {
            return;
        }

        // Tìm lỗ trống nhỏ nhất bắt đầu từ 1
        int candidateID = 1;
        while (usedIDs.Contains(candidateID))
        {
            candidateID++;
        }


        // Nếu forceFill = true, nó sẽ so sánh candidateID (ví dụ 3) với _vocabID hiện tại (ví dụ 5)
        if (_vocabID != candidateID)
        {
            // Cho phép Undo (Ctrl+Z) trong Editor
            Undo.RecordObject(this, "Change Vocab ID");
            
            int oldID = _vocabID;
            _vocabID = candidateID;
            
            EditorUtility.SetDirty(this);
            Debug.Log($"<color=cyan>[Smart ID]</color> Đã dời ID của {name}: {oldID} -> {_vocabID} (Lấp lỗ trống)");
        }
        else if (forceFill)
        {
            Debug.Log($"<color=gray>[Smart ID]</color> {name} đang ở vị trí tối ưu ({_vocabID}), không cần đổi.");
        }
    }
#endif
}
