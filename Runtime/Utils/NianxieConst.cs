using System;
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
        
        public static readonly string TemplateSimpleGame = $"{MiniTemplatesPath}/simpleGame";
        public static readonly string TemplateSimpleCraft = $"{MiniTemplatesPath}/simpleCraft";
        
        // some resource
        public static string MiniDefaultAssets = $"{NianxieMiniPath}/DefaultAssets";
        public static string MiniBootPath = $"{MiniDefaultAssets}/miniBoot.txt";
        public static string Sliced9Path = $"{MiniDefaultAssets}/sliced9.png";

        public const string ConfigTxt = "config.txt";
        
        /// <summary>
        /// 通过config.txt的路径来定位mini project folder的名字
        /// assets/miniprojects/{folder}/config.txt
        /// </summary>
        /// <param name="bundle"></param>
        /// <returns>folder名</returns>
        public static string CheckMiniFolder(this AssetBundle bundle)
        {
            var prefix = $"{MiniPrefixPath}/".ToLower();
            var suffix = $"/{ConfigTxt}";
            string folder = null;
            foreach (var assetName in bundle.GetAllAssetNames())
            {
                var assetNameLower = assetName.ToLower();
                if (assetNameLower.StartsWith(prefix) && assetNameLower.EndsWith(suffix))
                {
                    var arr = assetName.Split("/");
                    if (arr.Length == 4)
                    {
                        if (folder != null)
                        {
                            throw new Exception($"more then one possible folder : {folder} {arr[2]} in AssetBundle");
                        }
                        folder = arr[2];
                    }
                }
            }
            if (folder == null)
            {
                throw new Exception("no valid folder in AssetBundle");
            }
            return folder;
        }
    }
}
