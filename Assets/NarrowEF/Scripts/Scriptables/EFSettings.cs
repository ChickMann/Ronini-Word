using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EF.Generic
{
    [CreateAssetMenu(fileName = "Easy Firebase", menuName = "Settings")]
    public class EFSettings : ScriptableObject
    {
        #region Instance

        private static EFSettings instance;

        public static EFSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<EFSettings>("NarrowNetwork/EF-Settings");
                    if (instance == null)
                    {
                        Debug.LogError("Please setup Easy Firebase!");
                    }
                }
                return instance;
            }
        }
        #endregion

    
        [HideInInspector] public List<DataItem> dataItems = new List<DataItem>();

        [HideInInspector] public string googleIdToken = "your-id-token";
        [SerializeField, HideInInspector] private string encryptedToken;

        private static readonly string encryptionKey = "n9mCV9fti0wFVGgVFDa0ufcUNu1Fn5Sh"; 

        public void EncryptGoogleIdToken()
        {
            if (string.IsNullOrEmpty(googleIdToken)) return;
            encryptedToken = EFEncryptor.Encrypt(googleIdToken, encryptionKey);
        }

        public string GetGoogleIdToken()
        {
            return string.IsNullOrEmpty(encryptedToken) ? "" : EFEncryptor.Decrypt(encryptedToken, encryptionKey);
        }

        public void Refresh()
        {
            
        }

    }

    [System.Serializable]
    public class DataItem
    {
        public DataType dataType;
        public string title;
        public string prefix;
    }
    public enum DataType
    {
        Object,
        String,
        Integer,
        Bool
    }
}
