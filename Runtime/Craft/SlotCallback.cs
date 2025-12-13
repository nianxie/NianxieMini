using System.Collections.Generic;
using Nianxie.Framework;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    public abstract class SlotCallback:MonoBehaviour
    {
        private Dictionary<int, Dictionary<int, AbstractSlotCom>> resIdToComDict;
        public AbstractNodeSlot selectNodeSlot { get; private set; }
        public PositionSlot selectPosSlot { get; private set; }
        protected RuntimeReflectEnv reflectEnv;
        public LuaTable NewTable()
        {
            return reflectEnv.NewTable();
        }
        protected MiniEditArgs editArgs;
        public void ShellRefresh()
        {
            editArgs.shellRefresh.Action();
        }
        public void Incref(AbstractSlotCom com, UnityEngine.Object obj)
        {
            if (!resIdToComDict.TryGetValue(obj.GetInstanceID(), out var comDict))
            {
                comDict = new Dictionary<int, AbstractSlotCom>();
                resIdToComDict[obj.GetInstanceID()] = comDict;
            }
            comDict[com.GetInstanceID()] = com;
        }
        public void Decref(AbstractSlotCom com, UnityEngine.Object obj)
        {
            if (resIdToComDict.TryGetValue(obj.GetInstanceID(), out var comDict))
            {
                if (comDict.ContainsKey(com.GetInstanceID()))
                {
                    comDict.Remove(com.GetInstanceID());
                    if (comDict.Count <= 0)
                    {
                        resIdToComDict.Remove(obj.GetInstanceID());
                        editArgs.shellRelease.Action(obj);
                    }
                }
            }
        }
        public void OnSelect(AbstractNodeSlot assetSlot)
        {
            if (assetSlot == null)
            {
                selectNodeSlot = null;
                selectPosSlot = null;
            }
            else
            {
                selectNodeSlot = assetSlot;
                selectPosSlot = assetSlot.GetComponentInParent<PositionSlot>();
            }
            ShellRefresh();
        }
    }
}