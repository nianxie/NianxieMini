using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Nianxie.Components;
using Nianxie.Utils;
using XLua;

namespace Nianxie.Editor
{
    [InitializeOnLoad]
    public class NianxieAssetProcessors : AssetPostprocessor
    {
        private static bool TrySilentMap(string assetPath, out EditorEnvPaths envPaths)
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
                    var folder = splitArr[2];
                    envPaths = MiniEditorEnvPaths.SilentGet(folder);
                    return envPaths != null;
                }
            }
            envPaths = null;
            return false;
        }

        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            Dictionary<string, MiniEditorEnvPaths> miniRefreshDict = new();
            foreach (string path in importedAssets.Concat(deletedAssets).Concat(movedAssets).Concat(movedFromAssetPaths))
            {
                if (TrySilentMap(path, out var envPaths))
                {
                    if (AssetImporter.GetAtPath(path) is LuaScriptImporter)
                    {
                        envPaths.SetObsolete();
                        if(envPaths is MiniEditorEnvPaths miniEditorEnvPaths)
                        {
                            miniRefreshDict[miniEditorEnvPaths.folder] = miniEditorEnvPaths;
                        }
                    }
                    else if(envPaths is MiniEditorEnvPaths miniEditorEnvPaths)
                    {
                        // 如果是新建文件夹，或者是path，则也需要更新envPaths中的config
                        if (envPaths.miniProjectConfig == path || envPaths.pathPrefix == path)
                        {
                            miniRefreshDict[miniEditorEnvPaths.folder] = miniEditorEnvPaths;
                        }
                    }
                }
            }

            foreach (var envPaths in miniRefreshDict.Values)
            {
                envPaths.RefreshProjectConfig();
            }
            
            foreach (string assetPath in importedAssets.Concat(movedAssets))
            {
                if (assetPath.EndsWith(".prefab") && TrySilentMap(assetPath, out var envPaths))
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    var rootBehav = prefab.GetComponent<LuaBehaviour>();
                    if (rootBehav != null)
                    {
                        var newScriptPath = envPaths.assetPath2classPath(assetPath);
                        var oldScriptPath = rootBehav.classPath;
                        if (oldScriptPath != newScriptPath)
                        {
                            foreach (var childBehav in prefab.GetComponentsInChildren<LuaBehaviour>(true))
                            {
                                if (childBehav.classPath == oldScriptPath)
                                {
                                    childBehav.classPath = newScriptPath;
                                }
                            }
                            PrefabUtility.SavePrefabAsset(prefab);
                        }
                    }
                }
            }
        }
    }
}
