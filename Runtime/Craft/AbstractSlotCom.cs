using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

namespace Nianxie.Craft
{
    public abstract class AbstractSlotCom<TSlotJson, TSourceValue, TTargetValue>: AbstractSlotCom where TSlotJson:AbstractSlotJson
    {
        private class UserValue
        {
            public readonly TSourceValue source;
            public readonly TTargetValue target;
            public UserValue(TSourceValue source, TTargetValue target)
            {
                this.source = source;
                this.target = target;
            }
            // 只赋值target的情况，表示target是从外部传来的参数，不会在这里处理
            public UserValue(TTargetValue target)
            {
                this.source = default;
                this.target = target;
            }

            public void OnDestroy(Action<TTargetValue> destroyPost)
            {
                destroyPost(target);
            }
        }

        [SerializeField] private TSourceValue m_DefaultSource;
        protected TSourceValue defaultSource => m_DefaultSource;
        [NonSerialized] protected TTargetValue defaultTarget;
        private UserValue userValue;
        protected TTargetValue target => userValue!=null?userValue.target:defaultTarget;

        protected abstract void OnChangeValue();
        protected abstract TTargetValue ValueProcess(TSourceValue source);
        protected virtual void DestroyTarget(TTargetValue target) {}

        protected virtual void OnEnable()
        {
            if (userValue == null)
            {
                if (defaultTarget != null)
                {
                    DestroyTarget(defaultTarget);
                }

                if (defaultSource == null)
                {
                    defaultTarget = default;
                }
                else
                {
                    defaultTarget = ValueProcess(defaultSource);
                }
            }
            OnChangeValue();
        }

        public void AssignSource(TSourceValue source)
        {
            userValue?.OnDestroy(DestroyTarget);
            userValue = new UserValue(source, ValueProcess(source));
            OnChangeValue();
        }

        protected abstract TSlotJson PackFromSource(AbstractPackContext packContext, TSourceValue source);
        protected abstract TTargetValue UnpackToTarget(CraftUnpackContext unpackContext, TSlotJson slotJson);

        public sealed override AbstractSlotJson PackToJson(AbstractPackContext packContext)
        {
            if (userValue != null)
            {
                return PackFromSource(packContext, userValue.source);
            }
            else
            {
                return DefaultSlotJson.Instance;
            }
        }
        public sealed override void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson absSlotJson)
        {
            userValue?.OnDestroy(DestroyTarget);
            if (absSlotJson is TSlotJson slotJson)
            {
                userValue = new UserValue(UnpackToTarget(unpackContext, (TSlotJson) slotJson));
            }
            else
            {
                userValue = null;
                if (!(absSlotJson is DefaultSlotJson))
                {
                    Debug.LogError($"slot-com-{GetType()} not match slot-json-{absSlotJson.GetType()} when unpack");
                }
            }
            OnChangeValue();
        }
#if UNITY_EDITOR
        public override void OnInspectorChange()
        {
            if (defaultTarget != null)
            {
                DestroyTarget(defaultTarget);
            }
            defaultTarget = ValueProcess(defaultSource);
            OnChangeValue();
        }
#endif
    }
    
    [ExecuteAlways]
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class AbstractSlotCom : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler
    {
        [NonSerialized] public CraftModule craftModule;
        [NonSerialized] BoxCollider2D m_collider2D;
        public BoxCollider2D touchCollider2D
        {
            get
            {
                if (!m_collider2D)
                {
                    gameObject.TryGetComponent(out m_collider2D);
                }
                return m_collider2D;
            }
        }
        public abstract AbstractSlotJson PackToJson(AbstractPackContext packContext);
        public abstract void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson);
        public void OnPointerDown(PointerEventData eventData)
        {
            craftModule.DispatchSlotPointer(this, nameof(OnPointerDown), eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            craftModule.DispatchSlotPointer(this, nameof(OnPointerClick), eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            craftModule.DispatchSlotPointer(this, nameof(OnPointerUp), eventData);
        }
#if UNITY_EDITOR
        public virtual void OnInspectorChange()
        {
        }
#endif
    }
    
}
