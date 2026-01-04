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
    public class CraftJson:CustomJson
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

        private static JsonCodec<CraftJson, AbstractSlotJson> jsonCodec = new();

        public LargeBytes ToLargeBytes()
        {
            return LargeBytes.FromUtf8String(jsonCodec.Serialize(this));
        }
        
        public static CraftJson FromLargeBytes(LargeBytes jsonBytes)
        {
            return jsonCodec.Deserialize(jsonBytes.ToUtf8String());
        }
        #endregion

        public override string Dump()
        {
            return jsonCodec.Serialize(this);
        }
    }

}
