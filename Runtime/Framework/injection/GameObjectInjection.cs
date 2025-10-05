using System.Collections.Generic;
using Nianxie.Components;
using UnityEngine;

namespace XLua
{
    /// <summary>
    /// c#层暴露给lua层的component以及gameObject使用该injection提供,不包括luabehaviour
    /// </summary>
    public class GameObjectInjection:AbstractNodeInjection
    {
        public GameObjectInjection(WarmedReflectClass cls, RawReflectInjection rawInjection, InjectionMultipleKind kind) : base(cls, rawInjection, kind)
        {
        }

        public override Object ToNodeObject(LuaBehaviour behav, string targetNodePath)
        {
            return ToGameObject(behav, targetNodePath);
        }
        
        public GameObject ToGameObject(LuaBehaviour behav, string targetNodePath)
        {
            var trans = behav.transform.Find(targetNodePath);
            if (trans==null)
            {
                return null;
            }
            return trans.gameObject;
        }
    }
}