using Cysharp.Threading.Tasks;
using Nianxie.Craft;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;

namespace Nianxie.Framework
{
    public class MiniGameManager : AbstractGameManager
    {
        public MiniBridge bridge { get; private set; }
        public SlotBehaviour craftSlot { get; private set; }

        public MiniBridgeSession bridgeSession { get; private set; }

        private async UniTask PreInit(MiniBridge _bridge, MiniBridgeSession _session)
        {
            Assert.IsNull(bridge, "MiniGame is running");
            bridge = _bridge;
            bridgeSession = _session;
            GetComponent<AssetModule>().PreInit(bridge);
            await InitGameModule();
        }

        public async UniTask PlayGameMain(MiniBridge _bridge, MiniBridgeSession _session)
        {
            await PreInit(_bridge, _session);
            await PrepareContextAndRoot();
            rootLuafabLoading.Fork(transform);
        }
        
        public async UniTask PlayCraftMain(MiniBridge _bridge, MiniBridgeSession _session, CraftJson craftJson, Texture2D altasTex)
        {
            await PreInit(_bridge, _session);
            var craftLuafabLoading = assetModule.AttachLuafabLoading(bridge.GetEnvPaths().miniCraftLuafabPath, false);
            await craftLuafabLoading.WaitTask;
            craftSlot = (SlotBehaviour)craftLuafabLoading.RawFork(transform);
            foreach (var childRenderer in craftSlot.gameObject.GetComponentsInChildren<Renderer>())
            {
                childRenderer.enabled = false;
            }
            foreach (var childCollider2D in craftSlot.gameObject.GetComponentsInChildren<Collider2D>())
            {
                childCollider2D.enabled = false;
            }
            foreach (var childCollider in craftSlot.gameObject.GetComponentsInChildren<Collider>())
            {
                childCollider.enabled = false;
            }

            if (craftJson != null)
            {
                var unpackContext = new CraftUnpackContext(craftJson, altasTex);
                unpackContext.UnpackRoot(craftSlot);
            }
            await PrepareContextAndRoot();
            rootLuafabLoading.Fork(transform);
        }

        public async UniTask EditCraftMain(MiniBridge _bridge, MiniBridgeSession _session)
        {
            await PreInit(_bridge, _session);
            await GetComponent<EditCraftModule>().Main(this);
        }

        protected override RuntimeReflectEnv CreateReflectEnv()
        {
            return RuntimeReflectEnv.Create(this, bridge.GetEnvPaths(), bridge.GetMiniBoot());
        }

        public override void OnInjectGameHelper(LuaTable script, string key, System.Type helperType)
        {
            // mini game do nothing when module inject
            Debug.LogError("GameHelper inject not supported in mini mode");
        }
    }
}
