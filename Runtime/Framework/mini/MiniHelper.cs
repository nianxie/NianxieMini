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
        private MiniGameManager miniManager => (MiniGameManager)gameManager;
        public void PlayEnding()
        {
            miniManager.playArgs.PlayEnding(miniManager);
        }

        public LuaTable GetCraftTable()
        {
            var craftSlot = miniManager.editRoot.rootSlot;
            if (craftSlot != null)
            {
                return craftSlot.behav.luaTable;
            }
            else
            {
                return null;
            }
        }
    }
}
