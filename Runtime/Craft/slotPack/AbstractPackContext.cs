using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Nianxie.Craft
{
    public abstract class AbstractPackContext : IPutAsset
    {
        protected List<TypedBinary> binaryList = new();

        protected class TypedBinary
        {
            public string ext;
            public byte[] data;
        }

        protected List<AtlasSprite> atlasSpriteList = new();
        protected class AtlasSprite
        {
            public Sprite sprite;
            public int resolution;
            public Vector2Int pivot
            {
                
                get
                {
                    var p = sprite.pivot;
                    return new Vector2Int((int)p.x, (int)p.y);
                }
            }
            public Vector2Int size {
                get
                {
                    var s = sprite.rect.size;
                    return new Vector2Int((int)s.x, (int)s.y);
                }
            }
        }
        int IPutAsset.PutSprite(Sprite sprite)
        {
            var index = atlasSpriteList.Count;
            //var rect = sprite.rect;
            atlasSpriteList.Add(new AtlasSprite
            {
                sprite = sprite,
            });
            return index;
        }

        int IPutAsset.PutBinary(string ext, byte[] binary)
        {
            var index = binaryList.Count;
            binaryList.Add(new TypedBinary
            {
                ext=ext,
                data=binary
            });
            return index;
        }
    }

}
