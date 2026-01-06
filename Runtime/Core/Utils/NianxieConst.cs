using System;
using UnityEngine;

namespace Nianxie.Utils
{
    public static class NianxieConst
    {
        public static class Ext
        {
            public const string CRAFT = "craft";
            public const string BUNDLE = "bundle";
        }

        public static Version MINI_VERSION = Version.Parse("0.0.1");
        public const string UNITY_VERSION = "2022.3.62f2c1";
        public static string StoragePath => Application.persistentDataPath + "/storage.db";
        public const string ShellResPath = "Assets/ShellRes";
        public const string MiniSceneName = "MiniScene";
        
        public const string MiniPrefixPath = "Assets/MiniProjects";
        public const string NianxieMiniPath = "Assets/NianxieMini";
        public static readonly string MiniTemplatesPath = $"{NianxieMiniPath}/Templates";
        public const string MiniBundlesOutput = "MiniBundles";
        
        public static readonly string TemplateSimpleGame = $"{MiniTemplatesPath}/simpleGame";
        public static readonly string TemplateSimpleCraft = $"{MiniTemplatesPath}/simpleCraft";

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

        public static string ToJsonString(this Version version)
        {
            return version.ToString(3);
        }
    }
}
