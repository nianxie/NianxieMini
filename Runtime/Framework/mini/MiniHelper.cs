using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nianxie.Utils;
using UnityEngine;
using LuaAPI = XLua.LuaDLL.Lua;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
using XLua;

namespace Nianxie.Framework
{
    public class MiniHelper : AsyncHelper
    {
        private MiniGameManager miniManager;
        public void PlayEnding()
        {
            miniManager.args.PlayEnding(miniManager);
        }

        public LuaTable GetCraftSlot()
        {
            return miniManager.craftSlot.luaTable;
        }
    }
}
