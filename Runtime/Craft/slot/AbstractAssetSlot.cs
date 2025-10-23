using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XLua;

namespace Nianxie.Craft
{
    public abstract class AbstractAssetSlot<TSlotJson, TRawData, TFinalData>: AbstractAssetSlot where TSlotJson:AbstractSlotJson
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
        
        public override object ReadData()
        {
            return finalData;
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
        public override void OnInspectorUpdate(bool change)
        {
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
    
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class AbstractAssetSlot : AbstractSlotCom, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler
    {
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

        public abstract void WriteRawData(object rawData);

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            //craftModule.DispatchSlotPointer(this, nameof(IPointerDownHandler.OnPointerDown), eventData);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (!TryGetComponent<PositionSlot>(out var posSlot) || !posSlot.dragging)
            {
                editRoot.OnSelect(this);
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            //craftModule.DispatchSlotPointer(this, nameof(IPointerUpHandler.OnPointerUp), eventData);
        }
    }
    
}
