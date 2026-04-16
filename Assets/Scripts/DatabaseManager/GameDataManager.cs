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
      
        Debug.Log(" Đang đợi kết nối Firebase...");
        
        float timer = 0;
        while (FirebaseAuth.DefaultInstance.CurrentUser == null && timer < 5f)
        {
            timer += Time.deltaTime;
            yield return null; 
        }

        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            Debug.Log($"Đã đăng nhập: {FirebaseAuth.DefaultInstance.CurrentUser.UserId}");
            Debug.Log("Bắt đầu tải dữ liệu...");
            
            
            yield return new WaitForSeconds(0.5f); 
            
            SyncFromCloud();
        }
        else
        {
            Debug.LogWarning("Không thể đăng nhập hoặc đang Offline. Sử dụng dữ liệu máy.");
            LoadLocalBackup();
        }
    }




   
    public void OnPlayerAnswered(int vocabID, bool isPerfect)
    {
        ScoresManager.Instance.ProcessAnswer(vocabID, isPerfect);
        SaveLocalBackup();
    }


    public async void OnWaveEnded()
    {
        Debug.Log("Kết thúc màn -> Bắt đầu đồng bộ Online...");
        
        await ScoresManager.Instance.SaveScoreToCloudAsync();
        await VocabFirebaseManager.Instance.SaveDirtyVocabsToCloudAsync();
        SaveLocalBackup();
        
        Debug.Log("ĐỒNG BỘ HOÀN TẤT!");
    }

    // HỆ THỐNG BACKUP OFFLINE (JSON)

    private void SaveLocalBackup()
    {
        var data = new LocalBackupData
        {
            TotalScore = ScoresManager.Instance.LocalTotalScore,
            vocabs = VocabFirebaseManager.Instance.VocabDict
        };

        string json = JsonConvert.SerializeObject(data);
        File.WriteAllText(BackupPath, json);
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
                // Khôi phục dữ liệu 
                ScoresManager.Instance.LocalTotalScore = data.TotalScore;
                VocabFirebaseManager.Instance.VocabDict = data.vocabs ?? new Dictionary<string, int>();
                
                Debug.Log("Đã khôi phục dữ liệu từ máy (Offline Mode).");
                return true;
            }
        }
        catch { Debug.LogError("File Save bị lỗi, sẽ tải lại từ Cloud."); }
        
        return false;
    }

    //  HỆ THỐNG ĐỒNG BỘ CLOUD 

    public async Task SyncFromCloud()
    {
        
        var task1 = ScoresManager.Instance.LoadScoreFromCloudAsync();
        var task2 = VocabFirebaseManager.Instance.LoadVocabsFromCloudAsync();

        await Task.WhenAll(task1, task2); 
    }
    public void HardReset()
    {
        Debug.Log(" BẮT ĐẦU DỌN DẸP DỮ LIỆU CŨ...");

        // Xóa file save JSON trong máy
        string path = Path.Combine(Application.persistentDataPath, "user_backup.json");
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("🗑️ Đã xóa file user_backup.json");
        }

     
        if (VocabFirebaseManager.Instance != null) 
            VocabFirebaseManager.Instance.ResetData();

        if (ScoresManager.Instance != null) 
            ScoresManager.Instance.ResetData();


    }
    
   /// <summary>
    /// Trả về Dictionary:
    /// - Key: VocabData (Dữ liệu từ vựng gốc)
    /// - Value: int (Trạng thái/Status đã lưu, ví dụ: 2)
    /// </summary>
    public Dictionary<VocabData, int> GetVocabMapFromBackup()
    {
    
        Dictionary<VocabData, int> resultMap = new Dictionary<VocabData, int>();

        if (!File.Exists(BackupPath))
        {
            Debug.LogWarning("⚠️ Không tìm thấy file backup user_backup.json");
            return resultMap;
        }

        try
        {
            string json = File.ReadAllText(BackupPath);
            LocalBackupData data = JsonConvert.DeserializeObject<LocalBackupData>(json);

            if (data == null || data.vocabs == null || data.vocabs.Count == 0)
            {
                Debug.Log("File backup rỗng.");
                return resultMap;
            }

       
            Dictionary<int, VocabData> masterVocabMap = new Dictionary<int, VocabData>();

            if (GameManager.Instance != null && GameManager.Instance.levelDataList != null)
            {
                foreach (var level in GameManager.Instance.levelDataList)
                {
                    foreach (var wave in level.Waves)
                    {
                        foreach (var vocab in wave.VocabList)
                        {
                            if (!masterVocabMap.ContainsKey(vocab.VocabID))
                            {
                                masterVocabMap.Add(vocab.VocabID, vocab);
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Chưa load được LevelData trong GameManager!");
                return resultMap;
            }

            foreach (var kvp in data.vocabs)
            {
                // kvp.Key = "id_9"
                // kvp.Value = 2 (Status)

         
                string cleanIdString = kvp.Key.Replace("id_", "").Trim();

                if (int.TryParse(cleanIdString, out int vocabID))
                {
                   
                    if (masterVocabMap.TryGetValue(vocabID, out VocabData foundVocab))
                    {
                        int status = kvp.Value; 
                        

                        if (!resultMap.ContainsKey(foundVocab))
                        {
                            resultMap.Add(foundVocab, status);
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($" Lỗi GetVocabMapFromBackup: {ex.Message}");
        }

        Debug.Log($"Đã khôi phục {resultMap.Count} cặp (Vocab + Status) từ Backup.");
        return resultMap;
    }
   
}
