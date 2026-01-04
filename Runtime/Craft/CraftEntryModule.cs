using Cysharp.Threading.Tasks;
using Nianxie.Framework;
using UnityEngine;
using UnityEngine.Assertions;

namespace Nianxie.Craft
{
    public class CraftEntryModule:AbstractEntryModule
    {
        [SerializeField]
        private AssetModule assetModule;
        public CraftEdit craftEdit;

        private MiniGameManager miniGameManager => (MiniGameManager) gameManager;
        public MiniPlayArgs playArgs { get; protected set; }
        public override async UniTask PlayMain(MiniPlayArgs args)
        {
            playArgs = args;
            LuafabLoading miniCraftLoading = null;
            if (miniGameManager.bridge.miniConfig.craftable)
            {
                miniCraftLoading = assetModule.AttachLuafabLoading(miniGameManager.bridge.envPaths.miniCraftLuafabPath, false);
                await miniCraftLoading.WaitTask;
            }

            //craftEdit.PlayMain(this, miniCraftLoading);
            await miniGameManager.EntryPlay();
        }
        public override async UniTask EditMain(MiniEditArgs args)
        {
            var miniCraftLoading = assetModule.AttachLuafabLoading(miniGameManager.bridge.envPaths.miniCraftLuafabPath, false);
            await miniCraftLoading.WaitTask;
            craftEdit.EditMain(args, miniCraftLoading);
        }

        public override void PlayEnding()
        {
            playArgs.PlayEnding(miniGameManager);
        }
    }
}