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
            PixelTextController.Instance.ScoreTextAnimation(scoreToAdd);
        }
        else if (oldState == 1) // Case 2: Từ cũ (Chưa hoàn hảo)
        {
            if (isPerfect)
            {
                scoreToAdd = 2; // Bù điểm (5 - 3 = 2)
                VocabFirebaseManager.Instance.UpdateVocabState(vocabID, 2); // Up lên Perfect
                Debug.Log($"---> Nâng cấp: +{scoreToAdd} điểm");
                PixelTextController.Instance.ScoreTextAnimation(scoreToAdd);
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
        if (_currentWaveScore <= 0) return; // Bỏ qua nếu không có điểm để cộng

        // 1. Tính toán điểm mục tiêu BẮT BUỘC TRƯỚC khi chạy hiệu ứng
        int targetTotalScore = LocalTotalScore + _currentWaveScore;

        // 2. Chạy hiệu ứng cộng điểm mượt mà (vd: trong 1 giây)
        await PlayScoreTransferEffectAsync(targetTotalScore, 0.5f);

        // 3. Tiến hành lưu Cloud
        InitDataChannel();
    }

    /// <summary>
    /// Hiệu ứng chuyển điểm từ WaveScore sang TotalScore mượt mà theo thời gian.
    /// </summary>
    private async Task PlayScoreTransferEffectAsync(int targetTotalScore, float duration)
    {
        int startTotal = LocalTotalScore;
        int startWave = _currentWaveScore;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Mathf.Lerp giúp con số tăng/giảm mượt mà bất kể khoảng cách điểm lớn hay nhỏ
            LocalTotalScore = Mathf.RoundToInt(Mathf.Lerp(startTotal, targetTotalScore, t));
            _currentWaveScore = Mathf.RoundToInt(Mathf.Lerp(startWave, 0, t));

            UpdateUI();

            // Trả quyền điều khiển lại cho Unity Main Thread để vẽ UI frame này,
            // sau đó tiếp tục vòng lặp ở frame tiếp theo.
            await Task.Yield(); 
        }

        // Đảm bảo sau khi kết thúc hiệu ứng, con số là chính xác tuyệt đối
        LocalTotalScore = targetTotalScore;
        _currentWaveScore = 0;
        UpdateUI();
    }

    // --- 3. UI ---

    private void UpdateUI()
    {
        if (txtTotalScore) txtTotalScore.text = $"TotalScore: {LocalTotalScore}";
        
        // Chỉ hiện điểm cộng thêm nếu > 0
        if (txtWaveScore) 
        {
            txtWaveScore.text = $"Score: {_currentWaveScore}";
           
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