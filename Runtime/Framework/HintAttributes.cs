using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace XLua
{
    public class HintReturnAttribute: Attribute
    {
        enum HintKind
        {
            Normal=0,
            RetFuture=1,
            AsyncFuture=2,
        }

        private Type[] parTypes { get; }
        private Type retType { get; }
        private string retLuaHint { get; }
        private HintKind hintKind { get; }

        public string DumpHint(Func<Type, string> DumpTypeName)
        {
            if(hintKind == HintKind.AsyncFuture)
            {
                StringBuilder sb = new();
                var strArr = new []{"Fn($self"}.Concat(parTypes.Select(DumpTypeName));
                sb.Append(string.Join(",", strArr));
                sb.Append("):Ret(Future(");
                if (retType == null)
                {
                    sb.Append("Nil");
                }
                else
                {
                    sb.Append(DumpTypeName(retType));
                }
                sb.Append("))");
                return sb.ToString();
            }
            else
            {
                var name = "";
                if (retType != null)
                {
                    name = DumpTypeName(retType);
                }
                else
                {
                    name = retLuaHint;
                }

                if (hintKind == HintKind.RetFuture)
                {
                    return "Future("+name+")";
                }
                if (hintKind == HintKind.Normal)
                {
                    return name;
                }
            }
            return "Truth --[[ error fallback]]";
        }

        public HintReturnAttribute(string retLuaHint, bool useTask=false)
        {
            this.retLuaHint = retLuaHint;
            hintKind = useTask?HintKind.RetFuture:HintKind.Normal;
        }

        public HintReturnAttribute(Type retType, bool useTask=false)
        {
            this.retType = retType;
            hintKind = useTask?HintKind.RetFuture:HintKind.Normal;
        }
        public HintReturnAttribute(Type[] parTypes, Type retType=null)
        {
            this.parTypes = parTypes;
            this.retType = retType;
            hintKind = HintKind.AsyncFuture;
        }
    }
}