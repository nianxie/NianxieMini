using System;
using System.Collections.Generic;
using System.Linq;

namespace XLua
{
    public class AssetInjection:AbstractMultipleInjection
    {
        public readonly string assetPath;
        private readonly string[] assetPathArr;
        public ReadOnlySpan<string> assetPathList => new (assetPathArr);
        public AssetInjection(WarmedReflectClass cls, RawReflectInjection rawInjection, InjectionMultipleKind kind) : base(cls, rawInjection, kind)
        {
            if (rawInjection.table)
            {
                assetPath = null;
                assetPathArr = rawInjection.assetPathTable.Cast<string[]>().Select(a=>cls.reflectEnv.envPaths.relativePath2assetPath(a)).ToArray();
                _count = assetPathList.Length;
            }
            else
            {
                assetPath = cls.reflectEnv.envPaths.relativePath2assetPath(rawInjection.assetPath);
                assetPathArr = new []{assetPath};
                _count = 1;
            }
        }
    }
}