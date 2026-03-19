namespace Commersion.Core.Legal
{
    using System;
    using System.Threading.Tasks;
    using Commersion.Core;
    using TMPro;
    using UnityEngine;

    public class CommersionLegalPopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text contentText;
        private TaskCompletionSource<bool> _tcs;

        private void Awake()
        {
            CommersionManager.Instance.Initialize();
        }

        private void OnEnable()
        {
            contentText.text = CommersionManager.Instance.GetLegalText();
        }

        /// <summary>
        /// Shows the popup and waits until the player accepts or rejects.
        /// Returns true if accepted, false if rejected.
        /// </summary>
        public async Task<bool> GetResult()
        {
            _tcs = new TaskCompletionSource<bool>();
        
            bool result = await _tcs.Task;
        
            return result;
        }

        public void Accept()
        {
            PlayerPrefs.SetInt(Constants.LEGAL_SAVE_KEY,1);
            _tcs?.TrySetResult(true);
        }

        public void Reject()
        {
            _tcs?.TrySetResult(false);
        }
    }
}
