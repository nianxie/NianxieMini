using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nianxie.Utils
{
    // 由于大量使用宏定义容易漏掉宏未激活部分代码中的类型错误，所以一些不涉及编译的判断改成用变量来表达
    public static class PlatformHelper
    {
        public static string GetBuildTargetString()
        {
#if UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
            return "iOS";
#elif UNITY_WEBGL
            return "WebGL";
#elif UNITY_STANDALONE_WIN
            return "StandaloneWindows64";
#elif UNITY_STANDALONE_OSX
            return "StandaloneOSX";
#else
            throw new System.Exception("GetBuildTarget TODO in this platform");
            return "TODO";
#endif
        }
        public static bool IsAndroidDevice()
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            return true;
    #else
            return false;
    #endif
        }

        public static bool IsMobileDevice()
        {
    #if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            return true;
    #else
            return false;
    #endif
        }

    #if UNITY_EDITOR
        public const bool UNITY_EDITOR = true;
    #else
        public const bool UNITY_EDITOR = false;
    #endif

    #if UNITY_STANDALONE
        public const bool UNITY_STANDALONE = true;
    #else
        public const bool UNITY_STANDALONE = false;
    #endif

    #if UNITY_WEBGL
        public const bool UNITY_WEBGL = true;
    #else
        public const bool UNITY_WEBGL = false;
    #endif

    #if UNITY_STANDALONE_OSX
        public const bool UNITY_STANDALONE_OSX = true;
    #else
        public const bool UNITY_STANDALONE_OSX = false;
    #endif

    #if UNITY_STANDALONE_WIN
        public const bool UNITY_STANDALONE_WIN = true;
    #else
        public const bool UNITY_STANDALONE_WIN = false;
    #endif

    #if UNITY_ANDROID
        public const bool UNITY_ANDROID = true;
    #else
        public const bool UNITY_ANDROID = false;
    #endif

    #if UNITY_IOS
        public const bool UNITY_IOS = true;
    #else
        public const bool UNITY_IOS = false;
    #endif

    }
}
