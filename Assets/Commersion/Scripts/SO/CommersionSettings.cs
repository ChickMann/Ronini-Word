using System;
using UnityEngine;

namespace Commersion.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CommersionSettings", menuName = "Settings/CommersionSettings")]
    public class CommersionSettings : ScriptableObject
    {
        [Tooltip("If the user accepts the legal terms, we save this locally using PlayerPrefs")]
        [SerializeField] public bool saveLegalsLocally;
    
        [SerializeField] private PreLoaderSettings  preLoaderSettings;

        public CommersionLegal legals;
        public PreLoaderSettings GetPreLoaderSettings()
        {
            return preLoaderSettings;
        }
    }
    [Serializable]
    public class PreLoaderSettings
    {
        [Header("Settings")] 
        [SerializeField] public bool useCopyRightText;
        [SerializeField] public bool useLegalDisclaimer;
        [SerializeField] public Color _backgroundColor = Color.white;
        [SerializeField] public Color _textColor = Color.black;
        [SerializeField] public string copyrightText = "© {YEAR} {COMPANY}. All rights reserved.";
        [SerializeField] public string legalDisclaimer = "All trademarks, logos, and brand names are the property of {COMPANY} or their respective owners.\nUnauthorized use, reproduction, or distribution of any content is strictly prohibited.";

        [Header("Logos")] 
        [SerializeField] public Sprite logo;
        [SerializeField] public float logoHeight = 512;
        [SerializeField] public float logoWidth = 512;
    
        [Header("Conditions")]
        [SerializeField] public bool useTimer;
        [SerializeField] public float timer = 0;
        [SerializeField] public int nextScene = 0;
    }

    [Serializable]
    public class CommersionLegal
    {
        [SerializeField] public TextAsset TOSText;
        [SerializeField] public TextAsset PrivacyText;
    }

}
