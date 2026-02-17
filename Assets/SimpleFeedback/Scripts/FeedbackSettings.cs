using UnityEngine;
using System.Collections.Generic;

namespace FeedbackForm
{
    public class FeedbackSettings : MonoBehaviour
    {
        // Store Trello list IDs and credentials for API interactions.
        private string defaultListId;
        private string apiKey;
        private string apiToken;

        // Dictionary to map category names to list IDs.
        private Dictionary<string, string> categoryListMapping;

        // Initialize by loading the settings.
        private void Start()
        {
            LoadSettings();
        }

        // Load settings from a secure source (e.g., environment variables, encrypted file).
        private void LoadSettings()
        {
            // Default list ID for Trello, if not specified per category.
            defaultListId = "YOUR_DEFAULT_LIST_ID";

            // API Key and Token (should be stored securely in production).
            apiKey = "YOUR_API_KEY_HERE";
            apiToken = "YOUR_API_TOKEN_HERE";

            // Initialize the dictionary with category to list ID mapping.
            categoryListMapping = new Dictionary<string, string>
            {
                { "Bugs", "BUGS_LIST_ID" },      // Replace with actual ID for 'Bugs'
                { "Opinion", "OPINION_LIST_ID" }, // Replace with actual ID for 'Opinion'
                { "Another", "OTHER_LIST_ID" }      // Replace with actual ID for 'Other'
            };
        }

        // Get the Trello list ID based on the provided category.
        public string GetTrelloListId(string category)
        {
            // Check if the category exists in the dictionary, otherwise return the default list ID.
            if (categoryListMapping.TryGetValue(category, out var id))
            {
                return id;
            }

            // Return the default list ID if the category is not found.
            return defaultListId;
        }

        // Return the Trello API key.
        public string GetTrelloApiKey() => apiKey;

        // Return the Trello API token.
        public string GetTrelloApiToken() => apiToken;
    }
}
