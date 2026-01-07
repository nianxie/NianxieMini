using Cysharp.Threading.Tasks;
using Nianxie.Framework;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;

namespace Nianxie.Craft
{
    [BlackList]
    public class DefaultCraftEntry:AbstractCraftEntry
    {
        [SerializeField]
        private AssetModule assetModule;
        
        [SerializeField]
        private CraftEdit craftEdit;

        private MiniGameManager miniGameManager => (MiniGameManager) gameManager;
        public MiniEditArgs editArgs { get; private set; }
        public SlotBehaviour rootSlot { get; private set; }
        public override LuaTable PlayBuild()
        {
            craftEdit.gameObject.SetActive(false);
            var riffPackage = miniGameManager.bridge.riffPackage;
            if (riffPackage == null)
            {
                return null;
            }
            else
            {
                Debug.LogError("TODO build craft table");
                return null;
            }
        }
        public async UniTask<CraftEdit> EditMain(MiniEditArgs args)
        {
            editArgs = args;
            // 1. Instantiate MiniCraft as rootSlot
            var miniCraftLoading = assetModule.AttachLuafabLoading(miniGameManager.bridge.envPaths.miniCraftLuafabPath, false);
            await miniCraftLoading.WaitTask;
            var behav = miniCraftLoading.RawFork(craftEdit.editArea.transform);
            if (behav is SlotBehaviour slotBehav)
            {
                rootSlot = slotBehav;
                rootSlot.RootInit(craftEdit);
            }
            else
            {
                throw new System.Exception("BehavSlot expected in root of MiniCraft");
            }
            // 2. unpack from root slot
            var riffPackage = miniGameManager.bridge.riffPackage;
            if (riffPackage != null)
            {
                var unpackContext = new UnpackContext(riffPackage);
                unpackContext.UnpackRoot(rootSlot);
            }
            return craftEdit;
        }
    }
}