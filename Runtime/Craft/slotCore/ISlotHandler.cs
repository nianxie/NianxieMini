using System.Collections.Generic;
using Nianxie.Framework;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    // TODO，改成interface
    public interface ISlotHandler
    {
        public void ShellRefresh();
        public void Incref(AbstractSlotCom com, Texture2D tex);
        public void Decref(AbstractSlotCom com, Texture2D tex);
        public void OnSelect(SlotSelectHead slotSelect);
    }
}