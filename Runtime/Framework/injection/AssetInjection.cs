using System;
using System.Collections.Generic;
using System.Linq;

namespace XLua
{
    public class AssetInjection:AbstractMultipleInjection
    {
        public readonly string assetPath;
        public readonly string[] assetPathList;
        public AssetInjection(WarmedReflectClass cls, RawReflectInjection rawInjection, InjectionMultipleKind kind) : base(cls, rawInjection, kind)
        {
            if (rawInjection.table)
            {
                assetPathList = rawInjection.assetPathTable.Cast<string[]>().Select(a=>cls.reflectEnv.envPaths.relativePath2assetPath(a)).ToArray();
                _count = assetPathList.Length;
            }
            else
            {
                assetPath = cls.reflectEnv.envPaths.relativePath2assetPath(rawInjection.assetPath);
                _count = 1;
            }
        }
        public IEnumerable<string> EachAssetPath()
        {
            if (multipleKind == InjectionMultipleKind.Single)
            {
                yield return assetPath;
            }
            else
            {
                foreach (var a in assetPathList)
                {
                    yield return a;
                }
            }
        }
    }
}