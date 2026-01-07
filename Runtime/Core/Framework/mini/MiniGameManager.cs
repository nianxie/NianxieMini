using Cysharp.Threading.Tasks;
using Nianxie.Craft;
using Nianxie.Riff;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using XLua;
using XLua.LuaDLL;

namespace Nianxie.Framework
{
    public class MiniGameManager : AbstractGameManager
    {
        private bool unloaded = false;
        private MiniPlayArgs playArgs;
        public MiniBridge bridge { get; private set; }
        public LuaTable craftTable { get; private set; }
        public async UniTask Init(MiniBridge _bridge)
        {
            Assert.IsNull(bridge, "MiniGame is running");
            bridge = _bridge;
            await InitGameModule();
        }

        public async UniTask PlayMain(MiniPlayArgs playArgs)
        {
            craftTable = GetComponent<AbstractCraftEntry>().PlayBuild();
            await PrepareContextAndRoot();
            rootLuafabLoading.Fork(transform);
        }
        
        protected override RuntimeReflectEnv CreateReflectEnv()
        {
            return RuntimeReflectEnv.Create(this, bridge.envPaths, bridge.miniBoot);
        }
        public override IAssetLoader GetAssetLoader()
        {
            return bridge;
        }
        
        public async UniTask UnloadAsync()
        {
            if (unloaded) return;
            unloaded = true;
            try
            {
                await SceneAsyncUtility.UnloadSceneAsync(gameObject.scene);
            }
            finally
            {
                reflectEnv.Dispose();
            }
        }

        void OnDestroy()
        {
            if (!unloaded)
            {
                Debug.LogWarning($"use {nameof(UnloadAsync)} to destroy GameManager");
            }
        }

        public void PlayEnding()
        {
            playArgs.PlayEnding(this);
        }
    }
}
