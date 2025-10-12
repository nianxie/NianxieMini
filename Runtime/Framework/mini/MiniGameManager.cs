using Cysharp.Threading.Tasks;
using Nianxie.Craft;
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
        public async UniTask EditMain(MiniEditArgs args)
        {
            Assert.IsNotNull(bridge, "MiniGame is not PreInit");
            await craftModule.EditMain(args);
        }
        
        [HintReturn(new []{typeof(MiniPlayArgs)})]
        public lua_CSFunction FuturePlayMain => bridge.shellEnv.AsyncAction<MiniPlayArgs>(this, PlayMain);
        [HintReturn(new System.Type[]{}, typeof(CraftModule))]
        public lua_CSFunction FutureEditMain => bridge.shellEnv.AsyncAction<MiniEditArgs>(this, EditMain);


        protected override RuntimeReflectEnv CreateReflectEnv()
        {
            return RuntimeReflectEnv.Create(this, bridge.envPaths, bridge.GetMiniBoot());
        }

        public void Stop()
        {
            if (stopped) return;
            stopped = true;
            UniTask.Create(async () =>
            {
                try
                {
                    await bridge.UnloadMini(this);
                }
                finally
                {
                    // TODO how to dispose luaEnv properly??
                    //reflectEnv.Dispose();
                }
            }).Forget();
        }

        void OnDestroy()
        {
            Stop();
        }
    }
}
