using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.Events;

namespace FeedbackForm
{
    public class FeedbackSender : MonoBehaviour
    {
        private string trelloListId;
        private string trelloApiKey;
        private string trelloToken;

        private FeedbackSettings _settings;

        private void Start()
        {
            _settings = GetComponent<FeedbackSettings>();

            if (_settings)
            {
                Initialize(_settings.GetTrelloApiKey(),_settings.GetTrelloApiToken());
            }
            else
            {
                Debug.LogError("Please attach Feedback Settings to same object!");
                enabled = false;
            }
            
        }

        public void Initialize(string apiKey, string token)
        {
            trelloApiKey = apiKey;
            trelloToken = token;
        }

        public void SendFeedbackToTrello(string title, string description,string category,int priority, UnityEvent onSuccess, UnityEvent onFailure, GameObject panel)
        {
            trelloListId = _settings.GetTrelloListId(category);
            StartCoroutine(TakeScreenshotAndSend(title, description,priority, onSuccess, onFailure, panel));
        }

private IEnumerator TakeScreenshotAndSend(string title, string description, int priority, UnityEvent onSuccess, UnityEvent onFailure, GameObject panel)
{
    panel.SetActive(false);
    yield return new WaitForEndOfFrame();

    // capture screenshot
    Texture2D screenTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
    screenTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
    screenTexture.Apply();
    byte[] imageData = screenTexture.EncodeToPNG();
    yield return new WaitForEndOfFrame();
    panel.SetActive(true);

    // determine the priority label (use specific trello label ids)
    string labelId = string.Empty;
    switch (priority)
    {
        case 0:
            labelId = "YOUR_LOW_LEVEL_LABEL_ID"; // replace with actual label ID
            break;
        case 1:
            labelId = "YOUR_MED_LEVEL_LABEL_ID"; // replace with actual label ID
            break;
        case 2:
            labelId = "YOUR_HIGH_LEVEL_LABEL_ID"; // replace with actual label ID
            break;
        default:
            Debug.LogWarning("Unknown priority, no label will be applied.");
            break;
    }

    // Create Trello card form
    WWWForm form = new WWWForm();
    form.AddField("idList", trelloListId);
    form.AddField("key", trelloApiKey);
    form.AddField("token", trelloToken);
    form.AddField("name", title);
    form.AddField("desc", description);

    if (!string.IsNullOrEmpty(labelId))
    {
        form.AddField("idLabels", labelId); // Add the label ID for priority
    }

    form.AddBinaryData("fileSource", imageData, "screenshot.png", "image/png");

    // Send request to Trello
    UnityWebRequest www = UnityWebRequest.Post("https://api.trello.com/1/cards?", form);
    yield return www.SendWebRequest();

    if (www.result != UnityWebRequest.Result.Success)
    {
        Debug.LogError("Failed submission to Trello: " + www.error);
        onFailure?.Invoke();
    }
    else
    {
        Debug.Log("Feedback sent successfully");
        onSuccess?.Invoke();
    }
}

    }
}
