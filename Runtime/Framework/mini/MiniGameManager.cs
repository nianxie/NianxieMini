using Cysharp.Threading.Tasks;
using Nianxie.Craft;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;
using XLua.LuaDLL;

namespace Nianxie.Framework
{
    public class MiniGameManager : AbstractGameManager
    {
        private bool stopped = false;
        [SerializeField]
        private CraftEdit craftEdit;

        private MiniBridge bridge;
        public MiniPlayArgs playArgs { get; private set; }

        public async UniTask PreInit(MiniBridge _bridge)
        {
            Assert.IsNull(bridge, "MiniGame is running");
            bridge = _bridge;
            GetComponent<AssetModule>().PreInit(bridge);
            await InitGameModule();
        }

        public async UniTask PlayMain(MiniPlayArgs args)
        {
            Assert.IsNotNull(bridge, "MiniGame is not PreInit");
            playArgs = args;
            LuafabLoading miniCraftLoading = null;
            if (bridge.miniConfig.craftable)
            {
                miniCraftLoading = assetModule.AttachLuafabLoading(bridge.envPaths.miniCraftLuafabPath, false);
                await miniCraftLoading.WaitTask;
            }
            craftEdit.PlayMain(args, miniCraftLoading);
            await PrepareContextAndRoot();
            rootLuafabLoading.Fork(transform);
        }

        public async UniTask<CraftEdit> EditMain(MiniEditArgs args)
        {
            Assert.IsNotNull(bridge, "MiniGame is not PreInit");
            var miniCraftLoading = assetModule.AttachLuafabLoading(bridge.envPaths.miniCraftLuafabPath, false);
            await miniCraftLoading.WaitTask;
            craftEdit.EditMain(args, miniCraftLoading);
            return craftEdit;
        }

        protected override RuntimeReflectEnv CreateReflectEnv()
        {
            return RuntimeReflectEnv.Create(this, bridge.envPaths, bridge.miniBoot);
        }

        void OnDestroy()
        {
            if (stopped) return;
            stopped = true;
            UniTask.Create(async () =>
            {
                try
                {
                    await SceneAsyncUtility.UnloadSceneAsync(gameObject.scene);
                }
                finally
                {
                    // TODO how to dispose luaEnv properly??
                    //reflectEnv.Dispose();
                }
            }).Forget();
        }

        public LuaTable craftTable
        {
            get
            {
                var craftSlot = craftEdit.rootSlot;
                if (craftSlot != null)
                {
                    return craftSlot.behav.luaTable;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
