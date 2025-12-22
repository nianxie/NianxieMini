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
    public abstract class OldAssetSlot<TSlotJson, TRawData, TFinalData>: AbstractAssetSlot where TSlotJson:AbstractSlotJson
    {
        private class UserData
        {
            public readonly TRawData rawData;
            public readonly TFinalData finalData;
            public UserData(TRawData rawData, TFinalData finalData)
            {
                this.rawData = rawData;
                this.finalData = finalData;
            }
            // 只赋值target的情况，表示target是从外部传来的参数，不会在这里处理
            public UserData(TFinalData finalData)
            {
                this.rawData = default;
                this.finalData = finalData;
            }

            public void OnDestroy(Action<TFinalData> destroyTarget)
            {
                destroyTarget(finalData);
            }
        }

        [SerializeField] private TRawData m_DefaultRawData;

        protected TRawData defaultRawData
        {
            get => m_DefaultRawData;
            set => m_DefaultRawData = value;
        }
        [NonSerialized] protected TFinalData defaultFinalData;
        private UserData userData;
        protected TFinalData finalData => userData!=null?userData.finalData:defaultFinalData;

        protected abstract void OnDataModify();
        protected abstract TFinalData DataProcess(TRawData rawData);
        protected virtual void DestroyFinalData(TFinalData finalData) {}

        protected virtual void OnEnable()
        {
            if (userData == null)
            {
                if (defaultFinalData != null)
                {
                    DestroyFinalData(defaultFinalData);
                }

                if (defaultRawData == null)
                {
                    defaultFinalData = default;
                }
                else
                {
                    defaultFinalData = DataProcess(defaultRawData);
                }
            }
            OnDataModify();
        }

        public override void WriteRawData(object obj)
        {
            var source = (TRawData) obj;
            userData?.OnDestroy(DestroyFinalData);
            userData = new UserData(source, DataProcess(source));
            OnDataModify();
        }

        protected abstract TSlotJson PackFromRawData(AbstractPackContext packContext, TRawData rawData);
        protected abstract TFinalData UnpackToFinalData(CraftUnpackContext unpackContext, TSlotJson slotJson);

        public sealed override AbstractSlotJson PackToJson(AbstractPackContext packContext)
        {
            if (userData != null)
            {
                return PackFromRawData(packContext, userData.rawData);
            }
            else
            {
                return DefaultSlotJson.Instance;
            }
        }
        public sealed override void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson absSlotJson)
        {
            userData?.OnDestroy(DestroyFinalData);
            if (absSlotJson is TSlotJson slotJson)
            {
                userData = new UserData(UnpackToFinalData(unpackContext, (TSlotJson) slotJson));
            }
            else
            {
                userData = null;
                if (!(absSlotJson is DefaultSlotJson))
                {
                    Debug.LogError($"slot-com-{GetType()} not match slot-json-{absSlotJson.GetType()} when unpack");
                }
            }
            OnDataModify();
        }
#if UNITY_EDITOR
        [BlackList]
        public override void EditorInspectorUpdate(bool change)
        {
            base.EditorInspectorUpdate(change);
            if (!change) return;
            if (defaultFinalData != null)
            {
                DestroyFinalData(defaultFinalData);
            }
            defaultFinalData = DataProcess(defaultRawData);
            OnDataModify();
        }
#endif
    }
    [RequireComponent(typeof(SpriteRenderer))]
    public class TextureSlot : OldAssetSlot<TextureJson, Texture2D, Sprite>
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
        private SpriteRenderer drawRenderer
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

        protected override TextureJson PackFromRawData(AbstractPackContext packContext, Texture2D tex)
        {
            var cropRect = CalcPackAndCrop(tex, out var packSize);
            var spriteIndex = packContext.AddSprite(tex, cropRect, packSize);
            return new TextureJson()
            {
                sprite = spriteIndex,
            };
        }

        protected override Sprite UnpackToFinalData(CraftUnpackContext unpackContext, TextureJson textureJson)
        {
            var spriteRect = unpackContext.GetAtlasRect(textureJson.sprite);
            var pixelsPerUnit = 100.0f;
            if (m_FitViewAxis == FitViewAxis.Horizontal)
            {
                pixelsPerUnit = 100.0f * spriteRect.width / m_Size.x;
            }
            else
            {
                pixelsPerUnit = 100.0f * spriteRect.height / m_Size.y;
            }
            return unpackContext.GenSprite(textureJson.sprite, m_Pivot, pixelsPerUnit);
        }

        protected override void OnDataModify()
        {
            drawRenderer.sprite = finalData;
        }

        protected override Sprite DataProcess(Texture2D tex)
        {
            if (tex != null)
            {
                var cropRect = CalcPackAndCrop(tex, out _);
                var pixelsPerUnit = 100.0f;
                if (m_FitViewAxis == FitViewAxis.Horizontal)
                {
                    pixelsPerUnit = 100.0f * tex.width / m_Size.x;
                }
                else
                {
                    pixelsPerUnit = 100.0f * tex.height / m_Size.y;
                }
                return Sprite.Create(tex, cropRect.ToUnityRect(), m_Pivot, pixelsPerUnit);
            }
            else
            {
                return null;
            }
        }

        protected override void DestroyFinalData(Sprite finalSprite)
        {
            if (PlatformUtility.UNITY_EDITOR)
            {
                DestroyImmediate(finalSprite);
            }
            else
            {
                Destroy(finalSprite);
            }
        }
#if UNITY_EDITOR
        [BlackList]
        public override void EditorInspectorUpdate(bool change)
        {
            if (!change)
            {
                return;
            }

            m_Size = new Vector2Int(Math.Max(1, m_Size.x), Math.Max(1, m_Size.y));
            m_Resolution = Math.Clamp(m_Resolution, 1, 1024);
            var boxSize = new Vector2(m_Size.x / 100.0f, m_Size.y / 100.0f);
            /*touchCollider2D.size = boxSize;
            touchCollider2D.offset = new Vector2(
                boxSize.x * (0.5f - m_Pivot.x),
                boxSize.y * (0.5f - m_Pivot.y)
                );*/
            base.EditorInspectorUpdate(change);
        }
#endif
    }
}