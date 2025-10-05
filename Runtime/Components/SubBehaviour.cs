using System.Net;
using Nianxie.Framework;
using UnityEngine;
using XLua;

namespace Nianxie.Components
{
    public abstract class SubBehaviour : MonoBehaviour
    {
        protected LuaTable self;
        public abstract void Init(bool enabled, LuaTable luaTable, PartVtbl partVtbl);
    }

    public abstract class SubBehaviour<TSubVtbl> : SubBehaviour where TSubVtbl : PartVtbl
    {
        protected TSubVtbl subTable;

        public override void Init(bool miniEnabled, LuaTable miniLuaTable, PartVtbl partVtbl)
        {
            enabled = miniEnabled;
            self = miniLuaTable;
            subTable = (TSubVtbl)partVtbl;
        }
    }
}