using UnityEngine;
using System;
using System.Collections.Generic;
using XLua;
using Debug = UnityEngine.Debug;

namespace Nianxie.Craft
{

    [RequireComponent(typeof(SpriteSlot))]
    [DisallowMultipleComponent]
    public class PolygonSlot:AbstractSlotCom<SpritePolygon, PolygonJson>
    {
        
        [NonEditable]
        [SerializeField]
        private SpritePolygon m_DefaultPolygon;
        [NonEditable]
        [SerializeField]
        private Sprite m_DefaultSprite;
        [NonEditable]
        [SerializeField]
        private string m_DefaultSpriteHash;

        [NonSerialized]
        private SpritePolygon m_AssignedPolygon = null;
        
        [NonSerialized] SpriteSlot m_SpriteSlot;
        private SpriteSlot spriteSlot
        {
            get
            {
                if (!m_SpriteSlot)
                {
                    m_SpriteSlot = GetComponent<SpriteSlot>();
                }
                return m_SpriteSlot;
            }
        }
        
        private SpritePolygon currentPolygon => m_AssignedPolygon ?? m_DefaultPolygon;

        public void Assign(SpritePolygon spritePolygon)
        {
            m_AssignedPolygon = spritePolygon;
        }

        protected override PolygonJson TypedPackToJson()
        {
            return new PolygonJson()
            {
                paths = currentPolygon.ToPaths(),
            };
        }
        
        protected override void TypedUnpackFromJson(PolygonJson slotJson)
        {
            m_AssignedPolygon = SpritePolygon.FromPaths(slotJson.paths);
        }
#if UNITY_EDITOR
        private static string HashSprite(Sprite targetSprite)
        {
            return targetSprite==null?"":$"{targetSprite.rect}_{targetSprite.pivot}_{targetSprite.pixelsPerUnit}_{targetSprite.texture.imageContentsHash}";
        }
        [BlackList]
        public override void EditorInspectorUpdate(NianxieDefaultAssets defaultAssets)
        {
            var sprite = spriteSlot.defaultSprite;
            string spriteHash = HashSprite(sprite);
            if (sprite == m_DefaultSprite && spriteHash == m_DefaultSpriteHash)
            {
                return;
            }

            if (sprite != null && !sprite.texture.isReadable)
            {
                Debug.LogError($"texture is not readable {sprite.texture} when use {nameof(PolygonSlot)}");
                return;
            }

            m_DefaultSprite = sprite;
            m_DefaultSpriteHash = spriteHash;
            if (sprite == null)
            {
                m_DefaultPolygon = null;
            }
            else
            {
                m_DefaultPolygon=SpritePolygon.FromPaths(ContourTracer.CalcPolygon(sprite));
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
        private void OnDrawGizmos()
        {
            var polygon = currentPolygon;
            if (polygon == null)
            {
                return;
            }

            foreach(var path in polygon.paths)
            {
                var points = path.points;
                // 如果点太少，不绘制
                if (points == null || points.Length < 2) return;

                // 设置颜色（类似 Collider 的绿色）
                Gizmos.color = new Color(0.5f, 0.0f, 0.9f);

                // 循环绘制线段
                for (int i = 0; i < points.Length; i++)
                {
                    // 获取当前点和下一个点
                    Vector2 p1 = points[i];
                    Vector2 p2;

                    // 处理闭合逻辑
                    if (i == points.Length - 1)
                    {
                        p2 = points[0];
                    }
                    else
                    {
                        p2 = points[i + 1];
                    }

                    // 关键：将局部坐标转换为世界坐标
                    Vector3 worldP1 = transform.TransformPoint(p1);
                    Vector3 worldP2 = transform.TransformPoint(p2);

                    Gizmos.DrawLine(worldP1, worldP2);
                }
            }
        }
#endif
    }
}