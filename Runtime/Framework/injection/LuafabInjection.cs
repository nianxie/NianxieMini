using System.Collections.Generic;

namespace XLua
{
    public class LuafabInjection:AbstractReflectInjection
    {
        public readonly LuaTable clsOpen;
        public readonly string assetPath;
        public readonly bool lazy;
        public LuafabInjection(WarmedReflectClass cls, RawReflectInjection rawInjection) : base(cls, rawInjection)
        {
            clsOpen = rawInjection.clsOpen;
            assetPath = cls.reflectEnv.envPaths.relativePath2assetPath(rawInjection.assetPath);
            lazy = rawInjection.lazy;
        }
    }
}