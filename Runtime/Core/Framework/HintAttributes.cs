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

        // 一些用C#的Type表达起来比较麻烦的情况，直接用string
        public HintReturnAttribute(string retLuaHint, bool useFuture=false)
        {
            this.retLuaHint = retLuaHint;
            hintKind = useFuture?HintKind.RetFuture:HintKind.Normal;
        }

        // TODO 这些是旧的基于WrapTask的，Task开头的后续考虑都用下面的重构掉，一些特定的在C#层创建Future的用法可以保留
        public HintReturnAttribute(Type retType, bool useFuture=false)
        {
            this.retType = retType;
            hintKind = useFuture?HintKind.RetFuture:HintKind.Normal;
        }
        
        /// <summary>
        /// lua_CSFunction形式的异步函数用这个修饰，因为无法获取par，需要自己填入parType。
        /// </summary>
        /// <param name="parTypes"></param>
        /// <param name="retType"></param>
        public HintReturnAttribute(Type[] parTypes, Type retType=null)
        {
            this.parTypes = parTypes;
            this.retType = retType;
            hintKind = HintKind.AsyncFuture;
        }
    }
}