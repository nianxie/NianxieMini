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
        public static MiniProjectConfig ErrorInstance = new MiniProjectConfig
        {
            name = "ERROR",
        };
        public string[] scripts = {};
        public string miniId = "";
        public string name = "";
        public int version = NianxieConst.MINI_VERSION;
        public bool craft = false;
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
            return scripts != null && scripts.Length == sortedScripts.Length &&
                Enumerable.Range(0, sortedScripts.Length).All(i => scripts[i] == sortedScripts[i]);
        }

        public bool IsError()
        {
            return this == ErrorInstance;
        }
    }
}