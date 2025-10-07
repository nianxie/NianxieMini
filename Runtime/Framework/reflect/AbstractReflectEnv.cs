using System;
using UnityEngine;
using System.Collections.Generic;
using Nianxie.Framework;

namespace XLua
{
    [BlackList]
    public abstract class AbstractReflectEnv : LuaEnv
    {
        public TextAsset searchTextAssetForRequire(ref string strPath)
        {
            strPath = strPath.Replace('.', '/');
            if (!strPath.StartsWith(EnvPaths.LUAFAB_SLASH))
            {
                strPath = $"{EnvPaths.SRC}/{strPath}";
            }

            strPath = envPaths.pathPrefix + "/" + strPath;

            foreach (var ext in EnvPaths.SCRIPT_EXTS)
            {
                var strFile = strPath + ext;
                if(!scriptAssetDict.TryGetValue(strFile, out var asset))
                {
                    continue;
                }
                strPath = strFile;
                //extFound = ext;
                return asset;
            }

            //extFound = null;
            return null;
        }

        private byte[] FileLoader(ref string strPath)
        {
            if (strPath.IndexOf('/') >= 0)
            {
                Debug.LogError($"don't use '/' in require : {strPath}");
            }

            var asset = searchTextAssetForRequire(ref strPath);
            if (asset != null)
            {
                return asset.bytes;
            }

            // throw new Exception($"build load lua TODO path = {strPath}");
            return null;
        }

        public readonly EnvPaths envPaths;
        protected readonly LuaFunction luaRequire = null;
        protected readonly LuaFunction luaRawequal = null;
        protected readonly LuaFunction luaSetmetatable = null;
        protected readonly Dictionary<string, WarmedReflectClass> fileWarmedReflectDict = new();
        private readonly LuaTable fileClsOpenSet;
        protected LuaFunction contextNew { get; private set; }

        // editor模式下用来在inspector上显示lua层定义的属性，runtime模式下会挂在LuaModule上
        protected AbstractReflectEnv(EnvPaths vEnvPaths)
        {
            AddBuildin("rapidjson", XLua.LuaDLL.Lua.LoadRapidJson);
            envPaths = vEnvPaths;
            AddLoader(FileLoader);
            luaRequire = Global.Get<string, LuaFunction>("require");
            luaRawequal = Global.Get<string, LuaFunction>("rawequal");
            luaSetmetatable = Global.Get<string, LuaFunction>("setmetatable");
            fileClsOpenSet = NewTable();
        }

        public abstract IReadOnlyDictionary<string, TextAsset> scriptAssetDict { get; }

        public bool IsFileClass(LuaTable clsOpen)
        {
            return fileClsOpenSet.ContainsKey(clsOpen);
        }

        /// <summary>
        /// 预热：将prefab路径下的所有lua脚本require进来，构建reflect信息
        /// </summary>
        protected void Warmup()
        {
            contextNew = RequireFunction(envPaths.contextName);
            var okayPairList = new List<(string, LuaTable)>();
            var errPairList = new List<(string, string)>();
            foreach (var luaAssetPath in scriptAssetDict.Keys)
            {
                if (luaAssetPath.StartsWith(envPaths.luafabPathPrefix))
                {
                    var clsPath = envPaths.assetPath2classPath(luaAssetPath);
                    try
                    {
                        LuaTable clsOpen = RequireTable(clsPath);
                        if (clsOpen != null)
                        {
                            fileClsOpenSet.Set(clsOpen, clsPath);
                            okayPairList.Add((clsPath, clsOpen));
                        }
                        else
                        {
                            var message = $"require '{clsPath}' but got non-table return when warmup";
                            Debug.LogError(message);
                            errPairList.Add((clsPath, message));
                        }
                    }
                    catch (Exception e)
                    {
                        var message = $"require '{clsPath}' error when warmup, {e}";
                        Debug.LogError(message);
                        errPairList.Add((clsPath, message));
                    }
                }
            }
            foreach (var (clsPath, errMsg) in errPairList)
            {
                fileWarmedReflectDict[clsPath] = FallbackReflect(clsPath, EnvPaths.NESTED_KEYS_EMPTY, errMsg);
            }
            foreach (var (clsPath, clsOpen) in okayPairList)
            {
                try
                {
                    fileWarmedReflectDict[clsPath] = WarmedReflectClass.Create(this, clsOpen, clsPath, EnvPaths.NESTED_KEYS_EMPTY);
                }
                catch (Exception e)
                {
                    var message = $"reflect '{clsPath}' error when warmup, {e}";
                    Debug.LogError(message);
                    fileWarmedReflectDict[clsPath] = FallbackReflect(clsPath, EnvPaths.NESTED_KEYS_EMPTY, message);
                }
            }
        }
        

        protected abstract WarmedReflectClass FallbackReflect(string clsPath, string[] nestedPath, string message);

        public WarmedReflectClass GetFileWarmedReflect(string clsPath)
        {
            if (!fileWarmedReflectDict.TryGetValue(clsPath, out var ret))
            {
                return FallbackReflect(clsPath, EnvPaths.NESTED_KEYS_EMPTY, "path not exist");
            }
            return ret;
        }

        public WarmedReflectClass GetWarmedReflect(string clsPath, string[] nestedPaths)
        {
            if (!fileWarmedReflectDict.TryGetValue(clsPath, out var warmedReflect))
            {
                return FallbackReflect(clsPath, EnvPaths.NESTED_KEYS_EMPTY, "path not exist");
            }
            foreach (var nestedPath in nestedPaths)
            {
                if (!warmedReflect.TryNestGet(nestedPath, out warmedReflect))
                {
                    return FallbackReflect(clsPath, nestedPaths, "path not exist");
                }
            }
            return warmedReflect;
        }

        protected LuaTable RequireTable(string module)
        {
            return luaRequire.Func<string, LuaTable>(module);
        }
        private LuaFunction RequireFunction(string module)
        {
            return luaRequire.Func<string, LuaFunction>(module);
        }
    }
}
