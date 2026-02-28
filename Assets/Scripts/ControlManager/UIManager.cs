using System;
using System.Collections.Generic;
using System.Linq;
using SmallHedge.AudioManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Menu panel References")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject xGameObject;
    [SerializeField] private TextMeshProUGUI totalScoreText;

    [Header("login panel References")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject enterNamePanel;
    [SerializeField] private Button loginButton;
    [SerializeField] private Toggle rememberMeToggle;

    [Header("Feedback panel References")]
    [SerializeField] private GameObject feedbackPanel;

    [Header("level panel References")]
    [SerializeField] private GameObject levelPanel;
    [SerializeField] private List<Button> levelButtons;
    [SerializeField] private GameObject prefabLevelButton;
    [SerializeField] private GameObject prefabNextUpdateButton;
    [SerializeField] private GameObject contentLevelContainer;

    [Header("VocabsLevel panel References")]
    [SerializeField] private GameObject vocabsLevelPanel;
    [SerializeField] private GameObject prefabVocab;
    [SerializeField] private GameObject contentVocabsLevelContainer;
    [SerializeField] private List<GameObject> vocabsObjects;

    [Header("Completed panel References")]
    [SerializeField] private GameObject completedPanel;
    [SerializeField] private TextMeshProUGUI completedScoreText; 
    [SerializeField] private TextMeshProUGUI completedTotalScoreText;
    
    [Header("YourVocabs panel References")]
    [SerializeField] private GameObject yourVocabPanel;
    [SerializeField] private GameObject contentYourVocabsContainer;
    [SerializeField] private List<GameObject> yourVocabsObjects;
    
    [Header("Tutorial panel References")]
    [SerializeField] private GameObject tutorialPanel;
    
    [Header("Social")]
    private const string FaceBookPageId = "61586321263480"; 
    
    [Header("InGame references")]
    [SerializeField] private TextMeshProUGUI scoreText;

    private GameManager gameManager;
    private bool isAudioOn;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
       
        updateScoreText();
        
        if (ScoresManager.Instance != null)
        {
            UpdateScoreHUD(ScoresManager.Instance.LocalTotalScore, ScoresManager.Instance.CurrentWaveScore);
        }
    }

    private void Update()
    {
        updateTotalScoreText();
    }

    #region Menu
    public void PlayButton()
    {
        // [SOUND]
        AudioManager.PlaySound(SoundType.ButtonMenu);
        
        SwitchPanels(menuPanel, levelPanel);
        CreateLevelButtons();
    }

    public void FeedbackButton()
    {
        // [SOUND]
        AudioManager.PlaySound(SoundType.ButtonMenu);
        
        menuPanel.SetActive(false);
        feedbackPanel.SetActive(true);
    }

    public void LogoutButton()
    {
        // [SOUND]
        AudioManager.PlaySound(SoundType.ButtonMenu);
        
        LoginPanel();
        gameManager.loginWithGoogle.SignOut();
    }

    public void AudiosButton()
    {
        // [SOUND]
        AudioManager.PlaySound(SoundType.ButtonMenu);
        
        isAudioOn = !isAudioOn;
        xGameObject.SetActive(!isAudioOn);
        AudioManager.instance.SetMuteMusicAndSound(!isAudioOn); 
    }
    public void YourVocabsButton()
    {
        AudioManager.PlaySound(SoundType.ButtonMenu);
        SwitchPanels(menuPanel, yourVocabPanel);
        CreateYourVocabObjects();
    }

    public void TutorialPanelButton()
    {
        AudioManager.PlaySound(SoundType.ButtonMenu);
        SwitchPanels(menuPanel, tutorialPanel);
    }

    public void BackTutorialButton()
    {
        AudioManager.PlaySound(SoundType.ButtonMenu);
        SwitchPanels(tutorialPanel, menuPanel);
    }

    public void updateTotalScoreText()
    {
        totalScoreText.text = ScoresManager.Instance.LocalTotalScore.ToString();
    }

    public void updateScoreText()
    {
        scoreText.text = ScoresManager.Instance.CurrentWaveScore.ToString();
    }
    #endregion

    #region LoginPanel
    public void LoginPanel()
    {
        menuPanel.SetActive(false);
        loginPanel.SetActive(true);
        SetEnterNamePanel(false);
    }

    public void LoginButton()
    {
        // [SOUND]
        AudioManager.PlaySound(SoundType.ButtonMenu);
        
        gameManager.loginWithGoogle.SignInWithGoogleAsync();
    }
    public void SetEnterNamePanel(bool isActive)
    {
        if (enterNamePanel != null)
            enterNamePanel.SetActive(isActive);
    }
    #endregion

    #region FeedbackPanel
    public void BackFeebbackButton()
    {
        // [SOUND]
        AudioManager.PlaySound(SoundType.ButtonMenu);
        
        SwitchPanels(feedbackPanel, menuPanel);
    }
    #endregion

    #region LevelPanel
    public void BackLevelButton()
    {
        // [SOUND]
        AudioManager.PlaySound(SoundType.ButtonMenu);
        
        SwitchPanels(levelPanel, menuPanel);
    }

    private void CreateLevelButtons()
    {
        if (levelButtons != null)
        {
            foreach (Button btn in levelButtons)
            {
                if (btn != null) Destroy(btn.gameObject);
            }
            levelButtons.Clear();
        }
        else levelButtons = new List<Button>(); 

        if (gameManager.levelDataList == null) return;

        List<LevelData> levelDataList = gameManager.levelDataList;
        int count = levelDataList.Count;

        for (int i = 0; i < count; i++)
        {
            Button btn = Instantiate(prefabLevelButton, contentLevelContainer.transform).GetComponent<Button>();
            LevelData level = levelDataList[i];

            TextMeshProUGUI[] allTexts = btn.GetComponentsInChildren<TextMeshProUGUI>(true);
            TextMeshProUGUI levelText = allTexts.FirstOrDefault(p => p.name == "level text");
            TextMeshProUGUI scoreText = allTexts.FirstOrDefault(p => p.name == "score text");

            if (levelText != null) levelText.text = level.LevelID.ToString();
            if (scoreText != null) scoreText.text = level.scoreRequirement.ToString();

            if (ScoresManager.Instance.LocalTotalScore < level.scoreRequirement)
            {
                btn.interactable = false;
                btn.image.color = new Color(1f, 1f, 1f, 90f / 255f);
            }

            btn.onClick.AddListener(() => VocabsButton(level));
            
            levelButtons.Add(btn);
        }

        if (prefabNextUpdateButton != null)
        {
            Button nextUpdate = Instantiate(prefabNextUpdateButton, contentLevelContainer.transform).GetComponent<Button>();
            nextUpdate.interactable = false;
            levelButtons.Add(nextUpdate);
        }
    }

    public void VocabsButton(LevelData level)
    {
        // [SOUND] - Hàm này được gọi khi bấm chọn Level
        AudioManager.PlaySound(SoundType.ButtonMenu);
        
        SwitchPanels(levelPanel, vocabsLevelPanel);
        CreateVocabObjects(level);
        gameManager.SetCurrentLevel(level);
    }
   
    #endregion

    #region completedPanel

    public void SetActiveCompletedPanel(bool isActive)
    {
        completedPanel.SetActive(isActive);
    }
    
    public void HomeButton()
    {
        AudioManager.PlaySound(SoundType.ButtonMenu);
        
        SwitchPanels(completedPanel, menuPanel);
        gameManager.BackToMenu();
    }

    public void RestartButton()
    {
        // [SOUND]
        AudioManager.PlaySound(SoundType.ButtonMenu);
        
        SetActiveCompletedPanel(false);
        gameManager.RestartLevel();
    }
    

    public void UpdateScoreHUD(int localTotalScore, int currentWaveScore)
    {
        if (completedTotalScoreText) 
            completedTotalScoreText.text = $"{localTotalScore}";

        if (completedScoreText)
        {
            completedScoreText.text = $"{currentWaveScore}";
        }
    }

    #endregion
    
    #region VocabsLevelPanel
    
    public void StartButton()
    {
        AudioManager.PlaySound(SoundType.ButtonMenu);
        
        gameManager.StartLevel();
        vocabsLevelPanel.SetActive(false);
    }
    
    public void BackVocabButton()
    {
        AudioManager.PlaySound(SoundType.ButtonMenu);
        
        SwitchPanels(vocabsLevelPanel, levelPanel);
    }

    private void CreateVocabObjects(LevelData level)
    {
        if (vocabsObjects != null)
        {
            foreach (GameObject vc in vocabsObjects)
            {
                if (vc != null) Destroy(vc.gameObject);
            }
            vocabsObjects.Clear();
        }
        else vocabsObjects = new List<GameObject>(); 
        
        List<VocabData> vocabList = new List<VocabData>();
        foreach (var wave in level.Waves)
        {
            vocabList.AddRange(wave.VocabList);
        }

        for (int i = 0; i < vocabList.Count; i++)
        {
            GameObject vocabObject = Instantiate(prefabVocab, contentVocabsLevelContainer.transform);
            TextMeshProUGUI[] allTexts = vocabObject.GetComponentsInChildren<TextMeshProUGUI>(true);
            TextMeshProUGUI meaningText = allTexts.FirstOrDefault(p => p.name == "meaning text");
            TextMeshProUGUI answerText = allTexts.FirstOrDefault(p => p.name == "answer text");
            
            if(meaningText != null) meaningText.text = vocabList[i].Meaning;
            if(answerText != null) answerText.text = vocabList[i].Answer;
            vocabsObjects.Add(vocabObject);
        }
        
    }
    #endregion

    #region YourVocabsPanel

   
    public void BackYourVocabButton()
    {
        AudioManager.PlaySound(SoundType.ButtonMenu);
        SwitchPanels(yourVocabPanel, menuPanel);
    }
  private void CreateYourVocabObjects()
{
    // 1. DỌN DẸP LIST CŨ
    if (yourVocabsObjects != null)
    {
        foreach (GameObject vc in yourVocabsObjects)
        {
            if (vc != null) Destroy(vc.gameObject);
        }
        yourVocabsObjects.Clear();
    }
    else yourVocabsObjects = new List<GameObject>(); 
    
    // 2. LẤY DỮ LIỆU TỪ DICTIONARY (MỚI)
    // Map gồm: Key là VocabData, Value là Status (int)
    var vocabMap = GameDataManager.Instance.GetVocabMapFromBackup();

    // 3. XỬ LÝ KHI KHÔNG CÓ DỮ LIỆU
    if (vocabMap == null || vocabMap.Count == 0)
    {
        GameObject vocabObject = Instantiate(prefabVocab, contentYourVocabsContainer.transform);
        TextMeshProUGUI[] allTexts = vocabObject.GetComponentsInChildren<TextMeshProUGUI>(true);
        TextMeshProUGUI meaningText = allTexts.FirstOrDefault(p => p.name == "meaning text");
        TextMeshProUGUI answerText = allTexts.FirstOrDefault(p => p.name == "answer text");
        
        if(meaningText != null) meaningText.text = "Bạn chưa có được từ nào cả!";
        if(answerText != null) answerText.text = ":)?";
        
        yourVocabsObjects.Add(vocabObject);
        return; // Thoát hàm luôn, không chạy đoạn dưới nữa
    }

    // 4. DUYỆT QUA TỪNG CẶP (VOCAB + STATUS)
    foreach (var entry in vocabMap)
    {
        VocabData vocab = entry.Key; // Dữ liệu từ vựng
        int status = entry.Value;    // Trạng thái (1: Đã học, 2: Perfect...)

        GameObject vocabObject = Instantiate(prefabVocab, contentYourVocabsContainer.transform);
        
        // Tìm components (vẫn giữ cách cũ của bạn)
        TextMeshProUGUI[] allTexts = vocabObject.GetComponentsInChildren<TextMeshProUGUI>(true);
        TextMeshProUGUI meaningText = allTexts.FirstOrDefault(p => p.name == "meaning text");
        TextMeshProUGUI answerText = allTexts.FirstOrDefault(p => p.name == "answer text");
        
        // Gán dữ liệu
        if(meaningText != null) meaningText.text = vocab.Meaning;
        if(answerText != null) answerText.text = vocab.Answer;


        if (status >= 2)
        {
            if (answerText != null) answerText.color = Color.green; // Màu vàng
            if (meaningText != null)
            {
                meaningText.color = Color.green;
            }
        }
        else
        {
            if (answerText != null) answerText.color = Color.red;
            if (meaningText != null)
            {
                meaningText.color = Color.red;
            }
        }

        yourVocabsObjects.Add(vocabObject);
    }
}

    #endregion

    #region Social

    public void OpenFacebookPage()
    {
        AudioManager.PlaySound(SoundType.ButtonMenu);
#if UNITY_ANDROID
        string facebookUrl = $"fb://page/{FaceBookPageId}";
#elif UNITY_IOS
        string facebookUrl = $"fb://profile/{FaceBookPageId}";
#else
        // Nếu là WebGL hoặc Editor thì mở trình duyệt
        string facebookUrl = $"https://www.facebook.com/{FaceBookPageName}";
#endif

        Application.OpenURL(facebookUrl);
        
        // Log để kiểm tra trong Editor
        Debug.Log($"Đang mở Fanpage: {facebookUrl}");
    }

    #endregion
    
    private void SwitchPanels(GameObject currentPanel, GameObject previousPanel)
    {
        currentPanel.SetActive(false);
        previousPanel.SetActive(true);
    }
    
}