using System;
using System.Collections;
using System.Collections.Generic;
using EF.Database;
using EF.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EF.Demo
{
    public class EFDemoScript : MonoBehaviour
    {
        [Header("Sign In")] 
        [SerializeField] private TMP_InputField si_email;
        [SerializeField] private TMP_InputField si_password;
        
        [Header("Sign Up")]
        [SerializeField] private TMP_InputField sg_email;
        [SerializeField] private TMP_InputField sg_password;
        [SerializeField] private TMP_InputField sg_repassword;
        
        
        [SerializeField] private SimpleAuth auth;
    
        public SimpleData data;

        private async void Start()
        {
            auth.Initialize();
        }
    
        #region Methods
    
        public async void SignIn()
        {
           var result = await auth.SignIn();
        }
    
        #endregion
    }

}
