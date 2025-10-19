using System;
using System.Linq;
using System.Text;
using Nianxie.Utils;
using UnityEngine;

namespace Nianxie.Framework
{
    [Serializable]
    public class MiniCommonConfig
    {
        public string name = "";
        public bool craftable = false;
        public int majorVersion = NianxieConst.MINOR_VERSION;
        public int minorVersion = NianxieConst.MARJOR_VERSION;
        public string patchVersion = NianxieConst.PATCH_VERSION;
        public string unityVersion = Application.unityVersion;
        public MiniCommonConfig()
        {
        }
        public MiniCommonConfig(MiniCommonConfig commonConfig)
        {
            name = commonConfig.name;
            craftable = commonConfig.craftable;
            majorVersion = commonConfig.majorVersion;
            minorVersion = commonConfig.minorVersion;
            patchVersion = commonConfig.patchVersion;
            unityVersion = commonConfig.unityVersion;
        }
        public bool Match(MiniCommonConfig commonConfig)
        {
            return name == commonConfig.name &&
            craftable == commonConfig.craftable &&
            majorVersion == commonConfig.majorVersion &&
            minorVersion == commonConfig.minorVersion &&
            patchVersion == commonConfig.patchVersion &&
            unityVersion == commonConfig.unityVersion;
        }
    }

    [Serializable]
    public class MiniProjectConfig:MiniCommonConfig
    {
        public static MiniProjectConfig ErrorInstance = new MiniProjectConfig
        {
            name = "(ERROR)",
        };
        public string[] scripts = {};

        public MiniProjectConfig()
        {
        }
        public MiniProjectConfig(MiniCommonConfig commonConfig):base(commonConfig)
        {
        }

        public static MiniProjectConfig FromJson(byte[] jsonBytes)
        {
            var jsonStr = Encoding.UTF8.GetString(jsonBytes);
            return JsonUtility.FromJson<MiniProjectConfig>(jsonStr);
        }
        public byte[] ToJson()
        {
            var jsonStr = JsonUtility.ToJson(this, true);
            return Encoding.UTF8.GetBytes(jsonStr);
        }

        public bool CheckScriptsMatch(string[] sortedScripts)
        {
            if (scripts == null)
            {
                return false;
            }

            if (scripts.Length != sortedScripts.Length)
            {
                return false;
            }

            return scripts.SequenceEqual(sortedScripts);
        }

        public bool IsError()
        {
            return this == ErrorInstance;
        }

    }
}