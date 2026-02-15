using System;
using System.Collections.Generic;
using System.Threading.Tasks; 
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.Networking; 
using UnityEngine.UIElements;

public class LeaderboardController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset rowTemplate;

    [Header("Config")]
    [Tooltip("Để trống nếu dữ liệu nằm ngay ở Root.")]
    [SerializeField] private string dataRoot = ""; 
    [SerializeField] private int maxPlayers = 100;

    // UI Elements
    private VisualElement _root;
    private VisualElement _imgAvatar;
    private ScrollView _scrollContainer;
    private Label _lblMyRank, _lblMyName, _lblMyScore;

    // BIẾN LUỒNG MAIN THREAD
    private bool _needsRender = false;
    private List<LeaderboardItem> _dataToRender = new List<LeaderboardItem>();
    
    [System.Serializable]
    public class LeaderboardItem
    {
        public string UserId;
        public string Name;
        public long Score;
    }

    private void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        _root = uiDocument.rootVisualElement;
        
        _scrollContainer = _root.Q<ScrollView>("ScrollContainer");
        _lblMyRank = _root.Q<Label>("lblMyRank");
        _lblMyName = _root.Q<Label>("lblMyName");
        _lblMyScore = _root.Q<Label>("lblMyScore");
        _imgAvatar = _root.Q<VisualElement>("imgAvatar");

        LoadLeaderboard();
        LoadCurrentUserAvatar();
    }

    private void Update()
    {
        if (_needsRender)
        {
            _needsRender = false;
            RenderInternal(_dataToRender);
        }
    }

    // --- PHẦN 1: TẢI BẢNG XẾP HẠNG ---

    public void LoadLeaderboard()
    {
        _scrollContainer.Clear();
        UpdateMyRankUI("...", "Loading...", 0);
        
        DatabaseReference queryRef;
        if (string.IsNullOrEmpty(dataRoot))
            queryRef = FirebaseDatabase.DefaultInstance.RootReference;
        else
            queryRef = FirebaseDatabase.DefaultInstance.GetReference(dataRoot);

        Debug.Log($"🚀 [Leaderboard] Đang tải từ: {(string.IsNullOrEmpty(dataRoot) ? "ROOT" : dataRoot)}");

        queryRef.OrderByChild("TotalScore")
            .LimitToLast(maxPlayers)
            .GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("❌ Lỗi mạng: " + task.Exception);
                return;
            }

            DataSnapshot snapshot = task.Result;
            List<LeaderboardItem> tempList = new List<LeaderboardItem>();

            if (snapshot.Exists)
            {
                foreach (var child in snapshot.Children)
                {
                    try 
                    {
                        // [QUAN TRỌNG - FIX LỖI UNKNOWN]
                        // Chỉ lấy những node có chứa Username.
                        // Nếu không có Username -> Đây là node rác/folder hệ thống -> Bỏ qua ngay
                        if (!child.HasChild("Username")) 
                        {
                            // Debug.LogWarning($"⚠️ Bỏ qua node rác: {child.Key}");
                            continue;
                        }

                        string uName = $"{child.Child("Username").Value}";
                        long uScore = 0;
                        
                        if (child.HasChild("TotalScore")) 
                            long.TryParse($"{child.Child("TotalScore").Value}", out uScore);

                        tempList.Add(new LeaderboardItem { UserId = child.Key, Name = uName, Score = uScore });
                    }
                    catch { /* Bỏ qua */ }
                }
                tempList.Reverse(); // Cao -> Thấp
            }

            _dataToRender = tempList;
            _needsRender = true; 
        });
    }

    private void RenderInternal(List<LeaderboardItem> items)
    {
        _scrollContainer.Clear();
        string myId = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        bool foundMe = false;

        Debug.Log($"🎨 Vẽ {items.Count} người chơi lên bảng.");

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            int rank = i + 1;

            TemplateContainer row = rowTemplate.Instantiate();
            row.style.flexShrink = 0; 

            var lblRank = row.Q<Label>("lblRank");
            var lblName = row.Q<Label>("lblName");
            var lblScore = row.Q<Label>("lblScore");

            if (lblRank != null) lblRank.text = $"{rank}.";
            if (lblName != null) lblName.text = item.Name;
            if (lblScore != null) lblScore.text = item.Score.ToString();

            if (!string.IsNullOrEmpty(myId) && item.UserId == myId)
            {
                var container = row.Q<VisualElement>("RowContainer");
                if (container != null) 
                    container.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.2f, 0.5f));
                
                UpdateMyRankUI($"#{rank}", item.Name, item.Score);
                foundMe = true;
            }

            _scrollContainer.Add(row);
        }

        if (!foundMe && !string.IsNullOrEmpty(myId))
        {
            // Nếu không tìm thấy trong list, tải riêng lẻ
            FetchMyDataFallback(myId);
        }
    }

    // --- Hàm tải riêng cho bản thân nếu nằm ngoài top ---
    private async void FetchMyDataFallback(string uid)
    {
        try 
        {
            DatabaseReference refDb;
            if (string.IsNullOrEmpty(dataRoot)) refDb = FirebaseDatabase.DefaultInstance.RootReference.Child(uid);
            else refDb = FirebaseDatabase.DefaultInstance.GetReference(dataRoot).Child(uid);

            var snapshot = await refDb.GetValueAsync();

            if (snapshot.Exists && snapshot.HasChild("TotalScore"))
            {
                string name = snapshot.Child("Username").Value.ToString();
                long score = long.Parse(snapshot.Child("TotalScore").Value.ToString());
                UpdateMyRankUI("100+", name, score);
            }
            else
            {
                UpdateMyRankUI("---", "Chưa có hạng", 0);
            }
        }
        catch { /* Ignore */ }
    }

    private void UpdateMyRankUI(string rank, string name, long score)
    {
        if (_lblMyRank != null) _lblMyRank.text = rank;
        if (_lblMyName != null) _lblMyName.text = name;
        if (_lblMyScore != null) _lblMyScore.text = score.ToString();
    }

    // --- PHẦN 2: TẢI ẢNH GOOGLE AVATAR ---

    private async void LoadCurrentUserAvatar()
    {
        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;

        if (user == null || user.PhotoUrl == null || _imgAvatar == null) return;

        string url = user.PhotoUrl.ToString();
        
        try
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
            {
                var asyncOp = uwr.SendWebRequest();
                while (!asyncOp.isDone) await Task.Yield();

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                    _imgAvatar.style.backgroundImage = new StyleBackground(tex);
                    _imgAvatar.style.backgroundColor = Color.clear;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("❌ Lỗi Avatar: " + ex.Message);
        }
    }
}