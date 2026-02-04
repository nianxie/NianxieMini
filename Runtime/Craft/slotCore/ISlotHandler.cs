using System.Collections.Generic;
using Nianxie.Framework;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    public interface ISlotHandler
    {
        public void ShellRefresh();
        public void OnSelect(SlotSelectHead slotSelect);
        public AssetUsageCenter assetUsageCenter { get; }
    }
}