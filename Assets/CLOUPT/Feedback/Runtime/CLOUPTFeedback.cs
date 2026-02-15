using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using CLOUPT.Core;

namespace CLOUPT.Feedback
{
    /// <summary>
    /// CLOUPT Feedback Client - Collects and sends player feedback to CLOUPT servers.
    /// Supports bug reports, feature requests, crash reports, and general feedback.
    /// </summary>
    public class CLOUPTFeedback : MonoBehaviour
    {
        private const string FEEDBACK_ENDPOINT = "/api/v1/feedback";
        private const string API_BASE_URL = "https://api.cloupt.com";

        private static CLOUPTFeedback _instance;
        private Queue<FeedbackRequest> _offlineQueue = new Queue<FeedbackRequest>();
        private bool _isProcessingQueue = false;

        /// <summary>
        /// Singleton instance of the Feedback Client.
        /// </summary>
        public static CLOUPTFeedback Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[CLOUPT Feedback]");
                    _instance = go.AddComponent<CLOUPTFeedback>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets whether the feedback system is properly configured.
        /// </summary>
        public bool IsConfigured => CLOUPTSettings.Instance != null && CLOUPTSettings.Instance.IsValid();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        #region Public API

        /// <summary>
        /// Submits general feedback with an optional rating.
        /// </summary>
        /// <param name="message">Feedback message (5-5000 characters)</param>
        /// <param name="rating">Rating 1-5</param>
        /// <param name="onSuccess">Callback on successful submission</param>
        /// <param name="onError">Callback on error</param>
        public void SubmitFeedback(string message, int rating = 0, Action<FeedbackResponse> onSuccess = null, Action<FeedbackError> onError = null)
        {
            Submit(FeedbackType.Feedback, message, rating, null, null, FeedbackPriority.Medium, onSuccess, onError);
        }

        /// <summary>
        /// Submits a bug report.
        /// </summary>
        /// <param name="message">Bug description (5-5000 characters)</param>
        /// <param name="onSuccess">Callback on successful submission</param>
        /// <param name="onError">Callback on error</param>
        public void SubmitBugReport(string message, Action<FeedbackResponse> onSuccess = null, Action<FeedbackError> onError = null)
        {
            Submit(FeedbackType.Bug, message, 0, null, null, FeedbackPriority.Medium, onSuccess, onError);
        }

        /// <summary>
        /// Submits a bug report with additional logs.
        /// </summary>
        /// <param name="message">Bug description</param>
        /// <param name="logs">Array of log strings (max 50 entries)</param>
        /// <param name="onSuccess">Callback on successful submission</param>
        /// <param name="onError">Callback on error</param>
        public void SubmitBugReport(string message, string[] logs, Action<FeedbackResponse> onSuccess = null, Action<FeedbackError> onError = null)
        {
            var metadata = new FeedbackMetadata { logs = logs };
            Submit(FeedbackType.Bug, message, 0, metadata, null, FeedbackPriority.Medium, onSuccess, onError);
        }

        /// <summary>
        /// Submits a feature request.
        /// </summary>
        /// <param name="message">Feature request description (5-5000 characters)</param>
        /// <param name="onSuccess">Callback on successful submission</param>
        /// <param name="onError">Callback on error</param>
        public void SubmitFeatureRequest(string message, Action<FeedbackResponse> onSuccess = null, Action<FeedbackError> onError = null)
        {
            Submit(FeedbackType.Feature, message, 0, null, null, FeedbackPriority.Medium, onSuccess, onError);
        }

        /// <summary>
        /// Submits a suggestion.
        /// </summary>
        /// <param name="message">Suggestion description (5-5000 characters)</param>
        /// <param name="onSuccess">Callback on successful submission</param>
        /// <param name="onError">Callback on error</param>
        public void SubmitSuggestion(string message, Action<FeedbackResponse> onSuccess = null, Action<FeedbackError> onError = null)
        {
            Submit(FeedbackType.Suggestion, message, 0, null, null, FeedbackPriority.Medium, onSuccess, onError);
        }

        /// <summary>
        /// Submits a crash report.
        /// </summary>
        /// <param name="exception">The exception that caused the crash</param>
        /// <param name="onSuccess">Callback on successful submission</param>
        /// <param name="onError">Callback on error</param>
        public void SubmitCrashReport(Exception exception, Action<FeedbackResponse> onSuccess = null, Action<FeedbackError> onError = null)
        {
            string message = $"{exception.GetType().Name}: {exception.Message}";
            var logs = new string[]
            {
                $"[EXCEPTION] {exception.GetType().FullName}: {exception.Message}",
                $"[STACKTRACE] {exception.StackTrace}"
            };
            var metadata = new FeedbackMetadata { logs = logs };
            Submit(FeedbackType.Crash, message, 0, metadata, null, FeedbackPriority.Critical, onSuccess, onError);
        }

        /// <summary>
        /// Submits a crash report with custom message and logs.
        /// </summary>
        /// <param name="message">Crash description</param>
        /// <param name="logs">Stack trace or log entries</param>
        /// <param name="onSuccess">Callback on successful submission</param>
        /// <param name="onError">Callback on error</param>
        public void SubmitCrashReport(string message, string[] logs, Action<FeedbackResponse> onSuccess = null, Action<FeedbackError> onError = null)
        {
            var metadata = new FeedbackMetadata { logs = logs };
            Submit(FeedbackType.Crash, message, 0, metadata, null, FeedbackPriority.Critical, onSuccess, onError);
        }

        /// <summary>
        /// Submits feedback with full control over all parameters.
        /// </summary>
        /// <param name="type">Type of feedback</param>
        /// <param name="message">Feedback message</param>
        /// <param name="rating">Rating (only for Feedback type)</param>
        /// <param name="customMetadata">Custom metadata to include</param>
        /// <param name="header">Optional header/title for the feedback</param>
        /// <param name="priority">Priority level of the feedback</param>
        /// <param name="onSuccess">Callback on successful submission</param>
        /// <param name="onError">Callback on error</param>
        public void Submit(FeedbackType type, string message, int rating = 0, FeedbackMetadata customMetadata = null, string header = null, FeedbackPriority priority = FeedbackPriority.Medium, Action<FeedbackResponse> onSuccess = null, Action<FeedbackError> onError = null)
        {
            if (!ValidateInput(message, onError))
                return;

            var request = BuildRequest(type, message, rating, customMetadata, header, priority);
            StartCoroutine(SendFeedback(request, onSuccess, onError));
        }

        /// <summary>
        /// Submits feedback with a screenshot attached.
        /// </summary>
        /// <param name="type">Type of feedback</param>
        /// <param name="message">Feedback message</param>
        /// <param name="screenshot">Screenshot texture to attach</param>
        /// <param name="rating">Rating (only for Feedback type)</param>
        /// <param name="header">Optional header/title for the feedback</param>
        /// <param name="priority">Priority level of the feedback</param>
        /// <param name="onSuccess">Callback on successful submission</param>
        /// <param name="onError">Callback on error</param>
        public void SubmitWithScreenshot(FeedbackType type, string message, Texture2D screenshot, int rating = 0, string header = null, FeedbackPriority priority = FeedbackPriority.Medium, Action<FeedbackResponse> onSuccess = null, Action<FeedbackError> onError = null)
        {
            if (!ValidateInput(message, onError))
                return;

            var metadata = new FeedbackMetadata();
            
            if (screenshot != null)
            {
                Log($"Processing screenshot: {screenshot.width}x{screenshot.height}");
                byte[] pngData = screenshot.EncodeToPNG();
                if (pngData.Length <= 500 * 1024) // 500KB limit
                {
                    metadata.screenshot = $"data:image/png;base64,{Convert.ToBase64String(pngData)}";
                    Log($"Screenshot attached: {pngData.Length / 1024}KB");
                }
                else
                {
                    LogWarning($"Screenshot too large ({pngData.Length / 1024}KB > 500KB), skipping attachment.");
                }
            }

            var request = BuildRequest(type, message, rating, metadata, header, priority);
            StartCoroutine(SendFeedback(request, onSuccess, onError));
        }

        /// <summary>
        /// Captures a screenshot and submits feedback.
        /// </summary>
        /// <param name="type">Type of feedback</param>
        /// <param name="message">Feedback message</param>
        /// <param name="rating">Rating (only for Feedback type)</param>
        /// <param name="header">Optional header/title for the feedback</param>
        /// <param name="priority">Priority level of the feedback</param>
        /// <param name="onSuccess">Callback on successful submission</param>
        /// <param name="onError">Callback on error</param>
        public void SubmitWithScreenshotCapture(FeedbackType type, string message, int rating = 0, string header = null, FeedbackPriority priority = FeedbackPriority.Medium, Action<FeedbackResponse> onSuccess = null, Action<FeedbackError> onError = null)
        {
            StartCoroutine(CaptureAndSubmit(type, message, rating, header, priority, onSuccess, onError));
        }

        #endregion

        #region Internal Methods

        private bool ValidateInput(string message, Action<FeedbackError> onError)
        {
            if (!IsConfigured)
            {
                LogError("CLOUPT SDK is not configured. Please set your App ID in Tools > CLOUPT > Setup.");
                onError?.Invoke(new FeedbackError("INVALID_APP_ID", "SDK not configured"));
                return false;
            }

            if (string.IsNullOrEmpty(message) || message.Length < 5)
            {
                LogError("Message too short. Minimum 5 characters required.");
                onError?.Invoke(new FeedbackError("INVALID_MESSAGE", "Message too short"));
                return false;
            }

            if (message.Length > 5000)
            {
                LogError("Message too long. Maximum 5000 characters allowed.");
                onError?.Invoke(new FeedbackError("MESSAGE_TOO_LONG", "Message exceeds 5000 characters"));
                return false;
            }

            return true;
        }

        private FeedbackRequest BuildRequest(FeedbackType type, string message, int rating, FeedbackMetadata customMetadata, string header = null, FeedbackPriority priority = FeedbackPriority.Medium)
        {
            var metadata = customMetadata ?? new FeedbackMetadata();

            // Auto-fill device information
            metadata.platform = GetPlatformString();
            metadata.os = SystemInfo.operatingSystem;
            metadata.gameVersion = Application.version;
            metadata.deviceModel = SystemInfo.deviceModel;
            metadata.screenResolution = $"{Screen.width}x{Screen.height}";
            metadata.memoryUsage = $"{SystemInfo.systemMemorySize}MB";
            metadata.locale = Application.systemLanguage.ToString();

            try
            {
                metadata.level = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            }
            catch
            {
                metadata.level = "Unknown";
            }

            return new FeedbackRequest
            {
                appId = CLOUPTSettings.Instance.PublicAppId,
                deviceId = GetDeviceId(),
                type = GetTypeString(type),
                message = message,
                header = header ?? "",
                rating = type == FeedbackType.Feedback ? rating : 0,
                priority = GetPriorityString(priority),
                metadata = metadata
            };
        }

        private IEnumerator SendFeedback(FeedbackRequest request, Action<FeedbackResponse> onSuccess, Action<FeedbackError> onError)
        {
            string url = API_BASE_URL + FEEDBACK_ENDPOINT;
            string json = JsonUtility.ToJson(request);

            LogDebugRequest(request, json);

            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.timeout = 30;

                yield return webRequest.SendWebRequest();

                string responseBody = webRequest.downloadHandler?.text ?? "";
                LogDebugResponse(webRequest.responseCode, responseBody, webRequest.result == UnityWebRequest.Result.Success);

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<FeedbackResponse>(responseBody);
                    Log($"Feedback submitted successfully. ID: {response.feedbackId}");
                    onSuccess?.Invoke(response);
                }
                else
                {
                    FeedbackError error;

                    try
                    {
                        var errorResponse = JsonUtility.FromJson<FeedbackErrorResponse>(responseBody);
                        error = new FeedbackError(errorResponse.code, errorResponse.error);
                    }
                    catch
                    {
                        error = new FeedbackError("NETWORK_ERROR", webRequest.error);
                    }

                    LogError($"Feedback submission failed: {error.message}");
                    onError?.Invoke(error);

                    // Queue for offline retry if network error
                    if (webRequest.result == UnityWebRequest.Result.ConnectionError)
                    {
                        _offlineQueue.Enqueue(request);
                        Log("Feedback queued for offline retry.");
                    }
                }
            }
        }

        private IEnumerator CaptureAndSubmit(FeedbackType type, string message, int rating, string header, FeedbackPriority priority, Action<FeedbackResponse> onSuccess, Action<FeedbackError> onError)
        {
            yield return new WaitForEndOfFrame();

            Log($"Capturing screenshot at end of frame...");

            Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();

            Log($"Screenshot captured: {screenshot.width}x{screenshot.height}");

            // Scale down if too large
            if (screenshot.width > 1280)
            {
                float ratio = 1280f / screenshot.width;
                int newHeight = Mathf.RoundToInt(screenshot.height * ratio);
                Log($"Scaling screenshot from {screenshot.width}x{screenshot.height} to 1280x{newHeight}");
                screenshot = ScaleTexture(screenshot, 1280, newHeight);
            }

            SubmitWithScreenshot(type, message, screenshot, rating, header, priority, onSuccess, onError);

            Destroy(screenshot);
        }

        private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
            Graphics.Blit(source, rt);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D result = new Texture2D(targetWidth, targetHeight);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            return result;
        }

        /// <summary>
        /// Attempts to send any queued offline feedback.
        /// </summary>
        public void ProcessOfflineQueue()
        {
            if (_isProcessingQueue || _offlineQueue.Count == 0)
                return;

            Log($"Processing offline queue: {_offlineQueue.Count} items pending");
            StartCoroutine(ProcessQueue());
        }

        private IEnumerator ProcessQueue()
        {
            _isProcessingQueue = true;

            while (_offlineQueue.Count > 0)
            {
                var request = _offlineQueue.Peek();
                bool success = false;

                Log($"Retrying offline feedback: {request.type}");

                yield return SendFeedback(request,
                    response => { success = true; },
                    error => { success = false; });

                if (success)
                {
                    _offlineQueue.Dequeue();
                    Log($"Offline feedback sent successfully. Remaining: {_offlineQueue.Count}");
                }
                else
                {
                    LogWarning($"Offline feedback still failing. Will retry later.");
                    break; // Stop if still failing
                }

                yield return new WaitForSeconds(1f);
            }

            _isProcessingQueue = false;
        }

        private string GetDeviceId()
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            
            // Fallback if device ID is not available
            if (string.IsNullOrEmpty(deviceId) || deviceId == "n/a")
            {
                deviceId = PlayerPrefs.GetString("CLOUPT_DeviceId", "");
                if (string.IsNullOrEmpty(deviceId))
                {
                    deviceId = Guid.NewGuid().ToString();
                    PlayerPrefs.SetString("CLOUPT_DeviceId", deviceId);
                    PlayerPrefs.Save();
                }
            }

            return deviceId;
        }

        private string GetPlatformString()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer: return "ios";
                case RuntimePlatform.Android: return "android";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor: return "windows";
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor: return "mac";
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor: return "linux";
                case RuntimePlatform.WebGLPlayer: return "web";
                default: return Application.platform.ToString().ToLower();
            }
        }

        private string GetTypeString(FeedbackType type)
        {
            switch (type)
            {
                case FeedbackType.Bug: return "bug";
                case FeedbackType.Feedback: return "feedback";
                case FeedbackType.Feature: return "feature";
                case FeedbackType.Crash: return "crash";
                case FeedbackType.Suggestion: return "suggestion";
                default: return "feedback";
            }
        }

        private string GetPriorityString(FeedbackPriority priority)
        {
            switch (priority)
            {
                case FeedbackPriority.Low: return "low";
                case FeedbackPriority.Medium: return "medium";
                case FeedbackPriority.High: return "high";
                case FeedbackPriority.Critical: return "critical";
                default: return "medium";
            }
        }

        #endregion

        #region Logging

        private bool IsDebugMode => CLOUPTSettings.Instance != null && CLOUPTSettings.Instance.DebugMode;

        private void Log(string message)
        {
            if (IsDebugMode)
            {
                Debug.Log($"[CLOUPT Feedback] {message}");
            }
        }

        private void LogWarning(string message)
        {
            if (IsDebugMode)
            {
                Debug.LogWarning($"[CLOUPT Feedback] {message}");
            }
        }

        private void LogError(string message)
        {
            Debug.LogError($"[CLOUPT Feedback] {message}");
        }

        private void LogDebugRequest(FeedbackRequest request, string json)
        {
            if (!IsDebugMode) return;
            
            Debug.Log($"[CLOUPT Feedback] ======================================");
            Debug.Log($"[CLOUPT Feedback] SENDING FEEDBACK REQUEST");
            Debug.Log($"[CLOUPT Feedback] Type: {request.type}");
            Debug.Log($"[CLOUPT Feedback] Header: {(string.IsNullOrEmpty(request.header) ? "(none)" : request.header)}");
            Debug.Log($"[CLOUPT Feedback] Message: {request.message.Substring(0, Mathf.Min(100, request.message.Length))}{(request.message.Length > 100 ? "..." : "")}");
            Debug.Log($"[CLOUPT Feedback] Priority: {request.priority}");
            Debug.Log($"[CLOUPT Feedback] Rating: {request.rating}");
            Debug.Log($"[CLOUPT Feedback] Device ID: {request.deviceId}");
            Debug.Log($"[CLOUPT Feedback] Platform: {request.metadata?.platform}");
            Debug.Log($"[CLOUPT Feedback] Has Screenshot: {!string.IsNullOrEmpty(request.metadata?.screenshot)}");
            Debug.Log($"[CLOUPT Feedback] JSON Size: {json.Length} bytes");
            Debug.Log($"[CLOUPT Feedback] ======================================");
        }

        private void LogDebugResponse(long responseCode, string responseBody, bool success)
        {
            if (!IsDebugMode) return;
            
            Debug.Log($"[CLOUPT Feedback] ======================================");
            Debug.Log($"[CLOUPT Feedback] {(success ? "RESPONSE SUCCESS" : "RESPONSE ERROR")}");
            Debug.Log($"[CLOUPT Feedback] Status Code: {responseCode}");
            Debug.Log($"[CLOUPT Feedback] Response: {responseBody}");
            Debug.Log($"[CLOUPT Feedback] ======================================");
        }

        #endregion
    }

    #region Enums & Data Classes

    /// <summary>
    /// Types of feedback that can be submitted.
    /// </summary>
    public enum FeedbackType
    {
        Bug,
        Feedback,
        Feature,
        Crash,
        Suggestion
    }

    /// <summary>
    /// Priority levels for feedback submissions.
    /// </summary>
    public enum FeedbackPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    /// <summary>
    /// Feedback request payload.
    /// </summary>
    [Serializable]
    public class FeedbackRequest
    {
        public string appId;
        public string deviceId;
        public string type;
        public string message;
        public string header;
        public int rating;
        public string priority;
        public FeedbackMetadata metadata;
    }

    /// <summary>
    /// Optional metadata for feedback submissions.
    /// </summary>
    [Serializable]
    public class FeedbackMetadata
    {
        public string platform;
        public string os;
        public string gameVersion;
        public string deviceModel;
        public string locale;
        public string screenResolution;
        public string memoryUsage;
        public int playTime;
        public string level;
        public string sessionId;
        public string[] logs;
        public string screenshot;
        // Note: customData is not directly serializable with JsonUtility
    }

    /// <summary>
    /// Successful feedback submission response.
    /// </summary>
    [Serializable]
    public class FeedbackResponse
    {
        public bool success;
        public string feedbackId;
    }

    /// <summary>
    /// Feedback submission error.
    /// </summary>
    [Serializable]
    public class FeedbackError
    {
        public string code;
        public string message;

        public FeedbackError(string code, string message)
        {
            this.code = code;
            this.message = message;
        }

        public override string ToString()
        {
            return $"[{code}] {message}";
        }
    }

    /// <summary>
    /// API error response structure.
    /// </summary>
    [Serializable]
    internal class FeedbackErrorResponse
    {
        public bool success;
        public string error;
        public string code;
    }

    #endregion
}
