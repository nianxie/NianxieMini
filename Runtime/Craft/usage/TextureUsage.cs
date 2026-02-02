using System;
using System.Collections.Generic;
using Nianxie.Riff;
using UnityEngine;

namespace Nianxie.Craft
{
    public class AssignedSprite
    {
        public Sprite sprite;
        public TextureUsage usage;
        public SpriteMeta meta;
    }

    public class TextureUsage
    {
        public SourceKind sourceKind { get; private set; }
        public TextureRegion texRegion { get; private set; }
        
        private readonly HashSet<AssignedSprite> assignedSpriteSet = new();

        private TextureUsage()
        {
        }

        public static TextureUsage CreateByUpload(Texture2D uploadTex, Action<Texture2D> release)
        {
            var usage = new TextureUsage()
            {
                sourceKind = new UploadSourceKind(release),
                texRegion = new TextureRegion(uploadTex, new IntRectangle(0, 0, uploadTex.width, uploadTex.height)),
            };
            return usage;
        }
        public static TextureUsage CreateByBuiltin(Sprite sprite, string builtinPath)
        {
            var rect = new IntRectangle(Mathf.RoundToInt(sprite.textureRect.x), Mathf.RoundToInt(sprite.textureRect.y), Mathf.RoundToInt(sprite.textureRect.width), Mathf.RoundToInt(sprite.textureRect.height));
            var texture = sprite.texture;
            var usage = new TextureUsage()
            {
                sourceKind = new BuiltinSourceKind(builtinPath),
                texRegion = new TextureRegion(texture, rect),
            };
            return usage;
        }
        public static TextureUsage CreateByRiff(TextureRegion texRegion, int riffIndex)
        {
            var usage = new TextureUsage()
            {
                sourceKind = new RiffSourceKind(riffIndex),
                texRegion = new TextureRegion(texRegion.texture, texRegion.rect),
            };
            return usage;
        }

        public AssignedSprite UseAndCreateSprite(SpriteMeta spriteMeta)
        {
            var sprite = texRegion.CreateSprite(spriteMeta);
            var assignedSprite = new AssignedSprite
            {
                sprite=sprite,
                usage=this,
                meta=spriteMeta,
            };
            assignedSpriteSet.Add(assignedSprite);
            return assignedSprite;
        }
        public void DelUsage(AssignedSprite assignedSprite) 
        {
            if (assignedSpriteSet.Contains(assignedSprite))
            {
                assignedSpriteSet.Remove(assignedSprite);
                UnityEngine.Object.Destroy(assignedSprite.sprite);
            }

            if (assignedSpriteSet.Count == 0 && sourceKind is UploadSourceKind uploadUsageKind)
            {
                uploadUsageKind.releaseUpload(texRegion.texture);
                sourceKind = ReleasedSourceKind.Instance;
            }
        }
        public void Clear()
        {
            foreach (var assignedSprite in assignedSpriteSet)
            {
                UnityEngine.Object.Destroy(assignedSprite.sprite);
            } 
            assignedSpriteSet.Clear();
        }
    }
}