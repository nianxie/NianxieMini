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
        private LuaFunction _call;
        public LuaFunction Call => _call;
        private MiniGameManager miniManager;
        [BlackList]
        public override UniTask LateInit()
        {
            miniManager = (MiniGameManager) gameManager;
            var miniL = gameManager.reflectEnv.L;
            int oldTop = LuaAPI.lua_gettop(miniL);
            LuaAPI.lua_pushstdcallcfunction(miniL, callShell);
            var translator = ObjectTranslatorPool.Instance.PrimaryFind(miniL);
            translator.Get(miniL, -1, out _call);
            LuaAPI.lua_settop(miniL, oldTop);
            return UniTask.CompletedTask;
        }

        public void PlayEnding()
        {
            miniManager.bridgeSession.PlayEnding();
        }

        public LuaTable GetCraftSlot()
        {
            return miniManager.craftSlot.luaTable;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int callShell(IntPtr miniL)
        {
            return LuaAPI.luaL_error(miniL, "mini call TODO");
            try
            {
                var translator = ObjectTranslatorPool.Instance.PrimaryFind(miniL);
                var miniHelper = (MiniHelper)translator.FastGetCSObj(miniL, 1);
                var caster = translator.objectCasters.GetCaster(typeof(object));
                int gen_param_count = LuaAPI.lua_gettop(miniL);
                ArrayList args =new ArrayList();
                for (int i = 2; i <= gen_param_count; i++)
                {
                    var obj = caster(miniL, i, null);
                    args.Add(obj);
                }
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(miniL, "mini call exception" + e);
            }
            return 0;
        }
    }
}
