using System;
using System.Collections;
using Commersion.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Commersion.Preloader
{
    public class PreloadManager : MonoBehaviour
{
    [Header("UI")] 
    [SerializeField] private TMP_Text CopyrightText;
    [SerializeField] private TMP_Text LegalDisclaimerText;
    [SerializeField] private Image Logo;

    private void Awake()
    {
        CommersionManager.Instance.Initialize();
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        var preLoaderSettings = CommersionManager.Instance.GetSettings().GetPreLoaderSettings();
    
        if (preLoaderSettings == null)
        {
            Debug.LogError("PreLoaderSettings is null! Initialization aborted.");
            return;
        }
    
        if (CopyrightText == null)
        {
            Debug.LogError("CopyrightText reference is null!");
        }
        else
        {
            CopyrightText.text = preLoaderSettings.copyrightText;
            CopyrightText.color = preLoaderSettings._textColor;
            Debug.Log("CopyrightText set successfully.");
        }
    
        if (LegalDisclaimerText == null)
        {
            Debug.LogError("LegalDisclaimerText reference is null!");
        }
        else
        {
            LegalDisclaimerText.text = preLoaderSettings.legalDisclaimer;
            LegalDisclaimerText.color = preLoaderSettings._textColor;
            Debug.Log("LegalDisclaimerText set successfully.");
        }
    
        if (Logo == null)
        {
            Debug.LogError("Logo reference is null!");
        }
        else if (preLoaderSettings.logo == null)
        {
            Debug.LogWarning("Logo sprite in PreLoaderSettings is null.");
        }
        else
        {
            Logo.sprite = preLoaderSettings.logo;
            Debug.Log("Logo sprite set successfully.");
        }

        if (preLoaderSettings.useTimer)
        {
            StartCoroutine(StartTimer(preLoaderSettings.timer,preLoaderSettings.nextScene));
        }
    }

    IEnumerator StartTimer(float seconds,int nextScene)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene(nextScene);
    }

}

}
