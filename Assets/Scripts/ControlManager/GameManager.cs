using System;
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
       
        // Singleton Implementation
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
     
    }

    private void Start()
    {
        AudioManager.PlayMusic(MusicType.MusicMenu);
        gameState = GameState.Menu;
    }

    [ContextMenu("Start Level")]
    public void StartLevel()
    {
        AudioManager.PlayMusic(MusicType.MusicFight);
        gameState = GameState.Playing;
        combatManager.SetUpStartLevel(currentLevelData);
        panel.SetActive(false);
    }

    public void EndLevel()
    {
        panel.SetActive(true);
        gameState = GameState.CompletedLevel;
        combatManager.OnEndGame();
    }
    [ContextMenu("Restart Level")]
    public void RestartLevel()
    {
        panel.SetActive(false);
        gameState = GameState.Playing;
        this.DelayAction(2f,() => StartLevel());
    }

    [ContextMenu("Back To Menu")]
    public void BackToMenu()
    {
        AudioManager.PlayMusic(MusicType.MusicMenu);
    }

}