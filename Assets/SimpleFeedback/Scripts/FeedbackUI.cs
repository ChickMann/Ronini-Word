using UnityEngine;
using TMPro;
using UnityEngine.Events;

namespace FeedbackForm
{
    public class FeedbackUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_InputField headerText;
        [SerializeField] private TMP_InputField contentText;
        [SerializeField] private TMP_Dropdown priorityDropdown;
        [SerializeField] private TMP_Dropdown categoryDropdown;
        
        [Header("Settings")] 
        [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

        // Events for sending feedback
        public UnityEvent OnFeedBackSent;
        public UnityEvent OnFeedBackFailed;

        private FeedbackSender feedbackSender;

        private void Awake()
        {
            feedbackSender = GetComponent<FeedbackSender>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                if(panel.activeInHierarchy)
                    Hide();
                else
                    Show();
            }
        }

        public void Show()
        {
            panel.SetActive(true);
        }

        public void Hide()
        {
            panel.SetActive(false);
        }

        public void SendFeedback()
        {
            string header = headerText.text;
            if(string.IsNullOrEmpty(header))
            {
                OnFeedBackFailed?.Invoke();
                return;
            }
            string content = CreateInformation();
            
            feedbackSender.SendFeedbackToTrello(header, content,categoryDropdown.options[categoryDropdown.value].text,priorityDropdown.value, OnFeedBackSent, OnFeedBackFailed, panel);
        }

        private string CreateInformation()
        {
            string info = "";
            info = $"Operating system : {SystemInfo.operatingSystem}";
            return info;
        }
    }
}