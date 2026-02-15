using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace CLOUPT.Feedback
{
    /// <summary>
    /// CLOUPT Feedback UI - Ready-to-use feedback form component.
    /// Attach to a Canvas with the required UI elements.
    /// </summary>
    public class FeedbackUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TMP_Dropdown _typeDropdown;
        [SerializeField] private TMP_InputField _headerInput;
        [SerializeField] private TMP_InputField _messageInput;
        [SerializeField] private TMP_Dropdown _priorityDropdown;
        [SerializeField] private GameObject _ratingContainer;
        [SerializeField] private Button[] _ratingButtons;
        [SerializeField] private Button _submitButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _screenshotToggle;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private GameObject _loadingIndicator;

        [Header("Settings")]
        [SerializeField] private bool _includeScreenshot = false;
        [SerializeField] private bool _closeOnSubmit = true;
        [SerializeField] private float _statusDisplayDuration = 3f;

        [Header("Customization")]
        [SerializeField] private Color _selectedRatingColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color _unselectedRatingColor = Color.gray;

        private int _selectedRating = 0;
        private bool _isSubmitting = false;
        private FeedbackType _selectedType = FeedbackType.Feedback;
        private FeedbackPriority _selectedPriority = FeedbackPriority.Medium;

        /// <summary>
        /// Event fired when feedback is successfully submitted.
        /// </summary>
        public event Action<FeedbackResponse> OnFeedbackSubmitted;

        /// <summary>
        /// Event fired when feedback submission fails.
        /// </summary>
        public event Action<FeedbackError> OnFeedbackFailed;

        private void Start()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            // Setup type dropdown
            if (_typeDropdown != null)
            {
                _typeDropdown.ClearOptions();
                _typeDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "General Feedback",
                    "Bug Report",
                    "Feature Request",
                    "Suggestion"
                });
                _typeDropdown.onValueChanged.AddListener(OnTypeChanged);
            }

            // Setup priority dropdown
            if (_priorityDropdown != null)
            {
                _priorityDropdown.ClearOptions();
                _priorityDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "Low",
                    "Medium",
                    "High",
                    "Critical"
                });
                _priorityDropdown.value = 1; // Default to Medium
                _priorityDropdown.onValueChanged.AddListener(OnPriorityChanged);
            }

            // Setup rating buttons
            if (_ratingButtons != null)
            {
                for (int i = 0; i < _ratingButtons.Length; i++)
                {
                    int rating = i + 1;
                    _ratingButtons[i].onClick.AddListener(() => SetRating(rating));
                }
            }

            // Setup submit button
            if (_submitButton != null)
            {
                _submitButton.onClick.AddListener(OnSubmitClicked);
            }

            // Setup cancel button
            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(Hide);
            }

            // Setup screenshot toggle
            if (_screenshotToggle != null)
            {
                _screenshotToggle.onClick.AddListener(() =>
                {
                    _includeScreenshot = !_includeScreenshot;
                    UpdateScreenshotToggleVisual();
                });
            }

            // Initial state
            UpdateRatingVisuals();
            SetStatus("");
            if (_loadingIndicator != null) _loadingIndicator.SetActive(false);
            if (_ratingContainer != null) _ratingContainer.SetActive(true);
        }

        private void OnTypeChanged(int index)
        {
            switch (index)
            {
                case 0:
                    _selectedType = FeedbackType.Feedback;
                    if (_ratingContainer != null) _ratingContainer.SetActive(true);
                    break;
                case 1:
                    _selectedType = FeedbackType.Bug;
                    if (_ratingContainer != null) _ratingContainer.SetActive(false);
                    break;
                case 2:
                    _selectedType = FeedbackType.Feature;
                    if (_ratingContainer != null) _ratingContainer.SetActive(false);
                    break;
                case 3:
                    _selectedType = FeedbackType.Suggestion;
                    if (_ratingContainer != null) _ratingContainer.SetActive(false);
                    break;
            }
        }

        private void OnPriorityChanged(int index)
        {
            switch (index)
            {
                case 0:
                    _selectedPriority = FeedbackPriority.Low;
                    break;
                case 1:
                    _selectedPriority = FeedbackPriority.Medium;
                    break;
                case 2:
                    _selectedPriority = FeedbackPriority.High;
                    break;
                case 3:
                    _selectedPriority = FeedbackPriority.Critical;
                    break;
            }
        }

        private void SetRating(int rating)
        {
            _selectedRating = rating;
            UpdateRatingVisuals();
        }

        private void UpdateRatingVisuals()
        {
            if (_ratingButtons == null) return;

            for (int i = 0; i < _ratingButtons.Length; i++)
            {
                var colors = _ratingButtons[i].colors;
                colors.normalColor = (i < _selectedRating) ? _selectedRatingColor : _unselectedRatingColor;
                _ratingButtons[i].colors = colors;

                // Update star text if present
                var text = _ratingButtons[i].GetComponentInChildren<Text>();
                if (text != null)
                {
                    text.color = (i < _selectedRating) ? _selectedRatingColor : _unselectedRatingColor;
                }
            }
        }

        private void UpdateScreenshotToggleVisual()
        {
            if (_screenshotToggle != null)
            {
                var colors = _screenshotToggle.colors;
                colors.normalColor = _includeScreenshot ? _selectedRatingColor : _unselectedRatingColor;
                _screenshotToggle.colors = colors;
            }
        }

        private void OnSubmitClicked()
        {
            if (_isSubmitting) return;

            string message = _messageInput != null ? _messageInput.text : "";
            string header = _headerInput != null ? _headerInput.text : "";

            if (string.IsNullOrEmpty(message) || message.Length < 5)
            {
                SetStatus("Please enter at least 5 characters.", true);
                return;
            }

            _isSubmitting = true;
            SetSubmitting(true);

            if (_includeScreenshot)
            {
                CLOUPTFeedback.Instance.SubmitWithScreenshotCapture(
                    _selectedType,
                    message,
                    _selectedRating,
                    header,
                    _selectedPriority,
                    OnSubmitSuccess,
                    OnSubmitError
                );
            }
            else
            {
                CLOUPTFeedback.Instance.Submit(
                    _selectedType,
                    message,
                    _selectedRating,
                    null,
                    header,
                    _selectedPriority,
                    OnSubmitSuccess,
                    OnSubmitError
                );
            }
        }

        private void OnSubmitSuccess(FeedbackResponse response)
        {
            _isSubmitting = false;
            SetSubmitting(false);
            SetStatus("Thank you for your feedback!", false);
            OnFeedbackSubmitted?.Invoke(response);

            if (_closeOnSubmit)
            {
                Invoke(nameof(Hide), _statusDisplayDuration);
            }

            ResetForm();
        }

        private void OnSubmitError(FeedbackError error)
        {
            _isSubmitting = false;
            SetSubmitting(false);
            SetStatus($"Error: {error.message}", true);
            OnFeedbackFailed?.Invoke(error);
        }

        private void SetSubmitting(bool submitting)
        {
            if (_submitButton != null) _submitButton.interactable = !submitting;
            if (_loadingIndicator != null) _loadingIndicator.SetActive(submitting);
        }

        private void SetStatus(string message, bool isError = false)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
                _statusText.color = isError ? Color.red : Color.green;
            }
        }

        private void ResetForm()
        {
            if (_headerInput != null) _headerInput.text = "";
            if (_messageInput != null) _messageInput.text = "";
            if (_typeDropdown != null) _typeDropdown.value = 0;
            if (_priorityDropdown != null) _priorityDropdown.value = 1; // Medium
            _selectedRating = 0;
            _selectedPriority = FeedbackPriority.Medium;
            UpdateRatingVisuals();
        }

        /// <summary>
        /// Shows the feedback panel.
        /// </summary>
        public void Show()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }
            gameObject.SetActive(true);
            ResetForm();
            SetStatus("");
        }

        /// <summary>
        /// Hides the feedback panel.
        /// </summary>
        public void Hide()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Toggles the feedback panel visibility.
        /// </summary>
        public void Toggle()
        {
            bool isActive = _panelRoot != null ? _panelRoot.activeSelf : gameObject.activeSelf;
            if (isActive) Hide();
            else Show();
        }
    }
}
