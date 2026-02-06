using ControlManager;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public GameState gameState;

    [Header("Data")]
    public LevelData currentLevelData;

    [Header("Managers")]
    public BackGroundManager backGroundManager;
    public InputDisplayManager inputDisplayManager;
    public CombatManager combatManager; 
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
    
    [ContextMenu("Start Level")]
    public void StartLevel()
    {
        gameState = GameState.Playing;
        combatManager.SetUpStartLevel(currentLevelData);
    }

    public void EndLevel()
    {
        gameState = GameState.CompletedLevel;
        combatManager.OnEndGame();
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