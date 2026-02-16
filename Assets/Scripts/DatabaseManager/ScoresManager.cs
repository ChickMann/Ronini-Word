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

    public async Task SaveScoreToCloudAsync()
    {
        if (CurrentWaveScore <= 0) return; 

        int targetTotalScore = LocalTotalScore + CurrentWaveScore;

        // Chạy hiệu ứng (Logic hiệu ứng vẫn nằm đây vì nó thay đổi data theo thời gian)
        this.DelayAction(3f, () =>
        {
            _ = PlayScoreTransferEffectAsync(targetTotalScore, 0.5f);
        });  

        InitDataChannel();
        // Thực hiện lưu lên Cloud ở đây (bạn chưa viết hàm SaveData trong code cũ, hãy thêm vào nếu cần)
        // await _scoreDataChannel.SaveData(LocalTotalScore); 
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

            LocalTotalScore = Mathf.RoundToInt(Mathf.Lerp(startTotal, targetTotalScore, t));
            CurrentWaveScore = Mathf.RoundToInt(Mathf.Lerp(startWave, 0, t));

            NotifyUIUpdate(); // [CHANGED] Cập nhật UI liên tục trong vòng lặp

            await Task.Yield(); 
        }

        LocalTotalScore = targetTotalScore;
        CurrentWaveScore = 0;
        NotifyUIUpdate(); // [CHANGED] Cập nhật lần cuối
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