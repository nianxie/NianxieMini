using System.Reflection;
using UnityEngine;
using XLua;

namespace Nianxie.Framework
{
    public class BootTable
    {
        public LuaFunction Repl;
        // for task 
        public LuaFunction task;
        public LuaFunction sleep;
        public LuaFunction complete;
        // for json
        public LuaFunction rapidjsonDecode;
        public BootTable(LuaTable luaTable)
        {
            // 偷懒，用反射自动映射public的变量并绑定
            var type = typeof(BootTable);
            foreach(var field in type.GetFields(BindingFlags.Public|BindingFlags.Instance))
            {
                if (field.FieldType == typeof(LuaFunction))
                {
                    field.SetValue(this,luaTable.Get<LuaFunction>(field.Name));
                }
                else
                {
                    Debug.LogError($"other type TODO, {field.FieldType}");
                }
            }
        }
    }
}