using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace CLOUPT.Core
{
    /// <summary>
    /// CLOUPT API Client - Main interface for making API requests.
    /// Handles authentication, request building, and response parsing.
    /// </summary>
    public class CLOUPTClient : MonoBehaviour
    {
        private const string API_BASE_URL = "https://api.cloupt.com";
        private const string APP_ID_HEADER = "X-CLOUPT-App-Id";
        private const string CONTENT_TYPE_HEADER = "Content-Type";
        private const string CONTENT_TYPE_JSON = "application/json";

        private static CLOUPTClient _instance;

        /// <summary>
        /// Singleton instance of the CLOUPT Client.
        /// Auto-creates a GameObject if not present in scene.
        /// </summary>
        public static CLOUPTClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[CLOUPT Client]");
                    _instance = go.AddComponent<CLOUPTClient>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets whether the client is properly configured.
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

        #region Public API Methods

        /// <summary>
        /// Performs a GET request to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">API endpoint (e.g., "/api/v1/users")</param>
        /// <param name="onSuccess">Callback with response body on success</param>
        /// <param name="onError">Callback with error message on failure</param>
        public void Get(string endpoint, Action<string> onSuccess, Action<CLOUPTError> onError = null)
        {
            StartCoroutine(SendRequest(endpoint, "GET", null, onSuccess, onError));
        }

        /// <summary>
        /// Performs a POST request to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">API endpoint (e.g., "/api/v1/users")</param>
        /// <param name="jsonBody">JSON string body to send</param>
        /// <param name="onSuccess">Callback with response body on success</param>
        /// <param name="onError">Callback with error message on failure</param>
        public void Post(string endpoint, string jsonBody, Action<string> onSuccess, Action<CLOUPTError> onError = null)
        {
            StartCoroutine(SendRequest(endpoint, "POST", jsonBody, onSuccess, onError));
        }

        /// <summary>
        /// Performs a POST request with an object that will be serialized to JSON.
        /// </summary>
        /// <typeparam name="T">Type of the request body object</typeparam>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="body">Object to serialize and send</param>
        /// <param name="onSuccess">Callback with response body on success</param>
        /// <param name="onError">Callback with error message on failure</param>
        public void Post<T>(string endpoint, T body, Action<string> onSuccess, Action<CLOUPTError> onError = null)
        {
            string jsonBody = JsonUtility.ToJson(body);
            Post(endpoint, jsonBody, onSuccess, onError);
        }

        /// <summary>
        /// Performs a PUT request to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="jsonBody">JSON string body to send</param>
        /// <param name="onSuccess">Callback with response body on success</param>
        /// <param name="onError">Callback with error message on failure</param>
        public void Put(string endpoint, string jsonBody, Action<string> onSuccess, Action<CLOUPTError> onError = null)
        {
            StartCoroutine(SendRequest(endpoint, "PUT", jsonBody, onSuccess, onError));
        }

        /// <summary>
        /// Performs a PUT request with an object that will be serialized to JSON.
        /// </summary>
        /// <typeparam name="T">Type of the request body object</typeparam>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="body">Object to serialize and send</param>
        /// <param name="onSuccess">Callback with response body on success</param>
        /// <param name="onError">Callback with error message on failure</param>
        public void Put<T>(string endpoint, T body, Action<string> onSuccess, Action<CLOUPTError> onError = null)
        {
            string jsonBody = JsonUtility.ToJson(body);
            Put(endpoint, jsonBody, onSuccess, onError);
        }

        /// <summary>
        /// Performs a DELETE request to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="onSuccess">Callback with response body on success</param>
        /// <param name="onError">Callback with error message on failure</param>
        public void Delete(string endpoint, Action<string> onSuccess, Action<CLOUPTError> onError = null)
        {
            StartCoroutine(SendRequest(endpoint, "DELETE", null, onSuccess, onError));
        }

        /// <summary>
        /// Performs a PATCH request to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="jsonBody">JSON string body to send</param>
        /// <param name="onSuccess">Callback with response body on success</param>
        /// <param name="onError">Callback with error message on failure</param>
        public void Patch(string endpoint, string jsonBody, Action<string> onSuccess, Action<CLOUPTError> onError = null)
        {
            StartCoroutine(SendRequest(endpoint, "PATCH", jsonBody, onSuccess, onError));
        }

        #endregion

        #region Generic Response Methods

        /// <summary>
        /// Performs a GET request and deserializes the response to the specified type.
        /// </summary>
        /// <typeparam name="TResponse">Type to deserialize response to</typeparam>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="onSuccess">Callback with deserialized response on success</param>
        /// <param name="onError">Callback with error message on failure</param>
        public void Get<TResponse>(string endpoint, Action<TResponse> onSuccess, Action<CLOUPTError> onError = null)
        {
            Get(endpoint, 
                json => {
                    try
                    {
                        TResponse response = JsonUtility.FromJson<TResponse>(json);
                        onSuccess?.Invoke(response);
                    }
                    catch (Exception ex)
                    {
                        LogError($"Failed to parse response: {ex.Message}");
                        onError?.Invoke(new CLOUPTError(-1, $"Parse error: {ex.Message}"));
                    }
                }, 
                onError);
        }

        /// <summary>
        /// Performs a POST request and deserializes the response to the specified type.
        /// </summary>
        /// <typeparam name="TRequest">Type of the request body</typeparam>
        /// <typeparam name="TResponse">Type to deserialize response to</typeparam>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="body">Request body object</param>
        /// <param name="onSuccess">Callback with deserialized response on success</param>
        /// <param name="onError">Callback with error message on failure</param>
        public void Post<TRequest, TResponse>(string endpoint, TRequest body, Action<TResponse> onSuccess, Action<CLOUPTError> onError = null)
        {
            string jsonBody = JsonUtility.ToJson(body);
            Post(endpoint, jsonBody,
                json => {
                    try
                    {
                        TResponse response = JsonUtility.FromJson<TResponse>(json);
                        onSuccess?.Invoke(response);
                    }
                    catch (Exception ex)
                    {
                        LogError($"Failed to parse response: {ex.Message}");
                        onError?.Invoke(new CLOUPTError(-1, $"Parse error: {ex.Message}"));
                    }
                },
                onError);
        }

        #endregion

        #region Internal Request Handler

        /// <summary>
        /// Internal coroutine that handles all HTTP requests.
        /// </summary>
        private IEnumerator SendRequest(string endpoint, string method, string jsonBody, Action<string> onSuccess, Action<CLOUPTError> onError)
        {
            // Validate configuration
            if (!IsConfigured)
            {
                string error = "CLOUPT SDK is not configured. Please set your App ID in Window > CLOUPT > Setup.";
                LogError(error);
                onError?.Invoke(new CLOUPTError(0, error));
                yield break;
            }

            string url = API_BASE_URL + endpoint;
            Log($"[{method}] {endpoint}");

            using (UnityWebRequest request = CreateRequest(url, method, jsonBody))
            {
                // Add authentication header
                request.SetRequestHeader(APP_ID_HEADER, CLOUPTSettings.Instance.PublicAppId);
                request.SetRequestHeader(CONTENT_TYPE_HEADER, CONTENT_TYPE_JSON);

                yield return request.SendWebRequest();

                HandleResponse(request, onSuccess, onError);
            }
        }

        /// <summary>
        /// Creates a UnityWebRequest with the appropriate configuration.
        /// </summary>
        private UnityWebRequest CreateRequest(string url, string method, string jsonBody)
        {
            UnityWebRequest request;

            switch (method.ToUpper())
            {
                case "GET":
                    request = UnityWebRequest.Get(url);
                    break;

                case "POST":
                    request = new UnityWebRequest(url, "POST");
                    if (!string.IsNullOrEmpty(jsonBody))
                    {
                        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    }
                    request.downloadHandler = new DownloadHandlerBuffer();
                    break;

                case "PUT":
                    request = new UnityWebRequest(url, "PUT");
                    if (!string.IsNullOrEmpty(jsonBody))
                    {
                        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    }
                    request.downloadHandler = new DownloadHandlerBuffer();
                    break;

                case "DELETE":
                    request = UnityWebRequest.Delete(url);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    break;

                case "PATCH":
                    request = new UnityWebRequest(url, "PATCH");
                    if (!string.IsNullOrEmpty(jsonBody))
                    {
                        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    }
                    request.downloadHandler = new DownloadHandlerBuffer();
                    break;

                default:
                    request = UnityWebRequest.Get(url);
                    break;
            }

            request.timeout = 30;
            return request;
        }

        /// <summary>
        /// Handles the response from a completed request.
        /// </summary>
        private void HandleResponse(UnityWebRequest request, Action<string> onSuccess, Action<CLOUPTError> onError)
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseBody = request.downloadHandler?.text ?? "";
                Log($"Response [{request.responseCode}]: {TruncateLog(responseBody)}");
                onSuccess?.Invoke(responseBody);
            }
            else
            {
                string errorMessage = request.error;
                long errorCode = request.responseCode;

                // Try to parse error response body
                string responseBody = request.downloadHandler?.text ?? "";
                if (!string.IsNullOrEmpty(responseBody))
                {
                    try
                    {
                        var errorResponse = JsonUtility.FromJson<CLOUPTErrorResponse>(responseBody);
                        if (!string.IsNullOrEmpty(errorResponse.message))
                        {
                            errorMessage = errorResponse.message;
                        }
                    }
                    catch { }
                }

                LogError($"Request failed [{errorCode}]: {errorMessage}");
                onError?.Invoke(new CLOUPTError(errorCode, errorMessage));
            }
        }

        #endregion

        #region Logging

        private void Log(string message)
        {
            if (CLOUPTSettings.Instance != null && CLOUPTSettings.Instance.DebugMode)
            {
                Debug.Log($"[CLOUPT] {message}");
            }
        }

        private void LogError(string message)
        {
            Debug.LogError($"[CLOUPT] {message}");
        }

        private string TruncateLog(string text, int maxLength = 200)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;
            return text.Substring(0, maxLength) + "...";
        }

        #endregion
    }

    /// <summary>
    /// Represents an error returned from the CLOUPT API.
    /// </summary>
    [Serializable]
    public class CLOUPTError
    {
        public long Code;
        public string Message;

        public CLOUPTError(long code, string message)
        {
            Code = code;
            Message = message;
        }

        public override string ToString()
        {
            return $"[{Code}] {Message}";
        }
    }

    /// <summary>
    /// Internal class for parsing error responses from the API.
    /// </summary>
    [Serializable]
    internal class CLOUPTErrorResponse
    {
        public string message;
        public string error;
        public int code;
    }
}
