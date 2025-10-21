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
        public string name;
        public bool craftable;
        public int majorVersion;
        public int minorVersion;
        public string patchVersion;
        public string unityVersion;
        public MiniCommonConfig()
        {
        }
        public MiniCommonConfig(string _name, bool _craftable)
        {
            name = _name;
            craftable = _craftable;
            majorVersion = NianxieConst.MARJOR_VERSION;
            minorVersion = NianxieConst.MINOR_VERSION;
            patchVersion = NianxieConst.PATCH_VERSION;
            unityVersion = Application.unityVersion;
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
        public bool CheckBasicMatch(MiniCommonConfig commonConfig)
        {
            return name == commonConfig.name &&
            craftable == commonConfig.craftable;
        }
        public bool CheckVersionMatch()
        {
            return majorVersion == NianxieConst.MARJOR_VERSION &&
            minorVersion == NianxieConst.MINOR_VERSION &&
            patchVersion == NianxieConst.PATCH_VERSION &&
            unityVersion == NianxieConst.UNITY_VERSION;
        }
    }

    [Serializable]
    public class MiniProjectConfig:MiniCommonConfig
    {
        public static MiniProjectConfig ErrorInstance = new MiniProjectConfig(new string[]{}, null, false)
        {
            name = "(ERROR)",
        };
        public string[] scripts = {};

        public MiniProjectConfig(string [] scripts, string name, bool craftable):base(name, craftable)
        {
            this.scripts = scripts;
        }
        public MiniProjectConfig(MiniProjectConfig projectConfig):base(projectConfig)
        {
            scripts = projectConfig.scripts;
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