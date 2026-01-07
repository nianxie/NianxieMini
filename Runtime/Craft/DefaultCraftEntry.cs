using Cysharp.Threading.Tasks;
using Nianxie.Framework;
using UnityEngine;
using UnityEngine.Assertions;

namespace Nianxie.Craft
{
    public class DefaultCraftEntry:AbstractCraftEntry
    {
        [SerializeField]
        private AssetModule assetModule;
        public CraftEdit craftEdit;

        private MiniGameManager miniGameManager => (MiniGameManager) gameManager;
        public MiniPlayArgs playArgs { get; private set; }
        public MiniEditArgs editArgs { get; private set; }
        public SlotBehaviour rootSlot { get; private set; }
        public override async UniTask PlayMain(MiniPlayArgs args)
        {
            playArgs = args;
            craftEdit.gameObject.SetActive(false);
            if (miniGameManager.bridge.miniConfig.craftable)
            {
            }
            await miniGameManager.EntryPlay();
        }
        public override async UniTask EditMain(MiniEditArgs args)
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
        }

        public override void PlayEnding()
        {
            playArgs.PlayEnding(miniGameManager);
        }
    }
}