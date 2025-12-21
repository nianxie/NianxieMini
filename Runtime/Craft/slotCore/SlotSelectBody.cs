using System;
using System.Collections;
using System.Collections.Generic;
using Nianxie.Utils;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class SlotSelectBody : AbstractSlotSelect
    {
        [SerializeField] private SlotSelectHead m_SlotSelectHead;
        public SlotSelectHead selectHead => m_SlotSelectHead;
        
        [NonSerialized] private BoxCollider2D m_Collider2D;
        public BoxCollider2D touchCollider2D
        {
            get
            {
                if (!m_Collider2D)
                {
                    m_Collider2D = GetComponent<BoxCollider2D>();
                }
                return m_Collider2D;
            }
        }
        
        [NonSerialized] SpriteRenderer m_SpriteRenderer;
        public SpriteRenderer spriteRenderer
        {
            get
            {
                if (!m_SpriteRenderer)
                {
                    m_SpriteRenderer = GetComponent<SpriteRenderer>();
                }
                return m_SpriteRenderer;
            }
        }
#if UNITY_EDITOR
        [BlackList]
        public override void EditorInspectorUpdate()
        {
            SlotSelectHead head = null;
            var parent = transform.parent;
            if (parent != null)
            {
                parent.TryGetComponent(out head);
            }
            if (m_SlotSelectHead != head)
            {
                m_SlotSelectHead = head;
                UnityEditor.EditorUtility.SetDirty(this);
                if (m_SlotSelectHead == null)
                {
                    Debug.LogError($"{nameof(SlotSelectBody)} require a {nameof(SlotSelectHead)} in parent");
                }
            }

            if (m_SlotSelectHead != null)
            {
                m_SlotSelectHead.EditorLocalUpdate();
            }
            EditorLocalUpdate();
        }
        [BlackList]
        public override void EditorLocalUpdate()
        {
            var col = touchCollider2D;
            var render = spriteRenderer;
            var sprite = render.sprite;
            if (sprite != null)
            {
                var bounds = render.localBounds;
                if (col.size != bounds.size.ToVector2() || col.offset != bounds.center.ToVector2())
                {
                    col.size = bounds.size;
                    col.offset = bounds.center;
                    UnityEditor.EditorUtility.SetDirty(col);
                }
            }
            else
            {
                col.size = Vector2.one;
                col.offset = Vector2.zero;
            }
            if (gameObject.name != SELECT_BODY_NAME)
            {
                gameObject.name = SELECT_BODY_NAME;
                UnityEditor.EditorUtility.SetDirty(gameObject);
            }
        }
#endif
    }
}
