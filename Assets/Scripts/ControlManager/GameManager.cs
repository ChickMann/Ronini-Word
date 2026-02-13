using ControlManager;
using SmallHedge.AudioManager;
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
    public CutScenesManager CutScenesManager;
    public ScoresManager ScoresManager;
    public LoginWithGoogle LoginWithGoogle;
    public GameObject panel;
    
    private void Awake()
    {
        SmallHedge.AudioManager.AudioManager.PlayMusic(MusicType.MusicMenu);
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
        SmallHedge.AudioManager.AudioManager.PlayMusic(MusicType.MusicFight);
        gameState = GameState.Playing;
        combatManager.SetUpStartLevel(currentLevelData);
        panel.SetActive(false);
    }

    public void EndLevel()
    {
        gameState = GameState.CompletedLevel;
        combatManager.OnEndGame();
    }

}