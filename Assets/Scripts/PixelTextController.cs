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
    
    [Header("Text")]
    [SerializeField] private string goodText;
    [SerializeField] private string prefectText;
    
    [Header("position")]
    [SerializeField] private Vector3 scoreTextPosition;
    [SerializeField] private Vector3 goodTextPosition;
    [SerializeField] private Vector3 prefectTextPosition;

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
}
