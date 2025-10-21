using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEditor;
using UnityEngine;
using XLua;

namespace Nianxie.Editor
{
    public partial class MiniEditorEnvPaths : EditorEnvPaths
    {
        private static readonly Dictionary<string, MiniEditorEnvPaths> cache = new();
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
        public static readonly ReadOnlyDictionary<string, MiniEditorEnvPaths> readOnlyCache = new (cache);
        protected override EditorReflectEnv CreateReflectEnv()
        {
            Debug.Log($"mini refresh editor reflect env : {pathPrefix}");
            var miniBootBytes = AssetDatabase.LoadAssetAtPath<TextAsset>(NianxieConst.MiniBootPath).bytes;
            return EditorReflectEnv.Create(this, miniBootBytes);
        }
        
        
        private readonly string buildDir;
        public readonly string finalManifest;
        public Dictionary<BuildTarget, string> finalBundleDict { get; }

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
                if (!_config.CheckScriptsMatch(luaAssetPaths) || !_config.CheckVersionMatch())
                {
                    _config = new MiniProjectConfig(luaAssetPaths, _config.name, _config.craftable);
                    if (Directory.Exists(pathPrefix))
                    {
                        File.WriteAllBytes(miniProjectConfig, _config.ToJson());
                        AssetDatabase.Refresh();
                    }
                }
            } else
            {
                _config = new MiniProjectConfig(luaAssetPaths, "???", false);
                if (Directory.Exists(pathPrefix))
                {
                    File.WriteAllBytes(miniProjectConfig, _config.ToJson());
                    AssetDatabase.Refresh();
                }
            }
        }

        public void FlushName(string name)
        {
            _config = new MiniProjectConfig(_config)
            {
                name = name,
            };
            File.WriteAllBytes(miniProjectConfig, _config.ToJson());
            AssetDatabase.Refresh();
        }
        public string GetCachedName()
        {
            return config.name;
        }
    }
}
