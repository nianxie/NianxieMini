using System;
using System.Collections.Generic;
using System.Text;

namespace XLua
{
    public class HintReturnAttribute: Attribute
    {
        public Type RetType { get; }
        public string RetLuaHint { get; }
        public bool UseTask { get; }

        public string DumpHint(Func<Type, string> DumpTypeName)
        {
            var name = "";
            if (RetType != null)
            {
                name = DumpTypeName(RetType);
            }
            else
            {
                name = RetLuaHint;
            }

            if (UseTask)
            {
                return "Future("+name+")";
            }
            else
            {
                return name;
            }
        }

        public HintReturnAttribute(string retLuaHint, bool useTask = false)
        {
            RetLuaHint = retLuaHint;
            UseTask = useTask;
        }

        public HintReturnAttribute(Type retType, bool useTask=false)
        {
            RetType = retType;
            UseTask = useTask;
        }
    }
}