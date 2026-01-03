using Cysharp.Threading.Tasks;
using Nianxie.Craft;
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
        private bool stopped = false;

        private MiniBridge bridge;
        public MiniPlayArgs playArgs { get; private set; }
        public AbstractEntryModule entry { get; private set; }

        public async UniTask PreInit(MiniBridge _bridge)
        {
            Assert.IsNull(bridge, "MiniGame is running");
            bridge = _bridge;
            GetComponent<AssetModule>().PreInit(_bridge);
            entry = GetComponent<AbstractEntryModule>();
            entry.PreInit(_bridge, EntryPlay);
            await InitGameModule();
        }

        private async UniTask EntryPlay()
        {
            await PrepareContextAndRoot();
            rootLuafabLoading.Fork(transform);
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
