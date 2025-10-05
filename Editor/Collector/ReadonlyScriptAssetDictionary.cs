using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nianxie.Framework;
using UnityEditor;
using UnityEngine;
using XLua;
namespace Nianxie.Editor
{
    public class ReadonlyScriptAssetDictionary : IReadOnlyDictionary<string, TextAsset>
    {
        private readonly SortedDictionary<string, CollectScript> collectScriptDict;
        public ReadonlyScriptAssetDictionary(SortedDictionary<string, CollectScript> dict)
        {
            collectScriptDict = dict;
        }

        public IEnumerator<KeyValuePair<string, TextAsset>> GetEnumerator()
        {
            foreach (var (assetPath, collect) in collectScriptDict)
            {
                yield return new KeyValuePair<string, TextAsset>(assetPath, collect.LoadTextAsset());
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => collectScriptDict.Count;
        public bool ContainsKey(string key)
        {
            return collectScriptDict.ContainsKey(key);
        }

        public bool TryGetValue(string key, out TextAsset value)
        {
            if(collectScriptDict.TryGetValue(key, out var collect))
            {
                value = collect.LoadTextAsset();
                return true;
            } 
            value = null;
            return false;
        }

        public TextAsset this[string key] => collectScriptDict[key].LoadTextAsset();

        public IEnumerable<string> Keys => collectScriptDict.Keys;
        public IEnumerable<TextAsset> Values => collectScriptDict.Values.Select(v => v.LoadTextAsset());
    }
}