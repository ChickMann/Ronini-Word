using System;
using System.Threading.Tasks;
using Commersion.Core;
using Commersion.Core.Loader;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Commersion.Examples
{
    public class CommersionalExample : MonoBehaviour
    {
        private void Awake()
        {
            CommersionManager.Instance.Initialize();
        }

        private async void Start()
        {
            await Task.Delay(200);
            TaskRunner.Instance.ExecuteTaskAsync<bool>(
                CommersionManager.Instance.ShowLegalPopup,  // asyncTask
                result => Debug.Log("Result : " + result),  
                error => Debug.LogError("Error: " + error.Message),
                taskName: "Show Legal Popup",  
                showLoader: false  
            );

        }

        public void DoSomethingAsync()
        {
            TaskRunner.Instance.ExecuteTaskAsync<int>(
                AsyncTask,  // asyncTask
                result => Debug.Log("Result : " + result),  
                error => Debug.LogError("Error: " + error.Message),
                taskName: "Show Legal Popup",  
                showLoader: true  
            );
        }

        private async Task<int> AsyncTask()
        {
            await Task.Delay(Random.Range(1000, 2500));
            return Random.Range(0, 100);
        }
    
    }

}
