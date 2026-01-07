using System.Collections.Generic;
using Nianxie.Framework;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    public abstract class SlotCallback:MonoBehaviour
    {
        private Dictionary<int, Dictionary<int, AbstractSlotCom>> texIdToComDict = new();
        protected abstract MiniEditArgs editArgs { get; }

        [BlackList]
        public void ShellRefresh()
        {
            editArgs.shellRefresh.Action();
        }
        [BlackList]
        public void Incref(AbstractSlotCom com, Texture2D tex)
        {
            if (!texIdToComDict.TryGetValue(tex.GetInstanceID(), out var comDict))
            {
                comDict = new Dictionary<int, AbstractSlotCom>();
                texIdToComDict[tex.GetInstanceID()] = comDict;
            }
            comDict[com.GetInstanceID()] = com;
        }
        [BlackList]
        public void Decref(AbstractSlotCom com, Texture2D tex)
        {
            if (texIdToComDict.TryGetValue(tex.GetInstanceID(), out var comDict))
            {
                if (comDict.ContainsKey(com.GetInstanceID()))
                {
                    comDict.Remove(com.GetInstanceID());
                    if (comDict.Count <= 0)
                    {
                        texIdToComDict.Remove(tex.GetInstanceID());
                        editArgs.shellRelease.Action(tex);
                    }
                }
            }
        }

        [BlackList]
        public abstract void OnSelect(SlotSelectHead slotSelect);
    }
}