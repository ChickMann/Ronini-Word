using System;
using System.Threading.Tasks;
using EF.Database; 
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
    [SerializeField] private TMP_InputField usernameInputField; 
    [SerializeField] private TextMeshProUGUI displayUsernameText; 
    [SerializeField] private Button submitNameButton; 
    
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
    
        UIManager.Instance.SetEnterNamePanel(false);

        bool isRemembered = PlayerPrefs.GetInt(PREF_REMEMBER_ME, 0) == 1;
        if (rememberMeToggle != null) rememberMeToggle.isOn = isRemembered;

        //  Tự động đăng nhập
        if (_auth.CurrentUser != null && isRemembered)
        {
            Debug.Log("⏳ Phát hiện phiên cũ. Đang đợi hệ thống ổn định...");
        
            //  Đợi 1 giây để EFManager và Firebase đồng bộ xong ID
            await Task.Delay(1000); 

           
            if (_auth.CurrentUser != null)
            {
                Debug.Log($" Bắt đầu Auto-login cho User: {_auth.CurrentUser.UserId}");
                HandlePostLogin(_auth.CurrentUser);
            }
            else
            {
                UIManager.Instance.LoginPanel();
            }
        }
        else
        {
            if (GoogleSignIn.DefaultInstance != null) GoogleSignIn.DefaultInstance.SignOut();
            UIManager.Instance.LoginPanel();
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

    //TẢI TÊN 
    private async void HandlePostLogin(FirebaseUser user)
    {
        loginPanel.SetActive(false);
        Debug.Log("Đang kiểm tra Username...");

        try
        {
            //  Tạo kênh dữ liệu
            SimpleData freshChannel = new SimpleData("Username", DataType.String, "Player");

            string debugPath = EFManager.Instance.RePlacePrefix("Player");
            Debug.Log($"---> [CHECK PATH] Code đang đọc tại: {debugPath}/Username");

            // Kiểm tra lỗi chưa lấy được ID
            if (debugPath == "Player/" || debugPath == "Player")
            {
                Debug.LogError("LỖI: Chưa lấy được UserID. Đang thử đợi thêm...");
                await Task.Delay(500); // Đợi thêm chút nữa
            }

            string savedName = await freshChannel.GetData<string>();

            if (string.IsNullOrEmpty(savedName) || savedName == "null")
            {
                Debug.Log("Không tìm thấy tên (hoặc tên rỗng) -> Mở bảng nhập.");
            
                if (usernameInputField != null) 
                    usernameInputField.text = user.DisplayName; 

                UIManager.Instance.SetEnterNamePanel(true);
                userPanel.SetActive(false); 
            }
            else
            {
                Debug.Log($" Đã tìm thấy tên: {savedName}");
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
            Debug.LogError($" Lỗi HandlePostLogin: {ex.Message}");
            if (usernameInputField != null) usernameInputField.text = user.DisplayName;
            UIManager.Instance.SetEnterNamePanel(true);
        }
    }

    // LƯU TÊN 

    private async void OnSubmitUsernameClicked()
    {
        string inputName = usernameInputField.text.Trim();
        if (string.IsNullOrEmpty(inputName)) return;

        submitNameButton.interactable = false;

        try 
        {

            SimpleData freshChannel = new SimpleData("Username", DataType.String, "Player");
            
            Debug.Log($"Đang LƯU vào: {EFManager.Instance.RePlacePrefix("Player")}/Username");

            await freshChannel.SetData(inputName);
            
            _currentCustomName = inputName;

            // Gọi đồng bộ ngay sau khi lưu tên
            if (GameDataManager.Instance != null)
            {
                await GameDataManager.Instance.SyncFromCloud();
            }

            UIManager.Instance.SetEnterNamePanel(false);
            ShowMainUserPanel(_auth.CurrentUser);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Lỗi lưu tên: {ex.Message}");
            submitNameButton.interactable = true;
        }
    }

    private void ShowMainUserPanel(FirebaseUser user)
    {
        if (user == null) return;

        userPanel.SetActive(true);
        UIManager.Instance.SetEnterNamePanel(false);
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
