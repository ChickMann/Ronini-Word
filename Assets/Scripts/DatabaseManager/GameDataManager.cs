using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Firebase.Auth;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class LocalBackupData
{
    public int TotalScore;
    public Dictionary<string, int> vocabs;
}

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    private string BackupPath => Path.Combine(Application.persistentDataPath, "user_backup.json");

    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        // [FIX] CHỜ ĐĂNG NHẬP
        // Đợi cho đến khi Firebase lấy được User ID hoặc quá 5 giây (timeout)
        Debug.Log(" Đang đợi kết nối Firebase...");
        
        float timer = 0;
        while (FirebaseAuth.DefaultInstance.CurrentUser == null && timer < 5f)
        {
            timer += Time.deltaTime;
            yield return null; // Đợi 1 frame
        }

        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            Debug.Log($"Đã đăng nhập: {FirebaseAuth.DefaultInstance.CurrentUser.UserId}");
            Debug.Log("Bắt đầu tải dữ liệu...");
            
            // Đợi thêm 1 chút để EFManager cập nhật Dictionary thay thế
            yield return new WaitForSeconds(0.5f); 
            
            SyncFromCloud();
        }
        else
        {
            Debug.LogWarning("⚠️ Không thể đăng nhập hoặc đang Offline. Sử dụng dữ liệu máy.");
            LoadLocalBackup();
        }
    }
    
    // ... (Các phần còn lại giữ nguyên) ...


    // --- 1. XỬ LÝ SỰ KIỆN TRONG GAME ---

    // Gọi hàm này KHI TRẢ LỜI XONG 1 CÂU
    public void OnPlayerAnswered(int vocabID, bool isPerfect)
    {
        // 1. Tính điểm & Cập nhật trạng thái (RAM)
        ScoresManager.Instance.ProcessAnswer(vocabID, isPerfect);

        // 2. Lưu ngay xuống ổ cứng máy (An toàn 100% nếu mất điện/crash)
        SaveLocalBackup();
    }

    // Gọi hàm này KHI HẾT MÀN (End Wave)
    public async void OnWaveEnded()
    {
        Debug.Log("Kết thúc màn -> Bắt đầu đồng bộ Online...");
        
        // 1. Lưu điểm số lên mây
        await ScoresManager.Instance.SaveScoreToCloudAsync();

        // 2. Lưu các từ vựng mới học lên mây
        await VocabFirebaseManager.Instance.SaveDirtyVocabsToCloudAsync();
        
        // 3. Update lại file backup lần nữa cho chắc
        SaveLocalBackup();
        
        Debug.Log("ĐỒNG BỘ HOÀN TẤT!");
    }

    // --- 2. HỆ THỐNG BACKUP OFFLINE (JSON) ---

    private void SaveLocalBackup()
    {
        var data = new LocalBackupData
        {
            TotalScore = ScoresManager.Instance.LocalTotalScore,
            vocabs = VocabFirebaseManager.Instance.VocabDict
        };

        string json = JsonConvert.SerializeObject(data);
        File.WriteAllText(BackupPath, json);
        // Debug.Log("Đã Auto-Save xuống máy.");
    }

    private bool LoadLocalBackup()
    {
        if (!File.Exists(BackupPath)) return false;

        try
        {
            string json = File.ReadAllText(BackupPath);
            var data = JsonConvert.DeserializeObject<LocalBackupData>(json);

            if (data != null)
            {
                // Khôi phục dữ liệu vào 2 Manager
                ScoresManager.Instance.LocalTotalScore = data.TotalScore;
                VocabFirebaseManager.Instance.VocabDict = data.vocabs ?? new System.Collections.Generic.Dictionary<string, int>();
                
                Debug.Log("Đã khôi phục dữ liệu từ máy (Offline Mode).");
                return true;
            }
        }
        catch { Debug.LogError("File Save bị lỗi, sẽ tải lại từ Cloud."); }
        
        return false;
    }

    // --- 3. HỆ THỐNG ĐỒNG BỘ CLOUD ---

    public async Task SyncFromCloud()
    {
        // Ra lệnh cho 2 đệ tử đi tải hàng về
        var task1 = ScoresManager.Instance.LoadScoreFromCloudAsync();
        var task2 = VocabFirebaseManager.Instance.LoadVocabsFromCloudAsync();

        await Task.WhenAll(task1, task2); // Đợi cả 2 xong
    }
    public void HardReset()
    {
        Debug.Log(" BẮT ĐẦU DỌN DẸP DỮ LIỆU CŨ...");

        // 1. Xóa file save vật lý (JSON) trong máy
        string path = Path.Combine(Application.persistentDataPath, "user_backup.json");
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("🗑️ Đã xóa file user_backup.json");
        }

        // 2. Reset RAM của các Manager
        if (VocabFirebaseManager.Instance != null) 
            VocabFirebaseManager.Instance.ResetData();

        if (ScoresManager.Instance != null) 
            ScoresManager.Instance.ResetData();

        // 3. (Tùy chọn) Reset các biến cục bộ khác của GameDataManager nếu có
    }
   
}