namespace EF.Generic
{
#if UNITY_EDITOR
    using UnityEditor;

    public static class GoogleSignInDefineSetter
    {
        private const string defineSymbol = "GOOGLE_SIGN_IN";

        public static void EnableGoogleSignInDefine()
        {
            BuildTargetGroup targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            if (!defines.Contains(defineSymbol))
            {
                defines += ";" + defineSymbol;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
            }
        }

        public static void DisableGoogleSignInDefine()
        {
            BuildTargetGroup targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            if (defines.Contains(defineSymbol))
            {
                defines = defines.Replace(defineSymbol, "").Replace(";;", ";").Trim(';');
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
            }
        }
    }
#endif

}
