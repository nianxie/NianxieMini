using Nianxie.Riff;

namespace Nianxie.Craft
{

    public class UnpackContext:IGetAsset
    {
        private RiffPackage package;
        private CraftRiffJson craftRiffJson;
        public UnpackContext(RiffPackage riffPackage)
        {
            package = riffPackage;
            craftRiffJson = (riffPackage.custom as CraftRiffJson)!;
        }
        
        public void UnpackRoot(SlotBehaviour rootBehav)
        {
            rootBehav.UnpackFromJson(this, craftRiffJson.root);
        }


        UnityEngine.Sprite IGetAsset.GetSprite(int spriteIndex)
        {
            return package.sprites[spriteIndex];
        }

    }
}
