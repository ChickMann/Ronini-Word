using System.Threading.Tasks;
using Commersion.Core;
using Commersion.Core.Legal;
using Commersion.ScriptableObjects;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Commersion.Core
{
    public class CommersionManager : MonoBehaviour
{
    private static CommersionManager _instance;
    
    // Public property to access the singleton instance
    public static CommersionManager Instance
    {
        get
        {
            // If instance doesn't exist, find it in the scene
            if (_instance == null)
            {
                _instance = FindObjectOfType<CommersionManager>();
                
                // If still not found, create a new GameObject with this component
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(CommersionManager).Name);
                    _instance = singletonObject.AddComponent<CommersionManager>();
                    
                    // Make it persistent across scenes (optional)
                    DontDestroyOnLoad(singletonObject);
                }
            }
            
            return _instance;
        }
    }

    private GameObject currentLegalPrefab;
    [SerializeField]private GameObject loaderPrefab;
    
    private void Awake()
    {
        // Ensure only one instance exists
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            // If another instance already exists, destroy this one
            Destroy(gameObject);
        }
    }
    
    private CommersionSettings _settings;

    public void Initialize()
    {
        _settings = Resources.Load<CommersionSettings>("Commersion/CommersionSettings");
        loaderPrefab = Resources.Load<GameObject>("Commersion/Loader");
        var taskRunner = Resources.Load<GameObject>("Commersion/TaskRunner");
        Instantiate(taskRunner);
        Debug.Log("Commersion initialized!");
    }

    public CommersionSettings GetSettings()
    {
        if (_settings == null)
        {
            Debug.LogError("No settings found");
            return null;
        }
        return _settings;
    }

    public string GetLegalText()
    {
        if (_settings == null)
        {
            Debug.LogError("No settings found");
            return "";
        }
        return _settings.legals.PrivacyText.text;
    }

    public GameObject GetLoaderPrefab()
    {
        if (loaderPrefab == null)
        {
            Debug.LogError("No loader prefab found");
        }
        return loaderPrefab;
    }

    public async Task<bool> ShowLegalPopup()
    {
        if (_settings.saveLegalsLocally && PlayerPrefs.HasKey(Constants.LEGAL_SAVE_KEY))
        {
            return true;
        }
        var popupPrefab = Resources.Load<GameObject>("Commersion/Popup_Legal");
        var legalCanvas = new GameObject("Legal Canvas");
        var scaler = legalCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        legalCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        legalCanvas.AddComponent<GraphicRaycaster>();
        currentLegalPrefab = Instantiate(popupPrefab,legalCanvas.transform);
        var legalScript = currentLegalPrefab.GetComponent<CommersionLegalPopup>();

        if (legalScript == null)
        {
            Debug.LogError("Commersion Legal Popup not found");
            DestroyImmediate(legalCanvas);
            return false;
        }
        else
        {
            var result = await legalScript.GetResult();
            DestroyImmediate(legalCanvas);
            return result;
        }
        
    }
}
}
