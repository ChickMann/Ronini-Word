using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Commersion.Core.Loader
{
    public class TaskRunner : MonoBehaviour
{
    [Header("Loader Configuration")]
    
    [SerializeField] private Canvas loaderCanvas;
    [SerializeField]  private GameObject loaderPrefab;
    [SerializeField] private bool blockUserInput = true;
    [SerializeField] private float minDisplayTime = 0.5f; // Minimum time to show loader
    
    [Header("UI Elements")]
    [SerializeField] private Text loadingText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Button cancelButton;
    
  
    private GameObject currentLoader;
    private GraphicRaycaster canvasRaycaster;
    private Queue<TaskInfo> taskQueue = new Queue<TaskInfo>();
    private bool isProcessingTask = false;
    private CancellationTokenSource currentCancellationToken;
    
    // Singleton pattern for easy access
    public static TaskRunner Instance { get; private set; }
    
    // Events
    public event Action<string> OnTaskStarted;
    public event Action<string> OnTaskCompleted;
    public event Action<string, Exception> OnTaskFailed;
    public event Action<float> OnProgressUpdated;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeTaskRunner();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        loaderPrefab = CommersionManager.Instance.GetLoaderPrefab();
    }

    private void InitializeTaskRunner()
    {
        // Get canvas raycaster for input blocking
        if (loaderCanvas != null)
        {
            canvasRaycaster = loaderCanvas.GetComponent<GraphicRaycaster>();
        }
       
        // Setup cancel button
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelCurrentTask);
        }
    }
    
    #region Public Methods
    
    /// <summary>
    /// Execute a simple action with loader
    /// </summary>
    public void ExecuteTask(Action task, string taskName = "Processing...", bool showLoader = true)
    {
        var taskInfo = new TaskInfo
        {
            TaskName = taskName,
            TaskType = TaskType.Action,
            SimpleAction = task,
            ShowLoader = showLoader
        };
        
        QueueTask(taskInfo);
    }
    
    /// <summary>
    /// Execute a function with loader and return result via callback
    /// </summary>
    public void ExecuteTask<T>(Func<T> task, Action<T> onSuccess, Action<Exception> onError = null,
        string taskName = "Processing...", bool showLoader = true)
    {
        var taskInfo = new TaskInfo
        {
            TaskName = taskName,
            TaskType = TaskType.FunctionWithResult,
            FunctionWithResult = () => task(),
            OnResultSuccess = (result) => onSuccess?.Invoke((T)result),
            OnResultError = onError,
            ShowLoader = showLoader
        };
        
        QueueTask(taskInfo);
    }
    
    /// <summary>
    /// Execute an async task with loader
    /// </summary>
    public void ExecuteTaskAsync(Func<Task> asyncTask, string taskName = "Processing...", bool showLoader = true)
    {
        var taskInfo = new TaskInfo
        {
            TaskName = taskName,
            TaskType = TaskType.AsyncTask,
            AsyncTask = asyncTask,
            ShowLoader = showLoader
        };
        
        QueueTask(taskInfo);
    }
    
    /// <summary>
    /// Execute an async task with loader and return result via callback
    /// </summary>
    public void ExecuteTaskAsync<T>(Func<Task<T>> asyncTask, Action<T> onSuccess, Action<Exception> onError = null, 
        string taskName = "Processing...", bool showLoader = true)
    {
        var taskInfo = new TaskInfo
        {
            TaskName = taskName,
            TaskType = TaskType.AsyncTaskWithResult,
            AsyncTaskWithResult = async () => await asyncTask(),
            OnResultSuccess = (result) => onSuccess?.Invoke((T)result),
            OnResultError = onError,
            ShowLoader = showLoader
        };
        
        QueueTask(taskInfo);
    }
    
    /// <summary>
    /// Execute an async task with progress reporting
    /// </summary>
    public void ExecuteTaskWithProgress(Func<IProgress<float>, Task> asyncTaskWithProgress, 
        string taskName = "Processing...", bool showLoader = true)
    {
        var taskInfo = new TaskInfo
        {
            TaskName = taskName,
            TaskType = TaskType.AsyncTaskWithProgress,
            AsyncTaskWithProgress = asyncTaskWithProgress,
            ShowLoader = showLoader
        };
        
        QueueTask(taskInfo);
    }
    
    /// <summary>
    /// Execute an async task with progress reporting and return result via callback
    /// </summary>
    public void ExecuteTaskWithProgress<T>(Func<IProgress<float>, Task<T>> asyncTaskWithProgress, 
        Action<T> onSuccess, Action<Exception> onError = null,
        string taskName = "Processing...", bool showLoader = true)
    {
        var taskInfo = new TaskInfo
        {
            TaskName = taskName,
            TaskType = TaskType.AsyncTaskWithProgressAndResult,
            AsyncTaskWithProgressAndResult = async (progress) => await asyncTaskWithProgress(progress),
            OnResultSuccess = (result) => onSuccess?.Invoke((T)result),
            OnResultError = onError,
            ShowLoader = showLoader
        };
        
        QueueTask(taskInfo);
    }
    
    /// <summary>
    /// Execute a coroutine with loader
    /// </summary>
    public void ExecuteCoroutine(Func<IEnumerator> coroutineFunc, string taskName = "Processing...", bool showLoader = true)
    {
        var taskInfo = new TaskInfo
        {
            TaskName = taskName,
            TaskType = TaskType.Coroutine,
            CoroutineFunc = coroutineFunc,
            ShowLoader = showLoader
        };
        
        QueueTask(taskInfo);
    }
    
    /// <summary>
    /// Execute a timed task (useful for API calls with timeout)
    /// </summary>
    public void ExecuteTimedTask(Func<Task> asyncTask, float timeoutSeconds, 
        string taskName = "Processing...", bool showLoader = true)
    {
        var taskInfo = new TaskInfo
        {
            TaskName = taskName,
            TaskType = TaskType.TimedTask,
            AsyncTask = asyncTask,
            TimeoutSeconds = timeoutSeconds,
            ShowLoader = showLoader
        };
        
        QueueTask(taskInfo);
    }
    
    /// <summary>
    /// Execute a background thread task
    /// </summary>
    public void ExecuteBackgroundTask(Action backgroundAction, Action<Exception> onComplete = null,
        string taskName = "Processing...", bool showLoader = true)
    {
        var taskInfo = new TaskInfo
        {
            TaskName = taskName,
            TaskType = TaskType.BackgroundThread,
            BackgroundAction = backgroundAction,
            OnBackgroundComplete = onComplete,
            ShowLoader = showLoader
        };
        
        QueueTask(taskInfo);
    }
    
    /// <summary>
    /// Execute a background thread task with result
    /// </summary>
    public void ExecuteBackgroundTask<T>(Func<T> backgroundFunction, Action<T> onSuccess, Action<Exception> onError = null,
        string taskName = "Processing...", bool showLoader = true)
    {
        var taskInfo = new TaskInfo
        {
            TaskName = taskName,
            TaskType = TaskType.BackgroundThreadWithResult,
            BackgroundFunction = () => backgroundFunction(),
            OnResultSuccess = (result) => onSuccess?.Invoke((T)result),
            OnResultError = onError,
            ShowLoader = showLoader
        };
        
        QueueTask(taskInfo);
    }
    
    /// <summary>
    /// Cancel the current running task
    /// </summary>
    public void CancelCurrentTask()
    {
        if (currentCancellationToken != null && !currentCancellationToken.Token.IsCancellationRequested)
        {
            currentCancellationToken.Cancel();
            Debug.Log("Task cancellation requested");
        }
    }
    
    /// <summary>
    /// Check if task runner is currently processing
    /// </summary>
    public bool IsProcessing => isProcessingTask;
    
    /// <summary>
    /// Get current task queue count
    /// </summary>
    public int QueueCount => taskQueue.Count;
    
    #endregion
    
    #region Private Methods
    
    private void QueueTask(TaskInfo taskInfo)
    {
        taskQueue.Enqueue(taskInfo);
        
        if (!isProcessingTask)
        {
            StartCoroutine(ProcessTaskQueue());
        }
    }
    
    private IEnumerator ProcessTaskQueue()
    {
        isProcessingTask = true;
        
        while (taskQueue.Count > 0)
        {
            var taskInfo = taskQueue.Dequeue();
            yield return StartCoroutine(ExecuteTaskInfo(taskInfo));
        }
        
        isProcessingTask = false;
    }
    
    private IEnumerator ExecuteTaskInfo(TaskInfo taskInfo)
    {
        float startTime = Time.time;
        currentCancellationToken = new CancellationTokenSource();
        bool success = false;
        Exception taskException = null;
        
        // Show loader if required
        if (taskInfo.ShowLoader)
        {
            ShowLoader(taskInfo.TaskName);
        }
        
        // Trigger start event
        OnTaskStarted?.Invoke(taskInfo.TaskName);
        
        // Execute based on task type with error handling
        yield return StartCoroutine(ExecuteTaskWithErrorHandling(taskInfo, (s, e) => 
        {
            success = s;
            taskException = e;
        }));
        
        // Ensure minimum display time
        float elapsedTime = Time.time - startTime;
        if (elapsedTime < minDisplayTime)
        {
            yield return new WaitForSeconds(minDisplayTime - elapsedTime);
        }
        
        // Trigger completion events
        if (success && !currentCancellationToken.Token.IsCancellationRequested)
        {
            OnTaskCompleted?.Invoke(taskInfo.TaskName);
        }
        else if (taskException != null)
        {
            OnTaskFailed?.Invoke(taskInfo.TaskName, taskException);
            Debug.LogError($"Task '{taskInfo.TaskName}' failed with exception: {taskException.Message}");
            
            // Call error callback if available
            taskInfo.OnResultError?.Invoke(taskException);
        }
        
        // Cleanup
        if (taskInfo.ShowLoader)
        {
            HideLoader();
        }
        
        currentCancellationToken?.Dispose();
        currentCancellationToken = null;
    }
    
    private IEnumerator ExecuteTaskWithErrorHandling(TaskInfo taskInfo, Action<bool, Exception> onComplete)
    {
        bool success = false;
        Exception exception = null;
        
        // Execute the appropriate task type
        switch (taskInfo.TaskType)
        {
            case TaskType.Action:
                yield return StartCoroutine(ExecuteSimpleAction(taskInfo, (s, e) => 
                {
                    success = s;
                    exception = e;
                }));
                break;
                
            case TaskType.FunctionWithResult:
                yield return StartCoroutine(ExecuteFunctionWithResult(taskInfo, (s, e) => 
                {
                    success = s;
                    exception = e;
                }));
                break;
                
            case TaskType.AsyncTask:
                yield return StartCoroutine(ExecuteAsyncTask(taskInfo, (s, e) => 
                {
                    success = s;
                    exception = e;
                }));
                break;
                
            case TaskType.AsyncTaskWithResult:
                yield return StartCoroutine(ExecuteAsyncTaskWithResult(taskInfo, (s, e) => 
                {
                    success = s;
                    exception = e;
                }));
                break;
                
            case TaskType.AsyncTaskWithProgress:
                yield return StartCoroutine(ExecuteAsyncTaskWithProgress(taskInfo, (s, e) => 
                {
                    success = s;
                    exception = e;
                }));
                break;
                
            case TaskType.AsyncTaskWithProgressAndResult:
                yield return StartCoroutine(ExecuteAsyncTaskWithProgressAndResult(taskInfo, (s, e) => 
                {
                    success = s;
                    exception = e;
                }));
                break;
                
            case TaskType.Coroutine:
                yield return StartCoroutine(ExecuteCoroutineTask(taskInfo, (s, e) => 
                {
                    success = s;
                    exception = e;
                }));
                break;
                
            case TaskType.TimedTask:
                yield return StartCoroutine(ExecuteTimedTask(taskInfo, (s, e) => 
                {
                    success = s;
                    exception = e;
                }));
                break;
                
            case TaskType.BackgroundThread:
                yield return StartCoroutine(ExecuteBackgroundTask(taskInfo, (s, e) => 
                {
                    success = s;
                    exception = e;
                }));
                break;
                
            case TaskType.BackgroundThreadWithResult:
                yield return StartCoroutine(ExecuteBackgroundTaskWithResult(taskInfo, (s, e) => 
                {
                    success = s;
                    exception = e;
                }));
                break;
        }
        
        onComplete?.Invoke(success, exception);
    }
    
    private IEnumerator ExecuteSimpleAction(TaskInfo taskInfo, Action<bool, Exception> onComplete)
    {
        yield return null; // Wait one frame
        
        bool success = false;
        Exception exception = null;
        
        try
        {
            taskInfo.SimpleAction?.Invoke();
            success = true;
        }
        catch (Exception e)
        {
            exception = e;
        }
        
        onComplete?.Invoke(success, exception);
    }
    
    private IEnumerator ExecuteFunctionWithResult(TaskInfo taskInfo, Action<bool, Exception> onComplete)
    {
        yield return null; // Wait one frame
        
        bool success = false;
        Exception exception = null;
        object result = null;
        
        try
        {
            result = taskInfo.FunctionWithResult?.Invoke();
            success = true;
            taskInfo.OnResultSuccess?.Invoke(result);
        }
        catch (Exception e)
        {
            exception = e;
        }
        
        onComplete?.Invoke(success, exception);
    }
    
    private IEnumerator ExecuteAsyncTask(TaskInfo taskInfo, Action<bool, Exception> onComplete)
    {
        bool success = false;
        Exception exception = null;
        Task task = null;
        
        try
        {
            task = taskInfo.AsyncTask?.Invoke();
        }
        catch (Exception e)
        {
            exception = e;
        }
        
        if (task != null && exception == null)
        {
            yield return new WaitUntil(() => task.IsCompleted || currentCancellationToken.Token.IsCancellationRequested);
            
            if (task.IsFaulted)
            {
                exception = task.Exception?.GetBaseException();
            }
            else if (!currentCancellationToken.Token.IsCancellationRequested)
            {
                success = true;
            }
        }
        
        onComplete?.Invoke(success, exception);
    }
    
    private IEnumerator ExecuteAsyncTaskWithResult(TaskInfo taskInfo, Action<bool, Exception> onComplete)
    {
        bool success = false;
        Exception exception = null;
        Task<object> task = null;
        
        try
        {
            task = taskInfo.AsyncTaskWithResult?.Invoke();
        }
        catch (Exception e)
        {
            exception = e;
        }
        
        if (task != null && exception == null)
        {
            yield return new WaitUntil(() => task.IsCompleted || currentCancellationToken.Token.IsCancellationRequested);
            
            if (task.IsFaulted)
            {
                exception = task.Exception?.GetBaseException();
            }
            else if (!currentCancellationToken.Token.IsCancellationRequested)
            {
                success = true;
                taskInfo.OnResultSuccess?.Invoke(task.Result);
            }
        }
        
        onComplete?.Invoke(success, exception);
    }
    
    private IEnumerator ExecuteAsyncTaskWithProgress(TaskInfo taskInfo, Action<bool, Exception> onComplete)
    {
        bool success = false;
        Exception exception = null;
        Task task = null;
        
        try
        {
            var progress = new Progress<float>(value => 
            {
                UpdateProgress(value);
                OnProgressUpdated?.Invoke(value);
            });
            
            task = taskInfo.AsyncTaskWithProgress?.Invoke(progress);
        }
        catch (Exception e)
        {
            exception = e;
        }
        
        if (task != null && exception == null)
        {
            yield return new WaitUntil(() => task.IsCompleted || currentCancellationToken.Token.IsCancellationRequested);
            
            if (task.IsFaulted)
            {
                exception = task.Exception?.GetBaseException();
            }
            else if (!currentCancellationToken.Token.IsCancellationRequested)
            {
                success = true;
            }
        }
        
        onComplete?.Invoke(success, exception);
    }
    
    private IEnumerator ExecuteAsyncTaskWithProgressAndResult(TaskInfo taskInfo, Action<bool, Exception> onComplete)
    {
        bool success = false;
        Exception exception = null;
        Task<object> task = null;
        
        try
        {
            var progress = new Progress<float>(value => 
            {
                UpdateProgress(value);
                OnProgressUpdated?.Invoke(value);
            });
            
            task = taskInfo.AsyncTaskWithProgressAndResult?.Invoke(progress);
        }
        catch (Exception e)
        {
            exception = e;
        }
        
        if (task != null && exception == null)
        {
            yield return new WaitUntil(() => task.IsCompleted || currentCancellationToken.Token.IsCancellationRequested);
            
            if (task.IsFaulted)
            {
                exception = task.Exception?.GetBaseException();
            }
            else if (!currentCancellationToken.Token.IsCancellationRequested)
            {
                success = true;
                taskInfo.OnResultSuccess?.Invoke(task.Result);
            }
        }
        
        onComplete?.Invoke(success, exception);
    }
    
    private IEnumerator ExecuteCoroutineTask(TaskInfo taskInfo, Action<bool, Exception> onComplete)
    {
        bool success = false;
        Exception exception = null;
        IEnumerator coroutine = null;
        
        try
        {
            coroutine = taskInfo.CoroutineFunc?.Invoke();
        }
        catch (Exception e)
        {
            exception = e;
        }
        
        if (coroutine != null && exception == null)
        {
            // Execute coroutine directly - Unity coroutines don't throw exceptions in the traditional sense
            yield return StartCoroutine(coroutine);
            success = true;
        }
        
        onComplete?.Invoke(success, exception);
    }
    
    private IEnumerator ExecuteTimedTask(TaskInfo taskInfo, Action<bool, Exception> onComplete)
    {
        bool success = false;
        Exception exception = null;
        Task task = null;
        
        try
        {
            task = taskInfo.AsyncTask?.Invoke();
        }
        catch (Exception e)
        {
            exception = e;
        }
        
        if (task != null && exception == null)
        {
            float timeoutTime = Time.time + taskInfo.TimeoutSeconds;
            
            yield return new WaitUntil(() => 
                task.IsCompleted || 
                Time.time > timeoutTime || 
                currentCancellationToken.Token.IsCancellationRequested);
            
            if (Time.time > timeoutTime && !task.IsCompleted)
            {
                Debug.LogWarning($"Task '{taskInfo.TaskName}' timed out after {taskInfo.TimeoutSeconds} seconds");
                currentCancellationToken.Cancel();
                exception = new TimeoutException($"Task timed out after {taskInfo.TimeoutSeconds} seconds");
            }
            else if (task.IsFaulted)
            {
                exception = task.Exception?.GetBaseException();
            }
            else if (!currentCancellationToken.Token.IsCancellationRequested)
            {
                success = true;
            }
        }
        
        onComplete?.Invoke(success, exception);
    }
    
    private IEnumerator ExecuteBackgroundTask(TaskInfo taskInfo, Action<bool, Exception> onComplete)
    {
        bool taskCompleted = false;
        Exception taskException = null;
        bool success = false;
        
        // Store references to avoid closure issues
        var backgroundAction = taskInfo.BackgroundAction;
        var backgroundComplete = taskInfo.OnBackgroundComplete;
        
        // Execute on background thread
        Task.Run(() =>
        {
            try
            {
                backgroundAction?.Invoke();
                success = true;
            }
            catch (Exception e)
            {
                taskException = e;
            }
            finally
            {
                taskCompleted = true;
            }
        });
        
        // Wait for completion
        yield return new WaitUntil(() => taskCompleted || currentCancellationToken.Token.IsCancellationRequested);
        
        // Handle completion callback on main thread
        if (taskCompleted)
        {
            if (taskException == null)
            {
                backgroundComplete?.Invoke(null);
            }
            else
            {
                backgroundComplete?.Invoke(taskException);
            }
        }
        
        onComplete?.Invoke(success && taskException == null, taskException);
    }
    
    private IEnumerator ExecuteBackgroundTaskWithResult(TaskInfo taskInfo, Action<bool, Exception> onComplete)
    {
        bool taskCompleted = false;
        Exception taskException = null;
        bool success = false;
        object result = null;
        
        // Store references to avoid closure issues
        var backgroundFunction = taskInfo.BackgroundFunction;
        var resultSuccess = taskInfo.OnResultSuccess;
        var resultError = taskInfo.OnResultError;
        
        // Execute on background thread
        Task.Run(() =>
        {
            try
            {
                result = backgroundFunction?.Invoke();
                success = true;
            }
            catch (Exception e)
            {
                taskException = e;
            }
            finally
            {
                taskCompleted = true;
            }
        });
        
        // Wait for completion
        yield return new WaitUntil(() => taskCompleted || currentCancellationToken.Token.IsCancellationRequested);
        
        // Handle completion callback on main thread
        if (taskCompleted)
        {
            if (taskException == null && success)
            {
                resultSuccess?.Invoke(result);
            }
            else if (taskException != null)
            {
                resultError?.Invoke(taskException);
            }
        }
        
        onComplete?.Invoke(success && taskException == null, taskException);
    }
    
    #endregion
    
    #region Loader Management
    

    private void ShowLoader(string message)
    {
        if (loaderPrefab != null && currentLoader == null)
        {
            if (currentLoader != null)
            {
                Destroy(currentLoader);
                currentLoader = null;
            }
        
            currentLoader = Instantiate(loaderPrefab, loaderCanvas.transform);
        
            // Update loading text
            if (loadingText != null)
            {
                loadingText.text = message;
            }
        
            // Reset progress bar
            if (progressBar != null)
            {
                progressBar.value = 0f;
                progressBar.gameObject.SetActive(false);
            }
        
            // Show cancel button if available
            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(true);
            }
        
            // Block input if required
            if (blockUserInput && canvasRaycaster != null)
            {
                canvasRaycaster.enabled = true;
            }
        }
    }
    
    private void HideLoader()
    {
        if (currentLoader != null)
        {
            currentLoader.SetActive(false);
            Destroy(currentLoader);
            currentLoader = null;
        }
    
        // Restore input
        if (blockUserInput && canvasRaycaster != null)
        {
            canvasRaycaster.enabled = false;
        }
    }


    
    private void UpdateProgress(float progress)
    {
        if (progressBar != null && currentLoader != null)
        {
            progressBar.gameObject.SetActive(true);
            progressBar.value = Mathf.Clamp01(progress);
        }
    }
    
    #endregion
}

#region Supporting Classes

[System.Serializable]
public class TaskInfo
{
    public string TaskName;
    public TaskType TaskType;
    public bool ShowLoader = true;
    public float TimeoutSeconds = 30f;
    
    // Different task types
    public Action SimpleAction;
    public Func<object> FunctionWithResult;
    public Func<Task> AsyncTask;
    public Func<Task<object>> AsyncTaskWithResult;
    public Func<IProgress<float>, Task> AsyncTaskWithProgress;
    public Func<IProgress<float>, Task<object>> AsyncTaskWithProgressAndResult;
    public Func<IEnumerator> CoroutineFunc;
    public Action BackgroundAction;
    public Func<object> BackgroundFunction;
    public Action<Exception> OnBackgroundComplete;
    
    // Result handling callbacks
    public Action<object> OnResultSuccess;
    public Action<Exception> OnResultError;
}

public enum TaskType
{
    Action,
    FunctionWithResult,
    AsyncTask,
    AsyncTaskWithResult,
    AsyncTaskWithProgress,
    AsyncTaskWithProgressAndResult,
    Coroutine,
    TimedTask,
    BackgroundThread,
    BackgroundThreadWithResult
}

#endregion
}

