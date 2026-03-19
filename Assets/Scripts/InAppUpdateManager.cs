using System.Collections;
using Google.Play.AppUpdate;
using Google.Play.Common;
using UnityEngine;



public class InAppUpdateManager : MonoBehaviour
{
    private static InAppUpdateManager _instance;
    
    private AppUpdateManager _appUpdateManager;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() 
    {
        // Chỉ khởi tạo và gọi SDK khi ở trên môi trường Android (không phải Editor)
#if UNITY_ANDROID && !UNITY_EDITOR
        _appUpdateManager = new AppUpdateManager(); 
        StartCoroutine(HandleAppUpdate());
#else
        Debug.Log("[InAppUpdate] Tính năng này bị vô hiệu hóa trong Editor. Vui lòng build ra máy thật để test.");
#endif
    }

    private IEnumerator HandleAppUpdate()
    {
        var updateInfoOperation = _appUpdateManager.GetAppUpdateInfo();
        yield return updateInfoOperation;

        if (!updateInfoOperation.IsSuccessful)
        {
            Debug.LogError($"[InAppUpdate] Lỗi kiểm tra: {updateInfoOperation.Error}");
            yield break;
        }

        var updateInfo = updateInfoOperation.GetResult();

        if (updateInfo.UpdateAvailability is UpdateAvailability.UpdateAvailable)
        {
            var options = AppUpdateOptions.ImmediateAppUpdateOptions(allowAssetPackDeletion: true);
            var updateRequest = _appUpdateManager.StartUpdate(updateInfo, options);
            
            yield return updateRequest;

            Debug.LogWarning("[InAppUpdate] Cập nhật bị hủy hoặc gặp lỗi.");
        }
    }
}