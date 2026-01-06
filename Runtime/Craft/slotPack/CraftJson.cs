using System;
using System.Linq;
using Nianxie.Riff;
using UnityEngine;

namespace Nianxie.Craft
{
    public class CraftJson:CustomJson
    {
        public override string kind => nameof(CraftJson);
        public override string version => "0.0.1";

        static CraftJson()
        {
            var slotJsonType = typeof(AbstractSlotJson);
            // 使用反射获取contentType同命名空间、同程序集的派生类
            var asm = AppDomain.CurrentDomain.GetAssemblies().First(asm => asm.GetType(slotJsonType.FullName) != null);
            var jsonTypes = asm.GetTypes().Where(type => type.Namespace == slotJsonType.Namespace && type.IsSubclassOf(slotJsonType)).ToArray();
            var typeMap = jsonTypes.ToDictionary(type => type.Name);
            JsonCodec.RegisterFactory<CraftJson>(typeMap);
        }

        public class SpriteInfo
        {
            public IntRectangle rect;
            public Vector2Int pivot;
            public float pixelsPerUnit;
        }

        public SlotBehavJson root;
        public Vector2Int atlasSize;
        public SpriteInfo[] spriteList;
    }

}
