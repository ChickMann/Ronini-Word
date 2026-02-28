using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EF.Database;
using EF.Generic;
using Firebase.Database;
using Newtonsoft.Json;
using UnityEngine;

public class VocabFirebaseManager : MonoBehaviour
{
    public static VocabFirebaseManager Instance { get; private set; }

    [Header("Cloud Config")]
    [SerializeField] private string autoPrefix = "Player"; // Tự động lấy UserID
    
    // Channel lưu danh sách từ (JSON String)
    private SimpleData _vocabDataChannel;

    // Cache dữ liệu (Key: VocabID, Value: State [0:None, 1:NotPerfect, 2:Perfect])
    public Dictionary<string, int> VocabDict = new Dictionary<string, int>();

    // Danh sách "Bẩn" (Những từ vừa thay đổi cần lưu lên mây)
    private HashSet<string> _dirtyVocabIDs = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Khởi tạo SimpleData
        _vocabDataChannel = new SimpleData("Vocabs", DataType.String, autoPrefix);
    }

    // --- 1. API CHO SCORES MANAGER GỌI ---

      private string GetSafeKey(int vocabID)
      {
          return "id_" + vocabID; // Biến số 1 thành "id_1"
      }
  
      public int GetVocabState(int vocabID)    {
        string key = GetSafeKey(vocabID); // Dùng Key an toàn
        if (VocabDict.TryGetValue(key, out int state)) return state;
        return 0;
    }

    public void UpdateVocabState(int vocabID, int newState)
    {
        string key = GetSafeKey(vocabID); // Dùng Key an toàn

        if (VocabDict.ContainsKey(key)) VocabDict[key] = newState;
        else VocabDict.Add(key, newState);

        if (!_dirtyVocabIDs.Contains(key))
        {
            _dirtyVocabIDs.Add(key);
        }
    }
    
    public async Task LoadVocabsFromCloudAsync()
    {
        string path = EFManager.Instance.RePlacePrefix(autoPrefix);
        Debug.Log($"🔍 Đang mò dữ liệu tại đường dẫn: {path}/Vocabs");
        try
        {
            // Bây giờ Firebase sẽ trả về Object {"id_1": 2, "id_2": 1}...
            // Nên Dictionary sẽ deserialize thành công!
            var savedData = await _vocabDataChannel.GetJsonValue<Dictionary<string, int>>();

            if (savedData != null)
            {
                VocabDict = savedData;
                Debug.Log($"[VocabManager] Đã tải {VocabDict.Count} từ vựng.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[VocabManager] Lỗi tải dữ liệu: {ex.Message}");
            VocabDict = new Dictionary<string, int>();
        }
    }

    public async Task SaveDirtyVocabsToCloudAsync()
    {
        // Bước 1: Check snapshot xem data gốc đã có chưa
        var snapshot = await _vocabDataChannel.GetSnapshot();

        if (!snapshot.Exists)
        {
            // Nếu chưa có gì -> Lưu toàn bộ (Full Save)
            Debug.Log("[VocabManager] Cloud rỗng -> Upload toàn bộ.");
            await _vocabDataChannel.SetData(VocabDict);
            _dirtyVocabIDs.Clear();
        }
        else if (_dirtyVocabIDs.Count > 0)
        {
            // Nếu đã có -> Chỉ lưu cái mới (Partial Update)
            Dictionary<string, object> updates = new Dictionary<string, object>();
            
            foreach (string id in _dirtyVocabIDs)
            {
                if (VocabDict.TryGetValue(id, out int state))
                {
                    updates.Add(id, state);
                }
            }

            try
            {
                // Dùng UpdateChildrenAsync để không ghi đè dữ liệu khác
                var refDb = EFManager.Instance.GetReference()
                    .Child(EFManager.Instance.RePlacePrefix(autoPrefix))
                    .Child("Vocabs");

                await refDb.UpdateChildrenAsync(updates);
                
                Debug.Log($"[VocabManager] Đã cập nhật {_dirtyVocabIDs.Count} từ vựng lên mây.");
                _dirtyVocabIDs.Clear();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VocabManager] Lỗi Update: {ex.Message}");
            }
        }
        
    }
    public void ResetData()
    {
        // Xóa sạch bộ nhớ đệm
        VocabDict.Clear();
        _dirtyVocabIDs.Clear();
    
        Debug.Log("[VocabManager] Đã dọn sạch dữ liệu RAM.");
    }
}