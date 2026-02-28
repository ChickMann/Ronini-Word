using System;
using PixelBattleText;
using UnityEngine;

public class PixelTextController : MonoBehaviour
{
    public static PixelTextController Instance;
    [Header("Text animation")]
    public TextAnimation scoreTextAnimation;
    public TextAnimation goodTextAnimation;
    public TextAnimation prefectTextAnimation;
    public TextAnimation addHealthTextAnimation;
    public TextAnimation addShieldTextAnimation;
    
    [Header("Text")]
    [SerializeField] private string goodText;
    [SerializeField] private string prefectText;
    [SerializeField] private string addHealthText;
    [SerializeField] private string addShieldText;
    
    [Header("position")]
    [SerializeField] private Vector3 scoreTextPosition;
    [SerializeField] private Vector3 goodTextPosition;
    [SerializeField] private Vector3 prefectTextPosition;
    [SerializeField] private Vector3 addHealthTextPosition;
    [SerializeField] private Vector3 addShieldTextPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }

    [ContextMenu("test score")]
    public void ScoreTextAnimation(int score)
    {
        string text = "+"+score;
        PixelBattleTextController.DisplayText(text, scoreTextAnimation,
            scoreTextPosition);
    }
    
    [ContextMenu("test good")]
    public void GoodTextAnimation()
    {
        
        PixelBattleTextController.DisplayText(goodText, goodTextAnimation,
            goodTextPosition);
    }
    
    [ContextMenu("test prefect")]
    public void PrefectTextAnimation()
    {
        
        PixelBattleTextController.DisplayText(prefectText, prefectTextAnimation,
            prefectTextPosition);
    }
    [ContextMenu("test Health")]
    public void HealthTextAnimation()
    {
        
        PixelBattleTextController.DisplayText(addHealthText, addHealthTextAnimation,
            addHealthTextPosition);
    }
    [ContextMenu("test Shield")]
    public void ShieldTextAnimation()
    {
        
        PixelBattleTextController.DisplayText(addShieldText, addShieldTextAnimation,
            addShieldTextPosition);
    }
}
