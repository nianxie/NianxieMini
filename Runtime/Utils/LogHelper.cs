using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nianxie.Utils
{
    public class LogHelper 
    {
        private static void LogCore(LogType logType, object s)
        {
            var msg = $"{s}";
    #if !UNITY_EDITOR
            var time = System.DateTime.Now.ToString("HH:mm:ss");
            msg = $"[{time}] {s}";
    #endif
            Debug.unityLogger.Log(logType, s);
        }

        public static void Log(object s)
        {
            LogCore(LogType.Log, s);
        }

        public static void LogError(object s)
        {
            LogCore(LogType.Error, s);
        }
        
        public static void LogWarning(object s)
        {
            LogCore(LogType.Warning, s);
        }
    }
}
