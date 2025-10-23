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
        public CraftModule craftModule { get; private set; }
        public MiniBridge bridge { get; private set; }
        public MiniPlayArgs playArgs { get; private set; }

        public async UniTask PreInit(MiniBridge _bridge)
        {
            Assert.IsNull(bridge, "MiniGame is running");
            bridge = _bridge;
            craftModule = GetComponent<CraftModule>();
            GetComponent<AssetModule>().PreInit(bridge);
            await InitGameModule();
        }

        [BlackList]
        public async UniTask PlayMain(MiniPlayArgs args)
        {
            Assert.IsNotNull(bridge, "MiniGame is not PreInit");
            playArgs = args;
            await craftModule.PlayMain(playArgs);
            await PrepareContextAndRoot();
            rootLuafabLoading.Fork(transform);
        }

        [BlackList]
        public async UniTask<CraftModule> EditMain(MiniEditArgs args)
        {
            Assert.IsNotNull(bridge, "MiniGame is not PreInit");
            await craftModule.EditMain(args);
            return craftModule;
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
    }
}
