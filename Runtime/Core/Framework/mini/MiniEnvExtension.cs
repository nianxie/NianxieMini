using XLua;

namespace Nianxie.Framework
{
    public class MiniEnvExtension : AbstractReflectEnv.IEnvExtension
    {
        public readonly string miniCraftLuafabPath;
        private const string MiniRoot = nameof(MiniRoot);
        private const string MiniContext = nameof(MiniContext);
        private byte[] miniBoot;

        public MiniEnvExtension(EnvPaths envPaths, byte[] miniBoot)
        {
            contextName = MiniContext;
            rootLuafabPath = $"{envPaths.luafabPathPrefix}/{MiniRoot}.prefab";
            miniCraftLuafabPath = $"{envPaths.luafabPathPrefix}/MiniCraft.prefab";
            this.miniBoot = miniBoot;
        }

        public override LuaTable OnBootstrap(AbstractReflectEnv reflectEnv)
        {
            return reflectEnv.LoadString<LuaFunction>(miniBoot, nameof(miniBoot)).Func<LuaTable>();
        }
    }
}