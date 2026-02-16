using System;
using System.Collections.Generic;
using ControlManager;
using SmallHedge.AudioManager;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public GameState gameState;

    [Header("Data")]
    public List<LevelData> levelDataList;
    public LevelData currentLevelData;

    [Header("Managers")]
    public BackGroundManager backGroundManager;
    public InputDisplayManager inputDisplayManager;
    public CombatManager combatManager; 
    public CutScenesManager CutScenesManager;
    public LoginWithGoogle loginWithGoogle;
    
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
        this.DelayAction(2f, () =>
        {
            gameState = GameState.Playing;
            combatManager.SetUpStartLevel(currentLevelData);
            
        });
        
    }

    public void SetCurrentLevel(LevelData data)
    {
        currentLevelData = data;
    }

    public void EndLevel()
    {
        this.DelayAction(2f,() => UIManager.Instance.SetActiveCompletedPanel(true));
        gameState = GameState.CompletedLevel;
        combatManager.OnEndGame();
    }
    
    
    [ContextMenu("Restart Level")]
    public void RestartLevel()
    {
        AudioManager.PlayMusic(MusicType.MusicFight);
        gameState = GameState.Playing;
         combatManager.ResetLevlel();
    }

    [ContextMenu("Back To Menu")]
    public void BackToMenu()
    {
        AudioManager.PlayMusic(MusicType.MusicMenu);
        combatManager.ResetAll();
    }

}