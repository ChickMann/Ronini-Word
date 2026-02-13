using System;
using System.Threading.Tasks;
using EF.Database; // Tool SimpleData
using EF.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using Google;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoginWithGoogle : MonoBehaviour
{
    [Header("Google API")]
    [SerializeField] private string webClientId = "YOUR_WEB_CLIENT_ID";

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI userEmailText;
    [SerializeField] private TextMeshProUGUI userIdText;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject userPanel;
    [SerializeField] private Image userProfilePic;
    
    [Header("Username System")]
    [SerializeField] private GameObject usernameInputPanel; 
    [SerializeField] private TMP_InputField usernameInputField; 
    [SerializeField] private TextMeshProUGUI displayUsernameText; 
    [SerializeField] private Button submitNameButton; 
    
    // [KHÔNG CẦN QUAN TÂM CẤU HÌNH INSPECTOR NỮA]
    // Chúng ta sẽ tự tạo kênh dữ liệu chuẩn trong code
    [SerializeField] private SimpleData usernameDataChannel; 

    [Header("Settings")]
    [SerializeField] private Toggle rememberMeToggle;

    private FirebaseAuth _auth;
    private GoogleSignInConfiguration _configuration;
    private const string PREF_REMEMBER_ME = "RememberMe";
    private string _currentCustomName = ""; 

    private void Awake()
    {
        _configuration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestIdToken = true,
            RequestEmail = true
        };
        CheckConfiguration();
        
        if (submitNameButton != null)
            submitNameButton.onClick.AddListener(OnSubmitUsernameClicked);
    }

    private void CheckConfiguration()
    {
        if (GoogleSignIn.Configuration == null)
        {
            GoogleSignIn.Configuration = _configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;
        }
    }

    private void Start()
    {
        InitFirebase();
    }

    private async void InitFirebase()
    {
        _auth = FirebaseAuth.DefaultInstance;
    
        if (usernameInputPanel != null) usernameInputPanel.SetActive(false);

        bool isRemembered = PlayerPrefs.GetInt(PREF_REMEMBER_ME, 0) == 1;
        if (rememberMeToggle != null) rememberMeToggle.isOn = isRemembered;

        // Logic Tự động đăng nhập
        if (_auth.CurrentUser != null && isRemembered)
        {
            Debug.Log("⏳ Phát hiện phiên cũ. Đang đợi hệ thống ổn định...");
        
            // [FIX QUAN TRỌNG] Đợi 1 giây để EFManager và Firebase đồng bộ xong ID
            await Task.Delay(1000); 

            // Kiểm tra lại lần nữa cho chắc
            if (_auth.CurrentUser != null)
            {
                Debug.Log($"🔄 Bắt đầu Auto-login cho User: {_auth.CurrentUser.UserId}");
                HandlePostLogin(_auth.CurrentUser);
            }
            else
            {
                ShowLoginPanel();
            }
        }
        else
        {
            if (GoogleSignIn.DefaultInstance != null) GoogleSignIn.DefaultInstance.SignOut();
            ShowLoginPanel();
        }
    }
    public async void SignInWithGoogleAsync()
    {
        if (rememberMeToggle != null)
        {
            PlayerPrefs.SetInt(PREF_REMEMBER_ME, rememberMeToggle.isOn ? 1 : 0);
            PlayerPrefs.Save();
        }

        CheckConfiguration();

        try
        {
            GoogleSignIn.DefaultInstance.SignOut(); 
            Task<GoogleSignInUser> signInTask = GoogleSignIn.DefaultInstance.SignIn();
            GoogleSignInUser googleUser = await signInTask;

            Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
            FirebaseUser newUser = await _auth.SignInWithCredentialAsync(credential);
            
            HandlePostLogin(newUser);
        }
        catch (Exception ex)
        {
            if (!ex.Message.Contains("Canceled")) Debug.LogError($"Lỗi Login: {ex.Message}");
        }
    }

    // --- [FIX] LOGIC TẢI TÊN ---

    private async void HandlePostLogin(FirebaseUser user)
    {
        loginPanel.SetActive(false);
        Debug.Log("🔍 Đang kiểm tra Username...");

        try
        {
            // 1. Tạo kênh dữ liệu
            SimpleData freshChannel = new SimpleData("Username", DataType.String, "Player");

            // [DEBUG] In ra đường dẫn thực tế để kiểm tra
            // EFManager sẽ thay thế chữ "Player" bằng UserID thực tế
            string debugPath = EFManager.Instance.RePlacePrefix("Player");
            Debug.Log($"---> [CHECK PATH] Code đang đọc tại: {debugPath}/Username");

            // Nếu debugPath in ra là "Player/" mà không có ID phía sau -> Lỗi do chưa lấy được ID
            if (debugPath == "Player/" || debugPath == "Player")
            {
                Debug.LogError("❌ LỖI: Chưa lấy được UserID. Đang thử đợi thêm...");
                await Task.Delay(500); // Đợi thêm chút nữa
            }

            // 2. Lấy dữ liệu
            string savedName = await freshChannel.GetData<string>();

            // 3. Kiểm tra
            if (string.IsNullOrEmpty(savedName) || savedName == "null")
            {
                Debug.Log("⚠️ Không tìm thấy tên (hoặc tên rỗng) -> Mở bảng nhập.");
            
                if (usernameInputField != null) 
                    usernameInputField.text = user.DisplayName; 

                usernameInputPanel.SetActive(true);
                userPanel.SetActive(false); 
            }
            else
            {
                Debug.Log($"✅ Đã tìm thấy tên: {savedName}");
                _currentCustomName = savedName;

                if (GameDataManager.Instance != null)
                {
                    await GameDataManager.Instance.SyncFromCloud();
                }

                ShowMainUserPanel(user);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Lỗi HandlePostLogin: {ex.Message}");
            // Fallback
            if (usernameInputField != null) usernameInputField.text = user.DisplayName;
            usernameInputPanel.SetActive(true);
        }
    }

    // --- [FIX] LOGIC LƯU TÊN ---

    private async void OnSubmitUsernameClicked()
    {
        string inputName = usernameInputField.text.Trim();
        if (string.IsNullOrEmpty(inputName)) return;

        submitNameButton.interactable = false;

        try 
        {
            // [FIX CỨNG] Lưu đúng vào chỗ đã Tải ở trên ("Username", "Player")
            SimpleData freshChannel = new SimpleData("Username", DataType.String, "Player");
            
            Debug.Log($"---> Đang LƯU vào: {EFManager.Instance.RePlacePrefix("Player")}/Username");

            await freshChannel.SetData(inputName);
            
            _currentCustomName = inputName;

            // Gọi đồng bộ ngay sau khi lưu tên
            if (GameDataManager.Instance != null)
            {
                await GameDataManager.Instance.SyncFromCloud();
            }

            usernameInputPanel.SetActive(false);
            ShowMainUserPanel(_auth.CurrentUser);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Lỗi lưu tên: {ex.Message}");
            submitNameButton.interactable = true;
        }
    }

    // --- CÁC HÀM UI KHÁC GIỮ NGUYÊN ---

    private void ShowMainUserPanel(FirebaseUser user)
    {
        if (user == null) return;

        userPanel.SetActive(true);
        usernameInputPanel.SetActive(false);
        loginPanel.SetActive(false);

        if (displayUsernameText != null)
        {
            displayUsernameText.text = !string.IsNullOrEmpty(_currentCustomName) 
                ? _currentCustomName 
                : user.DisplayName;
        }

        if (userEmailText != null) userEmailText.text = user.Email;
        if (userIdText != null) userIdText.text = user.UserId;

        if (user.PhotoUrl != null)
        {
            _ = LoadProfileImage(user.PhotoUrl.ToString());
        }
    }

    public void SignOut()
    {
        CheckConfiguration();
        GoogleSignIn.DefaultInstance.SignOut();
        _auth.SignOut();
        
        if (GameDataManager.Instance != null) GameDataManager.Instance.HardReset();

        _currentCustomName = "";
        if (usernameInputField != null) usernameInputField.text = "";

        ShowLoginPanel();
    }

    private void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        userPanel.SetActive(false);
        if (usernameInputPanel != null) usernameInputPanel.SetActive(false);
    }

    private async Task LoadProfileImage(string url)
    {
        using UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        var operation = www.SendWebRequest();
        while (!operation.isDone) await Task.Yield();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            if (userProfilePic != null)
                userProfilePic.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }
}