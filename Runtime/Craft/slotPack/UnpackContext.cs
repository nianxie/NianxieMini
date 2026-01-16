using Nianxie.Riff;
using XLua;

namespace Nianxie.Craft
{

    public class UnpackContext:IGetAsset
    {
        private RuntimeReflectEnv env;
        private RiffPackage package;
        private CraftRiffJson craftRiffJson;
        public UnpackContext(RiffPackage riffPackage, RuntimeReflectEnv reflectEnv)
        {
            package = riffPackage;
            env = reflectEnv;
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

        LuaTable IGetAsset.NewTable()
        {
            return env.NewTable();
        }

    }
}
