using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

namespace EF.Generic
{
    public class EFManager : MonoBehaviour
    {
        #region Singleton
    
        private static EFManager _instance;
        public static EFManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<EFManager>();
    
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject("EasyFirebaseManager");
                        _instance = singletonObject.AddComponent<EFManager>();
                    }
    
                    DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }
    
        #endregion
    
        [Header("Data")]
        public EFSettings Settings;

        private Dictionary<string, string> _replacements = new Dictionary<string, string>();
        
        //Easy user for manage db operations
        private EasyUser user;
    
        private DatabaseReference reference;
        private FirebaseAuth auth;
    
        private void Awake()
        {
            reference = FirebaseDatabase.DefaultInstance.RootReference;
            auth = FirebaseAuth.DefaultInstance;

            auth.StateChanged += UserStateChanged;
        }
        
        #region State Changes

        private void UserStateChanged(object sender, EventArgs e)
        {
            if (auth.CurrentUser != null)
            {
                ReInitializeRePlacements();
                user = new EasyUser(auth.CurrentUser.UserId);
            }
            else
            {
                user = null;
            }
        }

        #endregion

        #region Prefix Replacer

        private void ReInitializeRePlacements()
        {
            string currentUserId = auth.CurrentUser != null ? auth.CurrentUser.UserId : "Anonymous";

            _replacements = new Dictionary<string, string>
            {
                { "Player", currentUserId }, 
                { "$Auth", "Admin" }   
            };
        }

        public string RePlacePrefix(string prefix)
        {
            Debug.Log("Initial prefix: " + prefix);
            Debug.Log("IsUserAvailable: " + IsUserAvailable());

            if (IsUserAvailable() == false)
            {
                Debug.LogWarning(Constants.AUTH_USER_NULL);
                return prefix;
            }

            foreach (var value in _replacements)
            {
                if (prefix.Contains(value.Key))
                {
                    Debug.Log("Replacing " + value.Key + " with " + value.Value);
                    prefix = prefix.Replace(value.Key, value.Value);
                }
                else
                {
                    Debug.LogWarning("Key not found in prefix: " + value.Key);
                }
            }

            Debug.Log("Final prefix: " + prefix);
            return prefix;
        }
    

        #endregion
    
        #region Gets

        public bool IsUserAvailable()
        {
            if (auth.CurrentUser != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    
        public DatabaseReference GetReference()
        {
            if (reference == null)
            {
                throw new Exception(Constants.DB_REFERENCE_NULL);
            }
            return reference;
        }
    
        public FirebaseAuth GetAuth()
        {
            if (auth == null)
            {
                throw new Exception(Constants.AUTH_REFERENCE_NULL);
            }
    
            return auth;
        }
        
        public EFSettings GetActiveSettings()
        {
            return Settings;
        }
    
        #endregion

        public void CheckUserAvailable()
        {
            if (!IsUserAvailable())
            {
                throw new EFException(Constants.AUTH_USER_NULL);
            }
        }
    }

}
