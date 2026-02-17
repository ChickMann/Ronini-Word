#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Commersion.EditorScripts
{
    [InitializeOnLoad]
    public class CommersionLegalCreatorEditor : EditorWindow
    {
        public static readonly Version CURRENT_VERSION = new Version(1, 0, 0);
        private const string SETTINGS_EXIST_KEY = "cmLegalSettingExist";
        private const string COMPANY_NAME_KEY = "cmCompanyName";
        private const string COMPANY_EMAIL_KEY = "cmCompanyEmail";
        private const string SUPPORT_EMAIL_KEY = "cmSupportEmail";
        private const string WEBSITE_URL_KEY = "cmWebsiteUrl";
        private const string CONTACT_ADDRESS_KEY = "cmContactAddress";

        private static bool isSettingExist;
        private Vector2 scrollPosition;

        // Company information fields
        private string companyName = "";
        private string companyEmail = "";
        private string supportEmail = "";
        private string websiteUrl = "";
        private string contactAddress = "";

        // Dark theme colors
        private static readonly Color DarkBackground = new Color(0.2f, 0.2f, 0.2f, 1f);
        private static readonly Color DarkerBackground = new Color(0.15f, 0.15f, 0.15f, 1f);
        private static readonly Color AccentBlue = new Color(0.2f, 0.6f, 1f, 1f);
        private static readonly Color AccentBlueHover = new Color(0.3f, 0.7f, 1f, 1f);
        private static readonly Color AccentGreen = new Color(0.2f, 0.8f, 0.4f, 1f);
        private static readonly Color AccentGreenHover = new Color(0.3f, 0.9f, 0.5f, 1f);
        private static readonly Color TextPrimary = new Color(0.9f, 0.9f, 0.9f, 1f);
        private static readonly Color TextSecondary = new Color(0.7f, 0.7f, 0.7f, 1f);
        private static readonly Color BorderColor = new Color(0.4f, 0.4f, 0.4f, 1f);

        [MenuItem("Tools/Commersion/Legal Document Creator")]
        private static void OpenLegalCreator()
        {
            CommersionLegalCreatorEditor[] existingWindows = Resources.FindObjectsOfTypeAll<CommersionLegalCreatorEditor>();
            if (existingWindows.Length > 0)
            {
                existingWindows[0].Focus();
                return;
            }

            var window = GetWindow<CommersionLegalCreatorEditor>("Legal Creator");
            window.minSize = new Vector2(500, 600);
            window.maxSize = new Vector2(600, 800);
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            companyName = EditorPrefs.GetString(COMPANY_NAME_KEY, "");
            companyEmail = EditorPrefs.GetString(COMPANY_EMAIL_KEY, "");
            supportEmail = EditorPrefs.GetString(SUPPORT_EMAIL_KEY, "");
            websiteUrl = EditorPrefs.GetString(WEBSITE_URL_KEY, "");
            contactAddress = EditorPrefs.GetString(CONTACT_ADDRESS_KEY, "");
            isSettingExist = EditorPrefs.GetBool(SETTINGS_EXIST_KEY, false);
        }

        private void SaveSettings()
        {
            EditorPrefs.SetString(COMPANY_NAME_KEY, companyName);
            EditorPrefs.SetString(COMPANY_EMAIL_KEY, companyEmail);
            EditorPrefs.SetString(SUPPORT_EMAIL_KEY, supportEmail);
            EditorPrefs.SetString(WEBSITE_URL_KEY, websiteUrl);
            EditorPrefs.SetString(CONTACT_ADDRESS_KEY, contactAddress);
            EditorPrefs.SetBool(SETTINGS_EXIST_KEY, true);
        }

        private void OnGUI()
        {
            // Set background color
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), DarkBackground);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.BeginVertical();

            // Header section
            DrawHeader();

            GUILayout.Space(20);

            // Company information form
            DrawCompanyForm();

            GUILayout.Space(20);

            // Action buttons section
            DrawActionButtons();

            GUILayout.Space(20);

            GUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            // Header background
            Rect headerRect = GUILayoutUtility.GetRect(0, 100, GUILayout.ExpandWidth(true));
            headerRect.x = 0;
            headerRect.width = position.width;
            EditorGUI.DrawRect(headerRect, DarkerBackground);

            // Draw border
            EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.y + headerRect.height - 1, headerRect.width, 1), BorderColor);

            GUILayout.BeginArea(headerRect);
            GUILayout.Space(20);

            // Title
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = TextPrimary },
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            GUILayout.Label($"Commersion Legal Creator v{CURRENT_VERSION}", titleStyle);

            // Subtitle
            GUIStyle subtitleStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = AccentBlue },
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Italic
            };

            GUILayout.Label("Generate Privacy Policy & Terms of Service", subtitleStyle);

            GUILayout.EndArea();
        }

        private void DrawCompanyForm()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical();

            // Form title
            GUIStyle sectionTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = TextPrimary },
                fontSize = 16,
                alignment = TextAnchor.MiddleLeft
            };

            GUILayout.Label("📋 Company Information", sectionTitleStyle);
            GUILayout.Space(10);

            // Company Name
            DrawInputField("Company Name *", ref companyName, "Enter your company name");

            // Company Email
            DrawInputField("Company Email *", ref companyEmail, "Enter your company email");

            // Support Email
            DrawInputField("Support Email *", ref supportEmail, "Enter your support email");

            // Website URL
            DrawInputField("Website URL", ref websiteUrl, "https://yourwebsite.com");

            // Contact Address
            DrawTextAreaField("Contact Address", ref contactAddress, "Enter your company address\n(Street, City, State, Country)");

            GUILayout.Space(10);

            // Validation message
            if (!IsFormValid())
            {
                GUIStyle warningStyle = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = Color.yellow },
                    fontSize = 12,
                    wordWrap = true
                };
                GUILayout.Label("⚠️ Please fill in all required fields marked with *", warningStyle);
            }

            GUILayout.EndVertical();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
        }

        private void DrawInputField(string label, ref string value, string placeholder)
        {
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = TextSecondary },
                fontSize = 12
            };

            GUILayout.Label(label, labelStyle);

            // Create custom input field background
            Color inputBg = new Color(0.3f, 0.3f, 0.3f, 1f);
            Color inputBorder = new Color(0.5f, 0.5f, 0.5f, 1f);
            
            GUIStyle textFieldStyle = new GUIStyle(EditorStyles.textField)
            {
                normal = { 
                    textColor = TextPrimary,
                    background = MakeTex(2, 2, inputBg)
                },
                focused = {
                    textColor = TextPrimary,
                    background = MakeTex(2, 2, new Color(0.35f, 0.35f, 0.35f, 1f))
                },
                fontSize = 12,
                padding = new RectOffset(10, 10, 8, 8),
                border = new RectOffset(1, 1, 1, 1),
                fixedHeight = 25
            };

            // Draw border
            Rect fieldRect = GUILayoutUtility.GetRect(0, 27, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(fieldRect, inputBorder);
            EditorGUI.DrawRect(new Rect(fieldRect.x + 1, fieldRect.y + 1, fieldRect.width - 2, fieldRect.height - 2), inputBg);

            // Draw input field
            string displayValue = string.IsNullOrEmpty(value) ? placeholder : value;
            Color originalColor = GUI.color;
            
            if (string.IsNullOrEmpty(value))
            {
                GUI.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            }

            string newValue = EditorGUI.TextField(new Rect(fieldRect.x + 1, fieldRect.y + 1, fieldRect.width - 2, fieldRect.height - 2), displayValue, textFieldStyle);
            
            GUI.color = originalColor;

            if (string.IsNullOrEmpty(value) && newValue != placeholder)
            {
                value = newValue;
            }
            else if (!string.IsNullOrEmpty(value))
            {
                value = newValue;
            }

            GUILayout.Space(8);
        }

        private void DrawTextAreaField(string label, ref string value, string placeholder)
        {
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = TextSecondary },
                fontSize = 12
            };

            GUILayout.Label(label, labelStyle);

            // Create custom text area background
            Color inputBg = new Color(0.3f, 0.3f, 0.3f, 1f);
            Color inputBorder = new Color(0.5f, 0.5f, 0.5f, 1f);

            GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea)
            {
                normal = { 
                    textColor = TextPrimary,
                    background = MakeTex(2, 2, inputBg)
                },
                focused = {
                    textColor = TextPrimary,
                    background = MakeTex(2, 2, new Color(0.35f, 0.35f, 0.35f, 1f))
                },
                fontSize = 12,
                padding = new RectOffset(10, 10, 8, 8),
                border = new RectOffset(1, 1, 1, 1),
                wordWrap = true
            };

            // Draw border
            Rect fieldRect = GUILayoutUtility.GetRect(0, 62, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(fieldRect, inputBorder);
            EditorGUI.DrawRect(new Rect(fieldRect.x + 1, fieldRect.y + 1, fieldRect.width - 2, fieldRect.height - 2), inputBg);

            // Draw text area
            string displayValue = string.IsNullOrEmpty(value) ? placeholder : value;
            Color originalColor = GUI.color;
            
            if (string.IsNullOrEmpty(value))
            {
                GUI.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            }

            string newValue = EditorGUI.TextArea(new Rect(fieldRect.x + 1, fieldRect.y + 1, fieldRect.width - 2, fieldRect.height - 2), displayValue, textAreaStyle);
            
            GUI.color = originalColor;

            if (string.IsNullOrEmpty(value) && newValue != placeholder)
            {
                value = newValue;
            }
            else if (!string.IsNullOrEmpty(value))
            {
                value = newValue;
            }

            GUILayout.Space(8);
        }

        private bool IsFormValid()
        {
            return !string.IsNullOrEmpty(companyName) &&
                   !string.IsNullOrEmpty(companyEmail) &&
                   !string.IsNullOrEmpty(supportEmail);
        }

        private void DrawActionButtons()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical();

            // Generate Documents button
            GUI.enabled = IsFormValid();
            if (DrawStyledButton("📄 Generate Legal Documents", AccentGreen, AccentGreenHover, 16))
            {
                GenerateLegalDocuments();
            }
            GUI.enabled = true;

            GUILayout.Space(10);

            // Save Settings button
            if (DrawStyledButton("💾 Save Settings", AccentBlue, AccentBlueHover, 14))
            {
                SaveSettings();
                ShowNotification(new GUIContent("✅ Settings saved successfully!"));
            }

            GUILayout.Space(10);

            // Clear Settings button
            if (DrawStyledButton("🗑️ Clear All", new Color(0.8f, 0.3f, 0.3f), new Color(0.9f, 0.4f, 0.4f), 14))
            {
                if (EditorUtility.DisplayDialog("Clear Settings", "Are you sure you want to clear all settings?", "Yes", "Cancel"))
                {
                    ClearSettings();
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
        }

        private void GenerateLegalDocuments()
        {
            try
            {
                CreateDirectoriesIfNeeded();
                GeneratePrivacyPolicy();
                GenerateTermsOfService();
                SaveSettings();
                
                ShowNotification(new GUIContent("✅ Legal documents generated successfully!"));
                
                // Open the folder in the project window
                EditorUtility.RevealInFinder(Path.Combine(Application.dataPath, "Resources/Commersion/Examples"));
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error generating legal documents: {e.Message}");
                ShowNotification(new GUIContent("❌ Error generating documents!"));
            }
        }

        private void CreateDirectoriesIfNeeded()
        {
            string resourcesPath = Path.Combine(Application.dataPath, "Resources");
            string commersionPath = Path.Combine(resourcesPath, "Commersion");
            string examplesPath = Path.Combine(commersionPath, "Examples");

            if (!Directory.Exists(resourcesPath))
                Directory.CreateDirectory(resourcesPath);
            
            if (!Directory.Exists(commersionPath))
                Directory.CreateDirectory(commersionPath);
            
            if (!Directory.Exists(examplesPath))
                Directory.CreateDirectory(examplesPath);
        }

        private void GeneratePrivacyPolicy()
        {
            string template = GetPrivacyPolicyTemplate();
            string content = ReplacePlaceholders(template);
            string filePath = Path.Combine(Application.dataPath, "Resources/Commersion/Examples/PrivacyPolicy.txt");
            File.WriteAllText(filePath, content);
        }

        private void GenerateTermsOfService()
        {
            string template = GetTermsOfServiceTemplate();
            string content = ReplacePlaceholders(template);
            string filePath = Path.Combine(Application.dataPath, "Resources/Commersion/Examples/TermsOfService.txt");
            File.WriteAllText(filePath, content);
        }

        private string ReplacePlaceholders(string template)
        {
            string currentDate = DateTime.Now.ToString("MMMM dd, yyyy");
            
            return template
                .Replace("[COMPANY_NAME]", companyName)
                .Replace("[COMPANY_EMAIL]", companyEmail)
                .Replace("[SUPPORT_EMAIL]", supportEmail)
                .Replace("[WEBSITE_URL]", string.IsNullOrEmpty(websiteUrl) ? "our website" : websiteUrl)
                .Replace("[CONTACT_ADDRESS]", string.IsNullOrEmpty(contactAddress) ? "Please contact us via email" : contactAddress)
                .Replace("[DATE]", currentDate);
        }

        private void ClearSettings()
        {
            companyName = "";
            companyEmail = "";
            supportEmail = "";
            websiteUrl = "";
            contactAddress = "";
            
            EditorPrefs.DeleteKey(COMPANY_NAME_KEY);
            EditorPrefs.DeleteKey(COMPANY_EMAIL_KEY);
            EditorPrefs.DeleteKey(SUPPORT_EMAIL_KEY);
            EditorPrefs.DeleteKey(WEBSITE_URL_KEY);
            EditorPrefs.DeleteKey(CONTACT_ADDRESS_KEY);
            EditorPrefs.DeleteKey(SETTINGS_EXIST_KEY);
            
            ShowNotification(new GUIContent("🗑️ All settings cleared!"));
        }

        private bool DrawStyledButton(string text, Color normalColor, Color hoverColor, int fontSize)
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                normal = {
                    background = MakeTex(2, 2, normalColor),
                    textColor = Color.white
                },
                hover = {
                    background = MakeTex(2, 2, hoverColor),
                    textColor = Color.white
                },
                active = {
                    background = MakeTex(2, 2, normalColor * 0.8f),
                    textColor = Color.white
                },
                fontSize = fontSize,
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true,
                padding = new RectOffset(15, 15, 12, 12),
                border = new RectOffset(8, 8, 8, 8),
                fontStyle = FontStyle.Bold
            };

            Rect buttonRect = GUILayoutUtility.GetRect(0, 45, GUILayout.ExpandWidth(true));
            Rect shadowRect = new Rect(buttonRect.x + 2, buttonRect.y + 2, buttonRect.width, buttonRect.height);
            EditorGUI.DrawRect(shadowRect, new Color(0, 0, 0, 0.3f));

            return GUI.Button(buttonRect, text, buttonStyle);
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pix);
            texture.Apply();
            return texture;
        }

        private string GetPrivacyPolicyTemplate()
        {
            return @"PRIVACY POLICY

Last updated: [DATE]

This Privacy Policy describes how [COMPANY_NAME] (""we,"" ""our,"" or ""us"") collects, uses, and protects your information when you use our mobile application and related services (the ""Service"").

INFORMATION WE COLLECT

Personal Information
We may collect the following types of personal information:
• Email address
• Name and contact information
• Device information and identifiers
• Usage data and analytics
• In-app purchase information

Non-Personal Information
We may collect non-personal information such as:
• Device type and operating system
• App usage statistics
• Crash reports and performance data

HOW WE USE YOUR INFORMATION

We use the collected information for:
• Providing and maintaining our Service
• Improving user experience
• Sending important updates and notifications
• Processing transactions
• Providing customer support
• Complying with legal obligations

DATA SHARING AND DISCLOSURE

We do not sell, trade, or otherwise transfer your personal information to third parties except:
• With your explicit consent
• To service providers who assist us in operating our app
• When required by law or legal process
• To protect our rights and safety

THIRD-PARTY SERVICES

Our app may integrate with third-party services such as:
• Analytics providers (Google Analytics, Firebase)
• Advertising networks
• Social media platforms
• Payment processors

These services have their own privacy policies, and we encourage you to review them.

DATA SECURITY

We implement appropriate security measures to protect your information against unauthorized access, alteration, disclosure, or destruction. However, no method of transmission over the internet is 100% secure.

CHILDREN'S PRIVACY

Our Service is not intended for children under 13. We do not knowingly collect personal information from children under 13. If we become aware that we have collected such information, we will take steps to delete it.

YOUR RIGHTS

Depending on your location, you may have the right to:
• Access your personal information
• Correct inaccurate information
• Delete your information
• Restrict processing
• Data portability
• Object to processing

CHANGES TO THIS POLICY

We may update this Privacy Policy from time to time. We will notify you of any changes by posting the new Privacy Policy in the app and updating the ""Last updated"" date.

CONTACT US

If you have any questions about this Privacy Policy, please contact us:
• Email: [SUPPORT_EMAIL]
• Company: [COMPANY_NAME]
• Address: [CONTACT_ADDRESS]

This Privacy Policy is effective as of the date stated above and will remain in effect except with respect to any changes in its provisions in the future.";
        }

        private string GetTermsOfServiceTemplate()
        {
            return @"TERMS OF SERVICE

Last updated: [DATE]

These Terms of Service (""Terms"") govern your use of the mobile application operated by [COMPANY_NAME] (""we,"" ""our,"" or ""us""). By downloading, accessing, or using our app, you agree to be bound by these Terms.

ACCEPTANCE OF TERMS

By using our Service, you confirm that:
• You are at least 13 years old
• You have the legal capacity to enter into these Terms
• You agree to comply with all applicable laws and regulations

DESCRIPTION OF SERVICE

Our mobile application provides [BRIEF DESCRIPTION OF YOUR APP'S FUNCTIONALITY]. We reserve the right to modify, suspend, or discontinue any aspect of the Service at any time.

USER ACCOUNTS AND REGISTRATION

• You may need to create an account to access certain features
• You are responsible for maintaining the security of your account
• You must provide accurate and complete information
• You are responsible for all activities under your account

ACCEPTABLE USE

You agree NOT to:
• Use the Service for illegal or unauthorized purposes
• Violate any laws, regulations, or third-party rights
• Upload malicious code or harmful content
• Attempt to gain unauthorized access to our systems
• Interfere with or disrupt the Service
• Use the Service to spam or harass others

INTELLECTUAL PROPERTY

• All content and materials in the app are owned by [COMPANY_NAME] or our licensors
• You may not copy, modify, distribute, or create derivative works
• Any feedback or suggestions you provide may be used by us without compensation
• You retain ownership of content you create using our Service

PURCHASES AND PAYMENTS

• In-app purchases are processed through your device's app store
• All purchases are final and non-refundable unless required by law
• Prices are subject to change without notice
• We are not responsible for app store policies or payment processing

PRIVACY

Your privacy is important to us. Please review our Privacy Policy, which governs how we collect, use, and protect your information.

DISCLAIMERS

THE SERVICE IS PROVIDED ""AS IS"" WITHOUT WARRANTIES OF ANY KIND. WE DISCLAIM ALL WARRANTIES, EXPRESS OR IMPLIED, INCLUDING:
• MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
• NON-INFRINGEMENT
• ACCURACY OR RELIABILITY OF CONTENT
• UNINTERRUPTED OR ERROR-FREE OPERATION

LIMITATION OF LIABILITY

TO THE MAXIMUM EXTENT PERMITTED BY LAW, [COMPANY_NAME] SHALL NOT BE LIABLE FOR:
• INDIRECT, INCIDENTAL, OR CONSEQUENTIAL DAMAGES
• LOSS OF PROFITS, DATA, OR USE
• DAMAGES EXCEEDING THE AMOUNT PAID TO US IN THE LAST 12 MONTHS

INDEMNIFICATION

You agree to defend, indemnify, and hold us harmless from any claims, damages, or expenses arising from your use of the Service or violation of these Terms.

TERMINATION

• You may stop using the Service at any time
• We may suspend or terminate your access for violations of these Terms
• Certain provisions will survive termination

GOVERNING LAW

These Terms are governed by the laws of [YOUR JURISDICTION] without regard to conflict of law provisions.

CHANGES TO TERMS

We may modify these Terms at any time. We will notify users of significant changes through the app or other means. Continued use constitutes acceptance of the modified Terms.

CONTACT INFORMATION

For questions about these Terms, please contact us:
• Email: [SUPPORT_EMAIL]
• Company: [COMPANY_NAME]
• Address: [CONTACT_ADDRESS]

SEVERABILITY

If any provision of these Terms is found to be unenforceable, the remaining provisions will remain in full force and effect.

These Terms constitute the entire agreement between you and [COMPANY_NAME] regarding the Service and supersede all prior agreements.";
        }
    }
}

#endif