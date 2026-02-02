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
        public void RegisterBuiltinObject(string builtinPath, UnityEngine.Object builtinObj);
        public bool IsBuiltinObject(UnityEngine.Object builtinObj, out string builtinPath);
    }
}