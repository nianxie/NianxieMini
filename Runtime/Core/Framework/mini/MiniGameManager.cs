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
        public MiniBridge bridge { get; private set; }
        public AbstractCraftEntry craftEntry { get; private set; }

        public async UniTask Init(MiniBridge _bridge)
        {
            Assert.IsNull(bridge, "MiniGame is running");
            bridge = _bridge;
            craftEntry = GetComponent<AbstractCraftEntry>();
            await InitGameModule();
        }

        public async UniTask EntryPlay()
        {
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
    }
}
