using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nianxie.Components;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

namespace Nianxie.Craft
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteSlot : AbstractSlotCom<SpriteJson, Texture2D, Sprite>
    {
        [SerializeField]
        private Vector2 m_Pivot;
        [SerializeField]
        private Vector2Int m_Size = new Vector2Int(512, 768);
        [SerializeField]
        private FitViewAxis m_FitViewAxis;
        [SerializeField]
        private int m_Resolution = 512;

        [NonSerialized] SpriteRenderer m_Renderer;
        public SpriteRenderer drawRenderer
        {
            get
            {
                if (!m_Renderer)
                {
                    gameObject.TryGetComponent(out m_Renderer);
                }
                return m_Renderer;
            }
        }

        // 返回crop矩形
        private IntRectangle CalcPackAndCrop(Texture2D tex, out Vector2Int packSize)
        {
            if (m_FitViewAxis == FitViewAxis.Horizontal)
            {
                // 对于Horizontal的情况，确保缩放时，高度取整时总是小于等于对应宽高比的高度
                var maxPackSize = m_Size.x >= m_Size.y
                    ? new Vector2Int(m_Resolution, Mathf.FloorToInt(1.0f * m_Resolution * m_Size.y / m_Size.x))
                    : new Vector2Int(Mathf.CeilToInt(1.0f * m_Resolution * m_Size.x / m_Size.y), m_Resolution);

                int croppedHeight = tex.width * maxPackSize.y / maxPackSize.x;
                if (croppedHeight >= tex.height) // 如果maxPackSize.y / maxPackSize.x > texture2D.height / texture2D.width，则不需要裁切高度
                {
                    // 如果原图尺寸比最大打包尺寸要小，则直接使用原图尺寸打包
                    if (tex.width < maxPackSize.x)
                    {
                        packSize = new Vector2Int(tex.width, tex.height);
                    }
                    else
                    {
                        packSize = new Vector2Int(maxPackSize.x, maxPackSize.x * tex.height / tex.width);
                    }

                    return new IntRectangle(0, 0, tex.width, tex.height);
                }
                else // 如果需要裁剪
                {
                    if (tex.width < maxPackSize.x)
                    {
                        packSize = new Vector2Int(tex.width, croppedHeight);
                    }
                    else
                    {
                        packSize = maxPackSize;
                    }
                    return new IntRectangle(0, (tex.height - croppedHeight)/2, tex.width, croppedHeight);
                }
            }
            else
            {
                // 对于Vertical的情况，确保缩放时，宽度取整时总是小于等于对应宽高比的宽度
                var maxPackSize = m_Size.x <= m_Size.y
                    ? new Vector2Int(Mathf.FloorToInt(1.0f * m_Resolution * m_Size.x / m_Size.y), m_Resolution)
                    : new Vector2Int(m_Resolution, Mathf.CeilToInt(1.0f * m_Resolution * m_Size.y / m_Size.x));

                int croppedWidth = tex.height * maxPackSize.x / maxPackSize.y;
                if (croppedWidth >= tex.width) // 如果不裁切
                {
                    // 如果原图尺寸比最大打包尺寸要小，则直接使用原图尺寸打包
                    if (tex.height < maxPackSize.y)
                    {
                        packSize = new Vector2Int(tex.width, tex.height);
                    }
                    else
                    {
                        packSize = new Vector2Int(maxPackSize.y * tex.width / tex.height, maxPackSize.y);
                    }

                    return new IntRectangle(0, 0, tex.width, tex.height);
                }
                else // 如果需要裁剪
                {
                    if (tex.height < maxPackSize.y)
                    {
                        packSize = new Vector2Int(croppedWidth, tex.height);
                    }
                    else
                    {
                        packSize = maxPackSize;
                    }
                    return new IntRectangle((tex.width - croppedWidth)/2, 0, croppedWidth, tex.height);
                }
            }
        }

        protected override SpriteJson PackFromSource(AbstractPackContext packContext, Texture2D source)
        {
            var cropRect = CalcPackAndCrop(source, out var packSize);
            var spriteIndex = packContext.AddSprite(source, cropRect, packSize);
            return new SpriteJson()
            {
                sprite = spriteIndex,
            };
        }

        protected override Sprite UnpackToTarget(CraftUnpackContext unpackContext, SpriteJson spriteJson)
        {
            var spriteRect = unpackContext.GetAtlasRect(spriteJson.sprite);
            var pixelsPerUnit = 100.0f;
            if (m_FitViewAxis == FitViewAxis.Horizontal)
            {
                pixelsPerUnit = 100.0f * spriteRect.width / m_Size.x;
            }
            else
            {
                pixelsPerUnit = 100.0f * spriteRect.height / m_Size.y;
            }
            return unpackContext.GenSprite(spriteJson.sprite, m_Pivot, pixelsPerUnit);
        }

        protected override void OnChangeValue()
        {
            drawRenderer.sprite = target;
        }

        protected override Sprite ValueProcess(Texture2D source)
        {
            var cropRect = CalcPackAndCrop(source, out _);
            var pixelsPerUnit = 100.0f;
            if (m_FitViewAxis == FitViewAxis.Horizontal)
            {
                pixelsPerUnit = 100.0f * source.width / m_Size.x;
            }
            else
            {
                pixelsPerUnit = 100.0f * source.height / m_Size.y;
            }
            return Sprite.Create(source, cropRect.ToUnityRect(), m_Pivot, pixelsPerUnit);
        }

        protected override void DestroyTarget(Sprite postSprite)
        {
            if (PlatformUtility.UNITY_EDITOR)
            {
                DestroyImmediate(postSprite);
            }
            else
            {
                Destroy(postSprite);
            }
        }
#if UNITY_EDITOR
        [BlackList]
        public override void OnInspectorChange()
        {
            m_Size = new Vector2Int(Math.Max(1, m_Size.x), Math.Max(1, m_Size.y));
            m_Resolution = Math.Clamp(m_Resolution, 1, 1024);
            var boxSize = new Vector2(m_Size.x / 100.0f, m_Size.y / 100.0f);
            touchCollider2D.size = boxSize;
            touchCollider2D.offset = new Vector2(
                boxSize.x * (0.5f - m_Pivot.x),
                boxSize.y * (0.5f - m_Pivot.y)
                );
            base.OnInspectorChange();
        }
#endif
    }
}