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
        
        public MiniPlayArgs playArgs { get; protected set; }
        public override async UniTask PlayMain(MiniPlayArgs args)
        {
            Assert.IsNotNull(bridge, "MiniGame is not PreInit");
            playArgs = args;
            LuafabLoading miniCraftLoading = null;
            if (bridge.miniConfig.craftable)
            {
                miniCraftLoading = assetModule.AttachLuafabLoading(bridge.envPaths.miniCraftLuafabPath, false);
                await miniCraftLoading.WaitTask;
            }

            //craftEdit.PlayMain(this, miniCraftLoading);
            await playFn();
        }
        public override async UniTask EditMain(MiniEditArgs args)
        {
            Assert.IsNotNull(bridge, "MiniGame is not PreInit");
            var miniCraftLoading = assetModule.AttachLuafabLoading(bridge.envPaths.miniCraftLuafabPath, false);
            await miniCraftLoading.WaitTask;
            craftEdit.EditMain(args, miniCraftLoading);
        }

        public override void PlayEnding()
        {
            playArgs.PlayEnding((MiniGameManager)gameManager);
        }
    }
}