using System.Collections.Generic;

namespace XLua
{
    public class SubAssetInjection:AbstractMultipleInjection
    {
        public readonly string subName;
        public readonly string[] subNameList;
        public readonly string assetPath;
        public readonly InjectionMultipleKind collectionKind;

        public SubAssetInjection(WarmedReflectClass cls, RawReflectInjection rawInjection, InjectionMultipleKind kind) : base(cls, rawInjection, kind)
        {
            assetPath = cls.reflectEnv.envPaths.relativePath2assetPath(rawInjection.assetPath);
            if (rawInjection.table)
            {
                subNameList = rawInjection.nodePathTable.Cast<string[]>();
                _count = subNameList.Length;
            }
            else
            {
                subName = rawInjection.nodePath;
                _count = 1;
            }
        }
        public IEnumerable<string> EachSubName()
        {
            if (multipleKind == InjectionMultipleKind.Single)
            {
                yield return subName;
            }
            else
            {
                foreach (var a in subNameList)
                {
                    yield return a;
                }
            }
        }
    }
}