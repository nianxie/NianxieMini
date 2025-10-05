using System.Collections.Generic;
using Nianxie.Components;
using UnityEngine;

namespace XLua
{
    /// <summary>
    /// gameObject和component(包括luabehaviour)的injection
    /// </summary>
    public abstract class AbstractNodeInjection:AbstractMultipleInjection
    {
        public readonly string nodePath;
        public readonly string[] nodePathList;
        public readonly InjectionMultipleKind collectionKind;
        public AbstractNodeInjection(WarmedReflectClass cls, RawReflectInjection rawInjection, InjectionMultipleKind kind) : base(cls, rawInjection, kind)
        {
            if (rawInjection.table)
            {
                nodePathList = rawInjection.nodePathTable.Cast<string[]>();
                _count = nodePathList.Length;
            }
            else
            {
                nodePath = rawInjection.nodePath;
                _count = 1;
            }
        }

        public abstract Object ToNodeObject(LuaBehaviour behav, string targetNodePath);
        public IEnumerable<string> EachNodePath()
        {
            if (collectionKind == InjectionMultipleKind.Single)
            {
                yield return nodePath;
            }
            else
            {
                foreach (var a in nodePathList)
                {
                    yield return a;
                }
            }
        }
    }
}