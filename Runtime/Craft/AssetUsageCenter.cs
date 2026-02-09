using System.Collections.Generic;
using Nianxie.Framework;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{

    public class AssetUsageCenter:MonoBehaviour
    {
        // builtin items
        private Dictionary<int, string> builtinIdToPath = new();

        public TextureUsagePool textureUsagePool { get; private set; }
        public BinaryUsagePool binaryUsagePool { get; private set; }

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
            textureUsagePool = new TextureUsagePool(craftManager);
            binaryUsagePool = new BinaryUsagePool(craftManager);
            var riffPackage = gameManager.bridge.riffPackage;
            if (riffPackage != null)
            {
                for(int i=0;i<riffPackage.texRegions.Length;i++)
                {
                    textureUsagePool.AddByRiff(riffPackage.texRegions[i], i);
                }
            }
        }
        
        [BlackList]
        public void RegisterBuiltinObject(string builtinPath, UnityEngine.Object builtinObject)
        {
            if (builtinObject is Sprite sprite)
            {
                textureUsagePool.AddByBuiltin(sprite, builtinPath);
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
        
        private void OnDestroy()
        {
            textureUsagePool.Clear();
            binaryUsagePool.Clear();
        }

        public LuaTable NewTable()
        {
            return gameManager.reflectEnv.NewTable();
        }
    }
}