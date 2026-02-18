using System;
using System.Threading.Tasks;
using EF.Database; // Tool SimpleData
using EF.Generic;  // Tool EFManager
using UnityEngine;

public class ScoresManager : MonoBehaviour
{
    public static ScoresManager Instance { get; private set; }

    [Header("Cloud Configuration")]
    [SerializeField] private string autoPrefix = "Player"; 
    [SerializeField] private string scoreTagName = "TotalScore"; 

    private SimpleData _scoreDataChannel;

    [Header("Runtime Data")]
    public int LocalTotalScore = 0;    
    
    // Đổi thành Property để public get cho UIManager đọc, nhưng private set
    public int CurrentWaveScore { get; private set; } = 0; 

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    
        InitDataChannel();
    }

    private void InitDataChannel()
    {
        if (_scoreDataChannel == null)
        {
            if (string.IsNullOrEmpty(scoreTagName)) scoreTagName = "TotalScore";
            _scoreDataChannel = new SimpleData(scoreTagName, DataType.Integer, autoPrefix);
        }
    }

    // --- 1. LOGIC TÍNH ĐIỂM ---

    public int ProcessAnswer(int vocabID, bool isPerfect)
    {
        int oldState = VocabFirebaseManager.Instance.GetVocabState(vocabID);
        int scoreToAdd = 0;
        int newState = isPerfect ? 2 : 1; 

        Debug.Log($"[SCORE LOG] ID: {vocabID} | OldState: {oldState} | Perfect: {isPerfect}");

        if (oldState == 0) 
        {
            scoreToAdd = isPerfect ? 5 : 3;
            VocabFirebaseManager.Instance.UpdateVocabState(vocabID, newState);
            PixelTextController.Instance.ScoreTextAnimation(scoreToAdd);
        }
        else if (oldState == 1) 
        {
            if (isPerfect)
            {
                scoreToAdd = 2; 
                VocabFirebaseManager.Instance.UpdateVocabState(vocabID, 2); 
                PixelTextController.Instance.ScoreTextAnimation(scoreToAdd);
            }
        }
        
        if (scoreToAdd > 0)
        {
            AddWaveScore(scoreToAdd);
        }

        return scoreToAdd;
    }

    public void AddWaveScore(int amount)
    {
        CurrentWaveScore += amount;
        NotifyUIUpdate();
    }

    // --- 2. LOGIC CLOUD ---

    public async Task LoadScoreFromCloudAsync()
    {
        Debug.Log("[CLOUD] Đang tải điểm số...");
        InitDataChannel(); 

        try
        {
            if (_scoreDataChannel == null) return;

            int? savedScore = await _scoreDataChannel.GetData<int?>();
            LocalTotalScore = savedScore ?? 0;
        
            NotifyUIUpdate(); // [CHANGED] Cập nhật UI sau khi tải
            Debug.Log($"[CLOUD] Tải thành công. Total Score: {LocalTotalScore}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CLOUD] Lỗi tải điểm: {ex.Message}");
            LocalTotalScore = 0; 
        }
    }

// --- 2. LOGIC CLOUD (ĐÃ SỬA) ---

    public async Task SaveScoreToCloudAsync()
    {
        // 1. Kiểm tra điều kiện
        if (CurrentWaveScore <= 0) return; 

        // 2. Tính toán tổng điểm đích ngay lập tức
        int targetTotalScore = LocalTotalScore + CurrentWaveScore;

        Debug.Log($"[CLOUD] Bắt đầu lưu điểm: {targetTotalScore} (Cũ: {LocalTotalScore} + Mới: {CurrentWaveScore})");

        // 3. LƯU LÊN FIREBASE NGAY LẬP TỨC (Không chờ hiệu ứng)
        InitDataChannel();
        try 
        {
            // Giả định hàm trong SimpleData là SaveData hoặc SetData. 
            // Nếu tool của bạn dùng tên khác (vd: SetData), hãy đổi tên hàm này.
            // Đổi SaveData -> SetData
            await _scoreDataChannel.SetData(targetTotalScore);
            
            Debug.Log("[CLOUD] ✅ Đã upload điểm lên Firebase thành công!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CLOUD] ❌ Lỗi upload điểm: {ex.Message}");
            // Tùy chọn: Có thể return luôn nếu muốn fail-safe, nhưng vẫn nên cho chạy hiệu ứng UI cho đẹp
        }

        // 4. Chạy hiệu ứng chuyển điểm (Visual Only)
        // Không dùng DelayAction stringy nữa, dùng Task.Delay chuẩn async
        await Task.Delay(3000); // Đợi 3s như logic cũ của bạn
        
        await PlayScoreTransferEffectAsync(targetTotalScore, 0.5f);
    }

    private async Task PlayScoreTransferEffectAsync(int targetTotalScore, float duration)
    {
        int startTotal = LocalTotalScore;
        int startWave = CurrentWaveScore;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Lerp giá trị hiển thị
            LocalTotalScore = Mathf.RoundToInt(Mathf.Lerp(startTotal, targetTotalScore, t));
            
            // Giảm điểm wave về 0
            CurrentWaveScore = Mathf.RoundToInt(Mathf.Lerp(startWave, 0, t));

            NotifyUIUpdate(); 
            await Task.Yield(); 
        }

        // Chốt giá trị cuối cùng để đảm bảo chính xác
        LocalTotalScore = targetTotalScore;
        CurrentWaveScore = 0;
        NotifyUIUpdate(); 
    }

    public void ResetData()
    {
        LocalTotalScore = 0;
        CurrentWaveScore = 0;
        NotifyUIUpdate(); // [CHANGED]
        Debug.Log("[ScoresManager] Đã reset điểm về 0.");
    }

    // --- HELPER ĐỂ GỌI SANG UIMANAGER ---
    private void NotifyUIUpdate()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScoreHUD(LocalTotalScore, CurrentWaveScore);
            UIManager.Instance.updateScoreText();
        }
    }
}