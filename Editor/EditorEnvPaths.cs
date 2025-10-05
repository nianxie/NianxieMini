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
    public abstract class EditorEnvPaths : EnvPaths
    {
        protected abstract EditorReflectEnv CreateReflectEnv();

        private SortedDictionary<string, CollectScript> _collectScriptDict;
        public SortedDictionary<string, CollectScript> collectScriptDict
        {
            get
            {
                if (_collectScriptDict == null)
                {
                    _collectScriptDict = CollectScript.Collect(this);
                }
                return _collectScriptDict;
            }
        }
        private EditorReflectEnv _reflectEnv;
        public EditorReflectEnv reflectEnv {
            get
            {
                if (_reflectEnv == null)
                {
                    _reflectEnv = CreateReflectEnv();
                }
                return _reflectEnv;
            }
        }

        public void SetObsolete()
        {
            // 清理掉当前的script和env，用的时候以lazy方式加载
            _collectScriptDict = null;
            _reflectEnv?.Dispose();
            _reflectEnv = null;
        }

        protected EditorEnvPaths() : base()
        {
        }

        protected EditorEnvPaths(string miniId) : base(miniId)
        {
        }
        public static bool TryMapEnvPaths(string assetPath, out EditorEnvPaths envPaths)
        {
            if (assetPath.StartsWith(NianxieConst.ShellResPath))
            {
                envPaths = ShellEditorEnvPaths.Instance;
                return true;
            }
            if (assetPath.StartsWith(NianxieConst.MiniPrefixPath))
            {
                var splitArr = assetPath.Split("/");
                if (splitArr.Length >= 3 && !string.IsNullOrEmpty(splitArr[2]))
                {
                    var miniId = splitArr[2];
                    var miniEnvPaths = MiniEditorEnvPaths.Get(miniId);
                    envPaths = miniEnvPaths;
                    return true;
                }
            }
            envPaths = null;
            return false;
        }
    }

    public class ShellEditorEnvPaths : EditorEnvPaths
    {
        public static ShellEditorEnvPaths Instance = new ShellEditorEnvPaths();

        protected override EditorReflectEnv CreateReflectEnv()
        {
            Debug.Log($"shell refresh editor reflect env : {pathPrefix}");
            return EditorReflectEnv.Create(this, null);
        }
    }
    public class MiniEditorEnvPaths : EditorEnvPaths
    {
        protected override EditorReflectEnv CreateReflectEnv()
        {
            Debug.Log($"mini refresh editor reflect env : {pathPrefix}");
            var miniBootBytes = AssetDatabase.LoadAssetAtPath<TextAsset>(NianxieConst.MiniBootPath).bytes;
            return EditorReflectEnv.Create(this, miniBootBytes);
        }
        
        public const string BUNDLE_EXT = "bundle";
        // webp类型的build暂时先不处理
        public const string WBUNDLE_EXT = "wbundle";
        private const string MiniBundlesOutput = NianxieConst.MiniBundlesOutput;
        public static BuildTarget[] BuildTargets { get; } = {BuildTarget.iOS, BuildTarget.Android};
        private static Dictionary<string, MiniEditorEnvPaths> cache = new();
        public string buildDir { get; }
        public string finalManifest { get; }

        public Dictionary<BuildTarget, string> finalBundleDict { get; }

        public readonly string miniId;
        private MiniEditorEnvPaths(string miniId):base(miniId)
        {
            this.miniId = miniId;
            buildDir = $"{MiniBundlesOutput}/{miniId}";
            finalManifest = $"{buildDir}/{miniId}.json";
            finalBundleDict = BuildTargets.ToDictionary(t=>t, GetFinalBundlePath);
        }

        public static MiniEditorEnvPaths Get(string miniId)
        {
            if (!cache.TryGetValue(miniId, out var envPaths))
            {
                envPaths = new MiniEditorEnvPaths(miniId);
                cache[miniId] = envPaths;
                envPaths.RefreshProjectConfig();
            }
            return envPaths;
        }
        
        public string GetFinalBundlePath(BuildTarget buildTarget)
        {
            return $"{buildDir}/{miniId}_{buildTarget}.{BUNDLE_EXT}";
        }

        public string GetPlatformBuildDir(BuildTarget buildTarget)
        {
            return $"{buildDir}/{buildTarget}";
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
                    Debug.LogError($"json parse error in {miniProjectConfig} : {e}");
                    _config = null;
                    return;
                }

                if (!_config.CheckScriptsMatch(luaAssetPaths))
                {
                    _config.scripts = luaAssetPaths;
                    if (Directory.Exists(pathPrefix))
                    {
                        File.WriteAllBytes(miniProjectConfig, _config.ToJson());
                        AssetDatabase.Refresh();
                    }
                }
            }
            else
            {
                _config = new MiniProjectConfig
                {
                    scripts = luaAssetPaths,
                    miniId = "",
                    name = "(???)",
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
            _config.miniId = dbMini.miniId;
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
