using System;
using Nianxie.Utils;
using UnityEngine;
using WebP;
using Nianxie.Craft;

namespace Nianxie.Preview
{
    public class SimpleUnpackContext:AbstractUnpackContext
    {
        public SimpleUnpackContext()
        {
        }
        
        public void UnpackRoot(CraftJson craftJson, Texture2D atlasTex, SlotBehaviour rootBehav)
        {
            spriteList = new Sprite[craftJson.spriteList.Length];
            for (int i = 0; i < craftJson.spriteList.Length; i++)
            {
                var info = craftJson.spriteList[i];
                spriteList[i] = Sprite.Create(atlasTex, info.rect.ToUnityRect(), new Vector2(1.0f*info.pivot.x/info.rect.width, 1.0f*info.pivot.y/info.rect.height), info.pixelsPerUnit);
            }
            rootBehav.UnpackFromJson(this, craftJson.root);
        }
    }
}