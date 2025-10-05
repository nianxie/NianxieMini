using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nianxie.Framework;
using UnityEditor;
using UnityEngine;
using XLua;

namespace Nianxie.Editor
{
    public class CollectScript:AbstractCollectAsset
    {
        private TextAsset textAsset;

        public TextAsset LoadTextAsset()
        {
            if (textAsset == null)
            {
                textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            }
            return textAsset;
        }
        public static SortedDictionary<string, CollectScript> Collect(EnvPaths envPaths)
        {
            var ret = new SortedDictionary<string, CollectScript>();
            var folders = (new [] {envPaths.srcPathPrefix, envPaths.luafabPathPrefix}).Where(a=>Directory.Exists(a)).ToArray();
            if (folders.Length > 0)
            {
                var guids = UnityEditor.AssetDatabase.FindAssets("t:TextAsset", folders);
                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var importer = AssetImporter.GetAtPath(assetPath);
                    if (importer is LuaScriptImporter)
                    {
                        var collect = new CollectScript
                        {
                            path = assetPath,
                            guid = guid,
                        };
                        ret[assetPath] = collect;
                    }
                }
            }
            return ret;
        }
    }
}