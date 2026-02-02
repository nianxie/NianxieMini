using System;
using JetBrains.Annotations;
using Nianxie.Craft;
using UnityEditor;
using UnityEngine;

namespace Nianxie.Riff
{
    public class SpriteMeta
    {
        public IntRectangle rect; 
        public Vector2 pivot; 
        public float pixelsPerUnit;
    }
    public class TextureRegion
    {
        public Vector2Int size => new Vector2Int(rect.width, rect.height);
        public readonly IntRectangle rect;
        public readonly Texture2D texture;
        public TextureRegion(Texture2D texture, IntRectangle rect)
        {
            this.rect = rect;
            this.texture = texture;
        }

        public Sprite CreateSprite(SpriteMeta spriteMeta)
        {
            var unityRect = new Rect(rect.x + spriteMeta.rect.x, rect.y + spriteMeta.rect.y, spriteMeta.rect.width, spriteMeta.rect.height);
            return Sprite.Create(texture, unityRect, Vector2.one*0.5f, spriteMeta.pixelsPerUnit);
        }
    }
}