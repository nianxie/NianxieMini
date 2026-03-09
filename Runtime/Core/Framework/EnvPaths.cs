using System;
using System.Collections.Generic;
using System.IO;
using Nianxie.Utils;

namespace Nianxie.Framework
{
    public class EnvPaths
    {
        public static EnvPaths RuntimeEnvPaths(string pathPrefix)
        {
            return new EnvPaths(pathPrefix);
        }
        public static string miniFolder2pathPrefix(string miniFolder)
        {
            return $"{NianxieConst.MiniPrefixPath}/{miniFolder}";
        }

        public static readonly string[] NESTED_KEYS_EMPTY = {};
        private const string LUAFAB = "luafab";
        public const string LUAFAB_SLASH = "luafab/";
        public const string SRC = "src";
        public static string[] SCRIPT_EXTS = {".lua", ".thlua"};
        
        public readonly string pathPrefix;
        public readonly string luafabPathPrefix;
        public readonly string srcPathPrefix;
        
        // config.txt
        public readonly string miniProjectConfig;

        protected EnvPaths(string vPrefix)
        {
            pathPrefix = vPrefix;
            luafabPathPrefix = $"{pathPrefix}/{LUAFAB}";
            srcPathPrefix = $"{pathPrefix}/{SRC}";
            // mini config
            miniProjectConfig = $"{pathPrefix}/{NianxieConst.ConfigTxt}";
        }

        /// <summary>
        /// aaa.bbb -> {prefix}/aaa/bbb.prefab
        /// </summary>
        public string classPath2luafabPath(string classPath)
        {
            var luafabPath = classPath.Replace(".", "/");
            return $"{pathPrefix}/{luafabPath}.prefab";
        }

        /// <summary>
        /// {prefix}/aaa/bbb.lua -> aaa.bbb
        /// </summary>
        public string assetPath2classPath(string assetPath)
        {
            var extension = Path.GetExtension(assetPath);
            var relativePathNoExt = assetPath.Substring(pathPrefix.Length + 1, assetPath.Length - extension.Length - pathPrefix.Length - 1);
            return relativePathNoExt.Replace("/", ".");
        }
        
        /// <summary>
        /// {prefix}/aaa/bbb.lua -> aaa/bbb.lua
        /// </summary>
        public string assetPath2relativePath(string assetPath)
        {
            if (assetPath.StartsWith(pathPrefix))
            {
                return assetPath.Substring(pathPrefix.Length + 1, assetPath.Length - pathPrefix.Length - 1);
            }
            throw new Exception($"invalid asset path:{assetPath} when convert to relative path");
        }
        
        /// <summary>
        ///  aaa/bbb.lua -> {prefix}/aaa/bbb.lua
        /// </summary>
        public string relativePath2assetPath(string assetPath)
        {
            return $"{pathPrefix}/{assetPath}";
        }
    }
}