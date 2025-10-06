using System.Collections.Generic;
using System.Linq;
using Nianxie.Components;
using UnityEngine;

namespace XLua
{
    /// <summary>
    /// injection 是luabehaviour的情况
    /// </summary>
    public class ScriptInjection:AbstractNodeInjection
    {
        public readonly LuaTable clsOpen;
        public readonly WarmedReflectClass nestedClass;
        public bool isNested => nestedClass != null;
        public ScriptInjection(WarmedReflectClass cls, string[] nestedKeys, RawReflectInjection rawInjection, InjectionMultipleKind kind) : base(cls, rawInjection, kind)
        {
            clsOpen = rawInjection.clsOpen;
            if (!cls.reflectEnv.IsFileClass(clsOpen))
            {
                nestedClass = WarmedReflectClass.Create(cls.reflectEnv, clsOpen, cls.classPath, nestedKeys.Concat(new []{key}).ToArray());
            }
        }

        public override Object ToNodeObject(LuaBehaviour behav, string targetNodePath)
        {
            return ToLuaBehaviour(behav, targetNodePath);
        }

        public LuaBehaviour ToLuaBehaviour(LuaBehaviour behav, string targetNodePath)
        {
            var trans = behav.transform.Find(targetNodePath);
            if (trans==null)
            {
                return null;
            }
            return trans.GetComponent<LuaBehaviour>();
        }
        public LuaTable ToLuaScript(LuaBehaviour behav, string targetNodePath)
        {
            var luaBehav = ToLuaBehaviour(behav, targetNodePath);
            return luaBehav.luaTable;
        }
    }
}