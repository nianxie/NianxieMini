using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XLua;

namespace Nianxie.Editor
{

    public class CollectNotScript:AbstractCollectAsset
    {
        public bool isExplicit { get; private set; }
        private static CollectNotScript Create(string assetPath, string guid)
        {
            return new CollectNotScript
            {
                path=assetPath,
                guid=guid,
                isExplicit=true,
            };
        }
        private static CollectNotScript Create(string assetPath)
        {
            return new CollectNotScript
            {
                path=assetPath,
                guid=AssetDatabase.AssetPathToGUID(assetPath),
                isExplicit=false,
            };
        }
        public static Dictionary<string, CollectNotScript> Collect(EditorReflectEnv reflectEnv)
        {
            // 1. collect explicit
            Dictionary<string, string> explicitPathToGuid = new();
            var guids = AssetDatabase.FindAssets("t:Prefab", new [] {reflectEnv.envPaths.luafabPathPrefix});
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                explicitPathToGuid[assetPath] = guid;
            }
            foreach (var pathGuid in reflectEnv.CollectReferenceAssetPaths())
            {
                explicitPathToGuid[pathGuid.Key] = pathGuid.Value;
            }

            // 2. collect dependencies
            HashSet<string> implicitPathSet = new();
            foreach (var assetPath in explicitPathToGuid.Keys)
            {
                foreach (var dependPath in AssetDatabase.GetDependencies(assetPath, true))
                {
                    if (!explicitPathToGuid.ContainsKey(dependPath))
                    {
                        implicitPathSet.Add(dependPath);
                    }
                }
            }
            implicitPathSet = implicitPathSet.Where(Filter).ToHashSet();
            
            // 3. build info
            var collectDict = new Dictionary<string, CollectNotScript>();
            foreach (var pathGuid in explicitPathToGuid)
            {
                collectDict[pathGuid.Key] = Create(pathGuid.Key, pathGuid.Value);
            }
            foreach (var assetPath in implicitPathSet)
            {
                if (!collectDict.ContainsKey(assetPath))
                {
                    collectDict[assetPath] = Create(assetPath);
                }
            }
            return collectDict;
        }
        private static readonly HashSet<string> IgnoreFileExtensions = new HashSet<string>() { "", ".so", ".dll", ".cs", ".js", ".boo", ".meta", ".cginc", ".hlsl" };
        private static bool Filter(string assetPath)
        {
            if (!assetPath.StartsWith("Assets/") && !assetPath.StartsWith("Packages/"))
            {
                Debug.LogWarning($"Invalid asset path : {assetPath}");
                return false;
            }

            // 忽略文件夹
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return false;
            }
            
            // 忽略Gizmos
            if (assetPath.Contains("/Gizmos/") || assetPath.Contains("/Editor"))
            {
                return false;
            }
            
            // 忽略Gizmos
            if (assetPath.Contains("/Editor Resources/"))
            {
                Debug.LogWarning($"Invalid asset path : {assetPath} in /Editor Resources/");
                return false;
            }

            return !IgnoreFileExtensions.Contains(Path.GetExtension(assetPath));
        }
    }
}