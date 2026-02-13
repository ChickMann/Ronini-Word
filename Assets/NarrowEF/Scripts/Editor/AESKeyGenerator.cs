using System.Text;
using UnityEditor;
using UnityEngine;

namespace EF.Generic
{
    public class AESKeyGenerator : MonoBehaviour
    {
        [MenuItem("Tools/Easy Firebase/Generate AES Key")]
        public static void GenerateAESKey()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 32; i++)
            {
                sb.Append(chars[Random.Range(0, chars.Length)]);
            }

            string key = sb.ToString();
            Debug.Log("Generated AES Key (32 chars): " + key);
            EditorGUIUtility.systemCopyBuffer = key;
            Debug.Log("📋 Key copied to clipboard!");
        }
    }
}