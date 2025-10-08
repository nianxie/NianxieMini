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
        public MiniBridge bridge { get; private set; }
        public SlotBehaviour craftSlot { get; private set; }
        public MiniArgs args { get; private set; }

        public async UniTask PreInit(MiniBridge _bridge)
        {
            Assert.IsNull(bridge, "MiniGame is running");
            bridge = _bridge;
            GetComponent<AssetModule>().PreInit(bridge);
            await InitGameModule();
        }

        [BlackList]
        public async UniTask PlayMain(MiniArgs _args)
        {
            Assert.IsNotNull(bridge, "MiniGame is not PreInit");
            args = _args;
            if (args.craft)
            {
                craftSlot = await CreateCraftSlot();
            }
            await PrepareContextAndRoot();
            rootLuafabLoading.Fork(transform);
        }

        [BlackList]
        public async UniTask EditMain()
        {
            Assert.IsNotNull(bridge, "MiniGame is not PreInit");
            await GetComponent<EditCraftModule>().Main(this);
        }
        
        [HintReturn("Fn():Ret(Future(Nil))")]
        public lua_CSFunction FuturePlayMain => bridge.shellEnv.AsyncAction<MiniArgs>(this, PlayMain);
        [HintReturn("Fn():Ret(Future(Nil))")]
        public lua_CSFunction FutureEditMain => bridge.shellEnv.AsyncAction(this, EditMain);


        private async UniTask<SlotBehaviour> CreateCraftSlot()
        {
            var craftLuafabLoading = assetModule.AttachLuafabLoading(bridge.envPaths.miniCraftLuafabPath, false);
            await craftLuafabLoading.WaitTask;
            var slotRoot = (SlotBehaviour)craftLuafabLoading.RawFork(transform);
            foreach (var childRenderer in slotRoot.gameObject.GetComponentsInChildren<Renderer>())
            {
                childRenderer.enabled = false;
            }
            foreach (var childCollider2D in slotRoot.gameObject.GetComponentsInChildren<Collider2D>())
            {
                childCollider2D.enabled = false;
            }
            foreach (var childCollider in slotRoot.gameObject.GetComponentsInChildren<Collider>())
            {
                childCollider.enabled = false;
            }

            var craftJson = args.craftJson;
            var altasTex = args.atlasTex;
            if (craftJson != null)
            {
                var unpackContext = new CraftUnpackContext(craftJson, altasTex);
                unpackContext.UnpackRoot(slotRoot);
            }
            return slotRoot;
        }

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
                    reflectEnv.Dispose();
                }
            }).Forget();
        }

        public void OnDestroy()
        {
            Stop();
        }
    }
}
