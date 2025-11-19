using System;
using System.Collections.Generic;

namespace XLua
{
    public class SubAssetInjection:AbstractMultipleInjection
    {
        public readonly string subName;
        private readonly string[] subNameArr;
        public ReadOnlySpan<string> subNameList => new (subNameArr);
        public readonly string assetPath;
        public readonly InjectionMultipleKind collectionKind;

        public SubAssetInjection(WarmedReflectClass cls, RawReflectInjection rawInjection, InjectionMultipleKind kind) : base(cls, rawInjection, kind)
        {
            assetPath = cls.reflectEnv.envPaths.relativePath2assetPath(rawInjection.assetPath);
            if (rawInjection.table)
            {
                subName = null;
                subNameArr = rawInjection.nodePathTable.Cast<string[]>();
                _count = subNameList.Length;
            }
            else
            {
                subName = rawInjection.nodePath;
                subNameArr = new []{subName};
                _count = 1;
            }
        }
    }
}