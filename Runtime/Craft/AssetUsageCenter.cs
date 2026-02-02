using System;
using System.Collections.Generic;
using System.Linq;
using Nianxie.Framework;
using Nianxie.Riff;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    public class AssetUsageCenter:MonoBehaviour
    {
        // texture usage indices
        private HashSet<TextureUsage> textureUsageSet = new();
        private Dictionary<int, TextureUsage> riffTextureUsageDict = new();
        private Dictionary<string, TextureUsage> builtinTextureUsageDict = new();
        
        // builtin items
        private Dictionary<int, string> builtinIdToPath = new();

        public IReadOnlyCollection<TextureUsage> textureUsageCollection => textureUsageSet;

        [SerializeField] 
        private CraftManager craftManager;
        [SerializeField] 
        private MiniGameManager gameManager;

        private bool mainCalled = false;
        
        [BlackList]
        public void Main()
        {
            UnityEngine.Assertions.Assert.IsFalse(mainCalled, "asset usage center'Main is called");
            mainCalled = true;
            var riffPackage = gameManager.bridge.riffPackage;
            if (riffPackage != null)
            {
                for(int i=0;i<riffPackage.texRegions.Length;i++)
                {
                    var usage = TextureUsage.CreateByRiff(riffPackage.texRegions[i], i);
                    textureUsageSet.Add(usage);
                    riffTextureUsageDict[i] = usage;
                }
            }
        }

        public TextureUsage UploadTexture(Texture2D tex)
        {
            var texUsage = TextureUsage.CreateByUpload(tex, craftManager.editArgs.shellRefresh.Action);
            textureUsageSet.Add(texUsage);
            return texUsage;
        }
        
        [BlackList]
        public void RegisterBuiltinObject(string builtinPath, UnityEngine.Object builtinObject)
        {
            if (builtinObject is Sprite sprite)
            {
                var texUsage = TextureUsage.CreateByBuiltin(sprite, builtinPath);
                textureUsageSet.Add(texUsage);
                builtinTextureUsageDict[builtinPath] = texUsage;
                builtinIdToPath[builtinObject.GetInstanceID()] = builtinPath;
            }
            else
            {
                Debug.LogError($"register default object with type={builtinObject.GetType()} TODO");
            }
        }
        
        [BlackList]
        public bool IsBuiltinObject(UnityEngine.Object builtinObject, out string builtinPath)
        {
            return builtinIdToPath.TryGetValue(builtinObject.GetInstanceID(), out builtinPath);
        }
        
        [BlackList]
        public TextureUsage GetBuiltinTextureUsage(string builtinPath)
        {
            return builtinTextureUsageDict[builtinPath];
        }
        
        [BlackList]
        public TextureUsage GetRiffTextureUsage(int riffIndex)
        {
            return riffTextureUsageDict[riffIndex];
        }
        
        private void OnDestroy()
        {
            foreach (var usage in textureUsageSet)
            {
                usage.Clear();
            }
            textureUsageSet.Clear();
        }
    }
}