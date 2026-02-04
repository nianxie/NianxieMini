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

    public class TextureUsage: AbstractUsage
    {
        public readonly TextureRegion texRegion;
        private readonly HashSet<AssignedSprite> assignedSpriteSet = new();

        public TextureUsage(UsageSourceInfo sourceInfo, TextureRegion texRegion)
        {
            this.sourceInfo = sourceInfo;
            this.texRegion = texRegion;
        }

        public AssignedSprite UseAndCreateSprite(SpriteMeta spriteMeta)
        {
            sourceInfo.usagePool.ResetPackPrepared();
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
            sourceInfo.usagePool.ResetPackPrepared();
            if (assignedSpriteSet.Contains(assignedSprite))
            {
                assignedSpriteSet.Remove(assignedSprite);
                UnityEngine.Object.Destroy(assignedSprite.sprite);
            }

            if (assignedSpriteSet.Count == 0 && sourceInfo is UploadSourceInfo uploadUsageKind)
            {
                uploadUsageKind.ReleaseSource(texRegion.texture);
                sourceInfo = ReleasedSourceInfo.Instance;
            }
        }
        public override void Clear()
        {
            foreach (var assignedSprite in assignedSpriteSet)
            {
                UnityEngine.Object.Destroy(assignedSprite.sprite);
            } 
            assignedSpriteSet.Clear();
        }
    }
}