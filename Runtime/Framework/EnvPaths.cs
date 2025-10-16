using System;
using System.Collections.Generic;
using System.IO;
using Nianxie.Utils;

namespace Nianxie.Framework
{
    public class EnvPaths
    {
        private const string ShellRoot = nameof(ShellRoot);
        private const string ShellContext = nameof(ShellContext);
        private const string MiniRoot = nameof(MiniRoot);
        private const string MiniContext = nameof(MiniContext);
        public static readonly EnvPaths ShellEnvPaths = new EnvPaths();

        public static EnvPaths MiniEnvPaths(string folder)
        {
            return new EnvPaths(folder);
        }

        public static readonly string[] NESTED_KEYS_EMPTY = {};
        private const string LUAFAB = "luafab";
        public const string LUAFAB_SLASH = "luafab/";
        public const string SRC = "src";
        public static string[] SCRIPT_EXTS = {".lua", ".thlua"};
        
        public readonly string pathPrefix;
        public readonly string luafabPathPrefix;
        public readonly string srcPathPrefix;
        
        // ShellContext, MiniContext
        public readonly string contextName;
        
        // ShellRoot, MiniRoot
        public readonly string rootLuafabPath;
        
        // MiniCraft
        public readonly string miniCraftLuafabPath;
        
        // config.txt
        public readonly string miniProjectConfig;

        protected EnvPaths(string vPrefix, string vContextName, string vRootLuafabPath)
        {
            pathPrefix = vPrefix;
            contextName = vContextName;
            luafabPathPrefix = $"{pathPrefix}/{LUAFAB}";
            srcPathPrefix = $"{pathPrefix}/{SRC}";
            rootLuafabPath = $"{luafabPathPrefix}/{vRootLuafabPath}.prefab";
        }

        // shell envPaths constructor
        protected EnvPaths():this($"{NianxieConst.ShellResPath}", ShellContext, ShellRoot)
        {
            // shell env path
            miniCraftLuafabPath = null;
            miniProjectConfig = null;
        }

        // mini envPaths constructor
        protected EnvPaths(string folder):this($"{NianxieConst.MiniPrefixPath}/{folder}", MiniContext, MiniRoot)
        {
            // mini env path
            miniCraftLuafabPath = $"{luafabPathPrefix}/MiniCraft.prefab";
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