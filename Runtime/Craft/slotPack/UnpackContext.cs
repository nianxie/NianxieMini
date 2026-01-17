using UnityEngine;
using System.Collections.Generic;
using Nianxie.Riff;
using XLua;

namespace Nianxie.Craft
{
    public class UnpackContext
    {
        private RuntimeReflectEnv env;
        private RiffPackage package;
        private Dictionary<string, Object> defaultObjectDict;
        public UnpackContext(Dictionary<string, Object> defaultPathToObject, RiffPackage riffPackage, RuntimeReflectEnv reflectEnv)
        {
            defaultObjectDict = defaultPathToObject;
            package = riffPackage;
            env = reflectEnv;
        }

        public Sprite GetSprite(int spriteIndex)
        {
            return package.sprites[spriteIndex];
        }
        public Object GetBinary(int binaryIndex)
        {
            return package.binaries[binaryIndex];
        }
        public Object GetDefault(string defaultPath)
        {
            return defaultObjectDict[defaultPath];
        }
        public LuaTable NewTable()
        {
            return env.NewTable();
        }
    }
}