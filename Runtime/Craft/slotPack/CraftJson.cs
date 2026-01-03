using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Nianxie.Riff;
using Nianxie.Utils;
using UnityEngine;

namespace Nianxie.Craft
{
    public class CraftJson:ArchiveJson
    {
        public class SpriteInfo
        {
            public IntRectangle rect;
            public Vector2Int pivot;
            public float pixelsPerUnit;
        }

        public SlotBehavJson root;
        public Vector2Int atlasSize;
        public SpriteInfo[] spriteList;
        
        #region // static items

        private static JsonCodec<CraftJson, AbstractSlotJson> jsonCodec;
        static CraftJson()
        {
            // 使用反射获取AbstractCraftJson同命名空间、同程序集的派生类
            jsonCodec = new();
        }

        public LargeBytes ToLargeBytes()
        {
            return LargeBytes.FromUtf8String(jsonCodec.Serialize(this));
        }
        
        public static CraftJson FromLargeBytes(LargeBytes jsonBytes)
        {
            return jsonCodec.Deserialize(jsonBytes.ToUtf8String());
        }
        #endregion
    }

}
