using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using EF.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using Google;
using TMPro;
using UnityEngine;

namespace EF.Generic
{
    [Serializable]
    public class SimpleAuth
    {
        [SerializeField] private AuthType authType = AuthType.EmailSignIn;

        private FirebaseAuth auth;

        //If authtype equals to email, then email and password will be used for authentication.
        [SerializeField] public SignInWithEmail signInWithEmail;

        #region Initialize

        public void Initialize()
        {
            auth = EFManager.Instance.GetAuth();
        }

        #endregion

        #region Details

        private async Task<AuthResult> SignInWithEmailAsync(string email, string password)
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    throw new EFException(Constants.AUTH_FAIL);
                }
                return task.Result;
            });

            if (result.User != null)
            {
                return new AuthResult(true, result.User.UserId);
            }
            else
            {
                return new AuthResult(false, "");
            }
        }

        private async Task<AuthResult> SignUpWithEmailAsync(string email, string password)
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    throw new EFException(Constants.AUTH_FAIL);
                }
                return task.Result;
            });

            if (result.User != null)
            {
                return new AuthResult(true, result.User.UserId);
            }
            else
            {
                return new AuthResult(false, "");
            }
        }


        #endregion

        #region Public API

        public async Task<SignInResult> SignIn()
        {
            if (auth == null)
            {
                throw new EFException(Constants.AUTH_REFERENCE_NULL);
            }

            var settings = EFManager.Instance.Settings;

            if (settings == null)
            {
                return new SignInResult { Ok = false, errorMessage = "Settings are null." };
            }

            switch (authType)
            {
                case AuthType.EmailSignIn:
                    {
                        if (signInWithEmail.emailInputField == null || signInWithEmail.passwordInputField == null)
                        {
                            return new SignInResult { Ok = false, errorMessage = "Email or password input fields are not assigned." };
                        }

                        string email = signInWithEmail.emailInputField?.text;
                        string repeatEmail = signInWithEmail.repeatEmailInputField?.text;
                        string password = signInWithEmail.passwordInputField?.text;

                        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                        {
                            return new SignInResult { Ok = false, errorMessage = "Email or password cannot be empty." };
                        }

                        if (signInWithEmail.useRepeatEmail)
                        {
                            if (string.IsNullOrEmpty(repeatEmail))
                            {
                                return new SignInResult { Ok = false, errorMessage = "Repeat email field is empty." };
                            }
                            if (!email.Equals(repeatEmail, StringComparison.OrdinalIgnoreCase))
                            {
                                return new SignInResult { Ok = false, errorMessage = "Emails do not match." };
                            }
                        }

                        try
                        {
                            var user = await auth.SignInWithEmailAndPasswordAsync(email, password);
                            Debug.Log("User signed in with email: " + user.User.Email);
                            return new SignInResult { Ok = true};
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("Email sign-in failed: " + ex);
                            return new SignInResult { Ok = false, errorMessage = ex.Message };
                        }
                    }
                case AuthType.EmailSignUp:
                    {
                        if (signInWithEmail.emailInputField == null || signInWithEmail.passwordInputField == null)
                        {
                            return new SignInResult { Ok = false, errorMessage = "Email or password input fields are not assigned." };
                        }

                        string email = signInWithEmail.emailInputField?.text;
                        string repeatEmail = signInWithEmail.repeatEmailInputField?.text;
                        string password = signInWithEmail.passwordInputField?.text;

                        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                        {
                            return new SignInResult { Ok = false, errorMessage = "Email or password cannot be empty." };
                        }

                        if (signInWithEmail.useRepeatEmail)
                        {
                            if (string.IsNullOrEmpty(repeatEmail))
                            {
                                return new SignInResult { Ok = false, errorMessage = "Repeat email field is empty." };
                            }
                            if (!email.Equals(repeatEmail, StringComparison.OrdinalIgnoreCase))
                            {
                                return new SignInResult { Ok = false, errorMessage = "Emails do not match." };
                            }
                        }

                        try
                        {
                            var user = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
                            Debug.Log("User signed up with email: " + user.User.Email);
                            return new SignInResult { Ok = true };
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("Email sign-up failed: " + ex);
                            return new SignInResult { Ok = false, errorMessage = ex.Message };
                        }
                    }



                case AuthType.Google:
#if GOOGLE_SIGN_IN
                    string webClientId = settings.GetGoogleIdToken();
                    if (string.IsNullOrEmpty(webClientId))
                    {
                        Debug.LogError("Google ID Token is not set. Please set it in the settings.");
                        return new SignInResult { Ok = false, errorMessage = "Google ID Token is not set." };
                    }

                    var configuration = new GoogleSignInConfiguration
                    {
                        WebClientId = webClientId,
                        RequestIdToken = true
                    };

                    GoogleSignIn.Configuration = configuration;

                    try
                    {
                        var googleUser = await GoogleSignIn.DefaultInstance.SignIn();

                        if (googleUser == null)
                        {
                            Debug.LogError("Google Sign-In returned null user.");
                            return new SignInResult { Ok = false, errorMessage = "Google Sign-In returned null user." };
                        }

                        string idToken = googleUser.IdToken;
                        if (string.IsNullOrEmpty(idToken))
                        {
                            Debug.LogError("Google Sign-In returned empty ID Token.");
                            return new SignInResult { Ok = false, errorMessage = "Google Sign-In returned empty ID Token." };
                        }

                        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

                        var firebaseUser = await auth.SignInWithCredentialAsync(credential);

                        Debug.Log("User signed in: " + firebaseUser.DisplayName);

                        return new SignInResult { Ok = true};
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Google Sign-In failed: " + ex);
                        return new SignInResult { Ok = false, errorMessage = ex.Message };
                    }
#endif

                case AuthType.Anonymous:
                    try
                    {
                        var anonUser = await auth.SignInAnonymouslyAsync();
                        Debug.Log("Anonymous user signed in: " + anonUser.User.UserId);
                        return new SignInResult { Ok = true};
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Anonymous sign-in failed: " + ex);
                        return new SignInResult { Ok = false, errorMessage = ex.Message };
                    }

                default:
                    return new SignInResult { Ok = false, errorMessage = "Unknown auth type." };
            }
        }

        #endregion

        [Serializable]
        public enum AuthType
        {
            EmailSignIn,
            EmailSignUp,
            Google,
            Anonymous
        }

        [Serializable]
        public struct SignInWithEmail
        {
            [SerializeField] public bool useRepeatEmail;
            [SerializeField] public TMP_InputField emailInputField;
            [SerializeField] public TMP_InputField repeatEmailInputField;
            [SerializeField] public TMP_InputField passwordInputField;
        }


        public struct SignInResult
        {
            public bool Ok;
            public string errorMessage;
        }
    }

}
