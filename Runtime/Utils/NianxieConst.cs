using UnityEngine;

namespace Nianxie.Utils
{
    public static class NianxieConst
    {
        public const int MINI_VERSION = 20250904;
        public static string StoragePath => Application.persistentDataPath + "/storage.db";
        public const string ShellResPath = "Assets/ShellRes";
        public const string MiniSceneName = "MiniScene";
        
        public const string MiniPrefixPath = "Assets/MiniProjects";
        private const string NianxieMiniPath = "Assets/NianxieMini";
        public static readonly string MiniTemplatesPath = $"{NianxieMiniPath}/Templates";
        public const string MiniBundlesOutput = "MiniBundles";
        
        public static readonly string TemplateSimple = $"{MiniTemplatesPath}/simple";
        
        // some resource
        public static string MiniDefaultAssets = $"{NianxieMiniPath}/DefaultAssets";
        public static string MiniBootPath = $"{MiniDefaultAssets}/miniBoot.txt";
        public static string Sliced9Path = $"{MiniDefaultAssets}/sliced9.png";
    }
}
