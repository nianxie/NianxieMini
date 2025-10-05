using System.Collections.Generic;
using Nianxie.Components;
using UnityEngine;

namespace XLua
{
    /// <summary>
    /// c#层暴露给lua层的component以及gameObject使用该injection提供,不包括luabehaviour
    /// </summary>
    public class ComponentInjection:AbstractNodeInjection
    {
        public ComponentInjection(WarmedReflectClass cls, RawReflectInjection rawInjection, InjectionMultipleKind kind) : base(cls, rawInjection, kind)
        {
        }

        public override Object ToNodeObject(LuaBehaviour behav, string targetNodePath)
        {
            return ToComponent(behav, targetNodePath);
        }

        public Component ToComponent(LuaBehaviour behav, string targetNodePath)
        {
            var trans = behav.transform.Find(targetNodePath);
            if (trans==null)
            {
                return null;
            }
            return trans.GetComponent(csharpType);
        }
    }
}