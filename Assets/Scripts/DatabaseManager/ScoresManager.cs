using System;
using System.Threading.Tasks;
using EF.Database; // Tool SimpleData
using EF.Generic;  // Tool EFManager
using TMPro;
using UnityEngine;

public class ScoresManager : MonoBehaviour
{
    public static ScoresManager Instance { get; private set; }

    [Header("Cloud Configuration")]
    [SerializeField] private string autoPrefix = "Player"; // Bắt buộc phải giống bên VocabFirebaseManager
    [SerializeField] private string scoreTagName = "TotalScore"; // Tên biến trên Firebase

    // Kênh lưu trữ điểm số
    private SimpleData _scoreDataChannel;

    [Header("Runtime Data")]
    public int LocalTotalScore = 0;    // Tổng điểm (Đã lưu)
    private int _currentWaveScore = 0; // Điểm màn chơi hiện tại (Chưa lưu)

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI txtTotalScore;
    [SerializeField] private TextMeshProUGUI txtWaveScore;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    
        // [FIX 1] Khởi tạo ngay trong Awake (chạy sớm nhất có thể)
        InitDataChannel();
    }

    private void Start()
    {
        // Start có thể để trống hoặc làm việc UI
        UpdateUI();
    }

// Hàm phụ để khởi tạo (tránh viết lặp lại code)
    private void InitDataChannel()
    {
        // Chỉ tạo mới nếu nó đang null
        if (_scoreDataChannel == null)
        {
            // Đảm bảo scoreTagName có giá trị (phòng trường hợp quên điền Inspector)
            if (string.IsNullOrEmpty(scoreTagName)) scoreTagName = "TotalScore";
        
            _scoreDataChannel = new SimpleData(scoreTagName, DataType.Integer, autoPrefix);
        }
    }

    // --- 1. LOGIC TÍNH ĐIỂM (Đã sửa lỗi Logic) ---

    public int ProcessAnswer(int vocabID, bool isPerfect)
    {
        // Bước 1: Lấy trạng thái CŨ (Trước khi trả lời câu này)
        int oldState = VocabFirebaseManager.Instance.GetVocabState(vocabID);
        
        int scoreToAdd = 0;
        int newState = isPerfect ? 2 : 1; // 2: Perfect, 1: NotPerfect

        Debug.Log($"[SCORE LOG] ID: {vocabID} | OldState: {oldState} | Perfect: {isPerfect}");

        // Bước 2: Tính toán điểm (5 - 3 - 2)
        if (oldState == 0) // Case 1: Từ mới tinh
        {
            scoreToAdd = isPerfect ? 5 : 3;
            // Quan trọng: Cập nhật state NGAY LẬP TỨC để tránh lỗi logic nếu hỏi lại từ này
            VocabFirebaseManager.Instance.UpdateVocabState(vocabID, newState);
            Debug.Log($"---> Từ mới: +{scoreToAdd} điểm");
        }
        else if (oldState == 1) // Case 2: Từ cũ (Chưa hoàn hảo)
        {
            if (isPerfect)
            {
                scoreToAdd = 2; // Bù điểm (5 - 3 = 2)
                VocabFirebaseManager.Instance.UpdateVocabState(vocabID, 2); // Up lên Perfect
                Debug.Log($"---> Nâng cấp: +{scoreToAdd} điểm");
            }
            else
            {
                Debug.Log("---> Vẫn sai: +0 điểm");
            }
        }
        else if (oldState == 2) // Case 3: Đã Perfect từ trước
        {
            scoreToAdd = 0; // Không cộng nữa
            Debug.Log("---> Đã Max: +0 điểm");
        }

        // Bước 3: Cộng vào điểm tạm (Wave Score)
        if (scoreToAdd > 0)
        {
            AddWaveScore(scoreToAdd);
        }

        return scoreToAdd;
    }

    public void AddWaveScore(int amount)
    {
        _currentWaveScore += amount;
        UpdateUI();
    }

    // --- 2. LOGIC CLOUD (Đã sửa lỗi Upload) ---

    public async Task LoadScoreFromCloudAsync()
    {
        Debug.Log("[CLOUD] Đang tải điểm số...");

        // [FIX 2] KIỂM TRA AN TOÀN TUYỆT ĐỐI
        // Nếu GameDataManager lỡ gọi hàm này trước cả Awake của script này -> Tự khởi tạo luôn
        InitDataChannel(); 

        try
        {
            // Kiểm tra lần cuối xem khởi tạo thành công chưa
            if (_scoreDataChannel == null) 
            {
                Debug.LogError("[CLOUD] Lỗi nghiêm trọng: Không thể khởi tạo SimpleData!");
                return;
            }

            // Dùng GetData an toàn
            int? savedScore = await _scoreDataChannel.GetData<int?>();
        
            LocalTotalScore = savedScore ?? 0;
        
            UpdateUI();
            Debug.Log($"[CLOUD] Tải thành công. Total Score: {LocalTotalScore}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CLOUD] Lỗi tải điểm: {ex.Message}");
            LocalTotalScore = 0; 
        }
    }

    public async Task SaveScoreToCloudAsync()
    {
        // ... (Đoạn cộng điểm giữ nguyên) ...
        LocalTotalScore += _currentWaveScore;
        _currentWaveScore = 0; 
        UpdateUI();

        // [FIX 3] Lại kiểm tra an toàn trước khi lưu
        InitDataChannel();

        try
        {
            // Gọi SetInteger
            int serverScore = await _scoreDataChannel.SetInteger(LocalTotalScore);
            // ... (Giữ nguyên code cũ) ...
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CLOUD] Lỗi Upload điểm: {ex.Message}");
        }
    }

    // --- 3. UI ---

    private void UpdateUI()
    {
        if (txtTotalScore) txtTotalScore.text = $"{LocalTotalScore}";
        
        // Chỉ hiện điểm cộng thêm nếu > 0
        if (txtWaveScore) 
        {
            txtWaveScore.text = _currentWaveScore > 0 ? $"{_currentWaveScore}" : "";
            txtWaveScore.gameObject.SetActive(_currentWaveScore > 0);
        }
    }
    
    public void ResetData()
    {
        LocalTotalScore = 0;
        _currentWaveScore = 0;
        UpdateUI(); // Cập nhật text về 0 luôn
    
        Debug.Log("[ScoresManager] Đã reset điểm về 0.");
    }
}