using ControlManager;
using UnityEngine;

/// <summary>
/// Singleton quản lý vòng đời game (Game Loop), chuyển cảnh và khởi tạo.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public GameState gameState;
    public bool isStartLevel;
    public bool isCompletedLevel;

    [Header("Data")]
    public LevelData currentLevelData;
    // Đã chuyển CurrentVocabIndex sang CombatManager quản lý

    [Header("Managers")]
    public BackGroundManager backGroundManager;
    public InputDisplayManager inputDisplayManager;
    public CombatManager combatManager; // Renamed to PascalCase
    public AudioManager audioManager;
    public CutScenesManager CutScenesManager;
    
    private void Awake()
    {
        // Singleton Implementation
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        gameState = GameState.Menu;
    }

    private void Start()
    {
        // Khởi động vocab đầu tiên nếu cần thiết hoặc đợi lệnh từ UI Menu
        if (currentLevelData != null)
        {
            // CombatManager sẽ tự StartLevel qua sự kiện OnLevelStart
        }
    }

    private void OnEnable()
    {
        // Không còn nghe OnSubmitAnswer ở đây nữa, CombatManager sẽ xử lý
    }

    private void OnDisable()
    {
        
    }

    private void Update()
    {
        if(gameState == GameState.Playing && !isStartLevel) 
        {
            StartLevel();
        }
    }
    
    public void StartLevel()
    {
        if (currentLevelData != null)
        {}
            GameEvents.OnLevelStart?.Invoke(currentLevelData);
            isStartLevel = true;
        }
    public void LoadLevel(int levelID)
    {
        // Logic load scene hoặc load data level mới
    }

    public void CompletedLevel()
    {
        isCompletedLevel = true;
        GameEvents.OnLevelComplete?.Invoke(true);
    }

    // --- Audio Proxies ---
    public void PlayerSwordEffect()
    {
        if(audioManager) audioManager.PlaySwordSound();
    }
    
    public void PlayerFootStepEffect(bool isPlay)
    {
        if(audioManager) audioManager.PlayFootStep(isPlay);
    }
    
    
   
}