using UnityEngine;
using System.Collections.Generic;
using Nianxie.Riff;
using XLua;

namespace Nianxie.Craft
{
    public class UnpackContext
    {
        private RuntimeReflectEnv env;
        private AssetUsageCenter assetUsageCenter;
        public UnpackContext(AssetUsageCenter usageCenter, RuntimeReflectEnv reflectEnv)
        {
            assetUsageCenter = usageCenter;
            env = reflectEnv;
        }

        public TextureUsage GetTextureUsage(string builtinPath, int riffIndex)
        {
            if (!string.IsNullOrEmpty(builtinPath))
            {
                return assetUsageCenter.GetBuiltinTextureUsage(builtinPath);
            }
            else
            {
                return assetUsageCenter.GetRiffTextureUsage(riffIndex);
            }
        }
        public LuaTable NewTable()
        {
            return env.NewTable();
        }
    }
}