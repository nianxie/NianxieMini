using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Nianxie.Components;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEditor;
using UnityEngine;
using XLua;

namespace Nianxie.Editor
{
    public partial class MiniEditorEnvPaths : EditorEnvPaths
    {
        private static Dictionary<string, MiniEditorEnvPaths> cache = new();
        public static BuildTarget[] BuildTargets { get; } = {BuildTarget.iOS, BuildTarget.Android};
        public static MiniEditorEnvPaths Get(string folder)
        {
            if (!cache.TryGetValue(folder, out var envPaths))
            {
                envPaths = new MiniEditorEnvPaths(folder);
                cache[folder] = envPaths;
                envPaths.RefreshProjectConfig();
            }
            return envPaths;
        }
        protected override EditorReflectEnv CreateReflectEnv()
        {
            Debug.Log($"mini refresh editor reflect env : {pathPrefix}");
            var miniBootBytes = AssetDatabase.LoadAssetAtPath<TextAsset>(NianxieConst.MiniBootPath).bytes;
            return EditorReflectEnv.Create(this, miniBootBytes);
        }
        
        
        private readonly string buildDir;
        public readonly string finalManifest;
        public Dictionary<BuildTarget, string> finalBundleDict { get; }

        public string miniId => AssetDatabase.AssetPathToGUID(pathPrefix);
        public readonly string folder;
        private MiniEditorEnvPaths(string folder):base(folder)
        {
            this.folder = folder;
            buildDir = $"{NianxieConst.MiniBundlesOutput}/{folder}";
            finalManifest = $"{buildDir}/{folder}.json";
            finalBundleDict = BuildTargets.ToDictionary(t=>t, GetPlatformFinalBundle);
        }

        private MiniProjectConfig _config;

        public MiniProjectConfig config => _config ?? MiniProjectConfig.ErrorInstance;

        public void RefreshProjectConfig()
        {
            var luaAssetPaths = collectScriptDict.Keys.Select(a => assetPath2relativePath(a)).ToArray();
            var configTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(miniProjectConfig);
            if (configTextAsset != null)
            {
                try
                {
                    _config = MiniProjectConfig.FromJson(configTextAsset.bytes);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"json parse error in {miniProjectConfig} : {e}");
                    _config = null;
                    return ;
                }
                if (!_config.CheckMatch(luaAssetPaths))
                {
                    _config.scripts = luaAssetPaths;
                    if (Directory.Exists(pathPrefix))
                    {
                        File.WriteAllBytes(miniProjectConfig, _config.ToJson());
                        AssetDatabase.Refresh();
                    }
                }
            } else
            {
                _config = new MiniProjectConfig
                {
                    scripts = luaAssetPaths,
                    name = "(default)",
                    version = NianxieConst.MINI_VERSION,
                    craft = false,
                };
                if (Directory.Exists(pathPrefix))
                {
                    File.WriteAllBytes(miniProjectConfig, _config.ToJson());
                    AssetDatabase.Refresh();
                }
            }
        }

        public void UpdateProjectConfig(DB_Mini dbMini)
        {
            _config.name = dbMini.name;
            _config.craft = dbMini.craft;
            if (Directory.Exists(pathPrefix))
            {
                File.WriteAllBytes(miniProjectConfig, _config.ToJson());
                AssetDatabase.Refresh();
            }
        }
        public string GetCachedName()
        {
            return config.name;
        }
    }
}
