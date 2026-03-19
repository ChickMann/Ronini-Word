#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text;
using System.IO;

public class VocabListGenerator
{
    [MenuItem("Tools/Ronin/Generate Vocab List (Text)")]
    public static void GenerateTextList()
    {
        // 1. Tìm tất cả file VocabData (Giữ nguyên)
        string[] guids = AssetDatabase.FindAssets("t:VocabData");
        
        var vocabList = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<VocabData>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(v => v != null && v.VocabID > 0)
            .OrderBy(v => v.VocabID)
            .ToList();

        if (vocabList.Count == 0)
        {
            Debug.LogWarning("Không tìm thấy từ vựng nào có ID hợp lệ!");
            return;
        }

        // 2. Xây dựng nội dung Text (Giữ nguyên)
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== DANH SÁCH TỪ VỰNG RONIN WORD ===");
        sb.AppendLine($"Tổng số: {vocabList.Count} từ");
        sb.AppendLine("------------------------------------");
        sb.AppendLine("ID: Tên File (Nghĩa) - Đáp án"); // Header

        foreach (var vocab in vocabList)
        {
            // Format: ID: TênFile (Nghĩa)
            // Ví dụ: 1: Vocab_Cat (Con mèo)
            sb.AppendLine($"{vocab.VocabID}: {vocab.name} ({vocab.Meaning}) - {vocab.Answer}");
        }

        // 3. XỬ LÝ ĐƯỜNG DẪN MỚI
        // Application.dataPath trỏ tới thư mục "Assets" của dự án
        string folderPath = Path.Combine(Application.dataPath, "Resources/VocabsData/VocabsListText");
        string fileName = "VocabList_Export.txt";
        string fullPath = Path.Combine(folderPath, fileName);

        // Tạo thư mục nếu chưa có (Tránh lỗi DirectoryNotFoundException)
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log($"Đã tạo thư mục mới: {folderPath}");
        }

        // Ghi file
        File.WriteAllText(fullPath, sb.ToString());
        
        // Refresh lại Unity Editor để file hiện ra ngay lập tức
        AssetDatabase.Refresh();
        
        // Mở file lên xem
        Application.OpenURL(fullPath);
        
        Debug.Log($"<color=green>Đã xuất danh sách ra file: {fullPath}</color>");
    }
}
#endif