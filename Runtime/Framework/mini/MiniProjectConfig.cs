using System;
using System.Linq;
using System.Text;
using Nianxie.Utils;
using UnityEngine;

namespace Nianxie.Framework
{
    [Serializable]
    public class MiniProjectConfig
    {
        public static MiniProjectConfig ErrorInstance = new MiniProjectConfig(new string[]{}, null, false)
        {
            name = "(ERROR)",
        };
        public string name;
        public bool craftable;
        public string miniVersion;
        public string unityVersion;
        public string[] scripts = {};
        public string previewVideoUrl = "";

        public MiniProjectConfig(string [] scripts, string name, bool craftable)
        {
            this.name = name;
            this.craftable = craftable;
            this.scripts = scripts;
            miniVersion = NianxieConst.MINI_VERSION;
            unityVersion = Application.unityVersion;
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

        public bool CheckScriptsAndInfoMatch(string[] sortedScripts)
        {
            // check script match
            if (scripts == null)
            {
                return false;
            } 
            if (scripts.Length != sortedScripts.Length)
            {
                return false;
            } 
            if (!scripts.SequenceEqual(sortedScripts))
            {
                return false;
            } 
            // check version match
            if (miniVersion != NianxieConst.MINI_VERSION)
            {
                return false;
            } 
            if (unityVersion != Application.unityVersion)
            {
                return false;
            }
            return true;
        }

        public bool IsError()
        {
            return this == ErrorInstance;
        }

    }
}