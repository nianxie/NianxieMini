using System;
using Nianxie.Components;
using Object = UnityEngine.Object;

namespace XLua
{
    /// <summary>
    /// gameObject和component(包括luabehaviour)的injection
    /// </summary>
    public abstract class AbstractNodeInjection:AbstractMultipleInjection
    {
        public readonly string nodePath;
        private readonly string[] nodePathArr;
        public ReadOnlySpan<string> nodePathList => new (nodePathArr);
        protected AbstractNodeInjection(WarmedReflectClass cls, RawReflectInjection rawInjection, InjectionMultipleKind kind) : base(cls, rawInjection, kind)
        {
            if (rawInjection.table)
            {
                nodePath = null;
                nodePathArr = rawInjection.nodePathTable.Cast<string[]>();
                _count = nodePathList.Length;
            }
            else
            {
                nodePath = rawInjection.nodePath;
                nodePathArr = new []{nodePath};
                _count = 1;
            }
        }

        public abstract Object ToNodeObject(LuaBehaviour behav, string targetNodePath);
    }
}