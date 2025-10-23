using System;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    public abstract class AbstractSlotCom:MonoBehaviour
    {
        [NonSerialized] public EditRoot editRoot;
        public abstract AbstractSlotJson PackToJson(AbstractPackContext packContext);
        public abstract void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson);
        public abstract object ReadData();
        protected virtual void Awake()
        {
            var pos = transform.localPosition;
            transform.localPosition = new Vector3(pos.x, pos.y, -0.1f);
        }
#if UNITY_EDITOR
        [BlackList]
        public virtual void OnInspectorUpdate(bool change)
        {
        }
#endif
    }
}