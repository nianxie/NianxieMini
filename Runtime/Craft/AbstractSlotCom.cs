using System;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    public abstract class AbstractSlotCom:MonoBehaviour
    {
        [NonSerialized] public CraftModule craftModule;
        public abstract AbstractSlotJson PackToJson(AbstractPackContext packContext);
        public abstract void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson);
        public abstract object ReadData();
#if UNITY_EDITOR
        [BlackList]
        public virtual void OnInspectorUpdate(bool change)
        {
        }
#endif
    }
}