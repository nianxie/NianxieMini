using Nianxie.Riff;

namespace Nianxie.Craft
{

    public class UnpackContext:IGetAsset
    {
        private RiffPackage package;
        private CraftJson craftJson;
        public UnpackContext(RiffPackage riffPackage)
        {
            package = riffPackage;
            craftJson = (riffPackage.customJson as CraftJson)!;
        }
        
        public void UnpackRoot(SlotBehaviour rootBehav)
        {
            rootBehav.UnpackFromJson(this, craftJson.root);
        }


        UnityEngine.Sprite IGetAsset.GetSprite(int spriteIndex)
        {
            return package.sprites[spriteIndex];
        }

    }
}
