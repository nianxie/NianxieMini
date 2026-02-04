using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using UnityEngine;

namespace Nianxie.Craft
{
    public class PositionJson:AbstractSlotJson<Vector2>
    {
        public float x;
        public float y;
        public override Vector2 Export(AssetUsageCenter usageCenter)
        {
            return new Vector2(x, y);
        }
    }
}
