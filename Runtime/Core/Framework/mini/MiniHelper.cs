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
        [SerializeField] private AbstractEntryModule entryModule;
        public void PlayEnding()
        {
            entryModule.PlayEnding();
        }

        public LuaTable GetCraftTable()
        {
            throw new Exception("craft table TODO");
            //return miniManager.craftTable;
        }
    }
}
