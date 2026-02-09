using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Nianxie.Framework;
using XLua.LuaDLL;
using LuaAPI = XLua.LuaDLL.Lua;

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
        protected BootTable boot { get; private set; }

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
        /// 启动：加载boot，mini模式下加载miniBoot，shell模式下require boot.boot
        /// </summary>
        protected virtual void Bootstrap(byte[] miniBoot) 
        {
            if (boot != null)
            {
                throw new Exception("ReflectEnv.Bootstrap called more than once");
            }
            if (miniBoot == null)
            {
                // miniBoot 为空时为shell的ReflectEnv
                boot = new BootTable(RequireTable("boot.boot"));
            }
            else
            {
                // miniBoot 不为空时为mini的ReflectEnv
                var miniBootTable = LoadString<LuaFunction>(miniBoot, nameof(miniBoot)).Func<LuaTable>();
                boot = new BootTable(miniBootTable);
            }
            translator.Push(rawL, BuildPrintFunc(boot, LogType.Log));
            if (0 != LuaAPI.xlua_setglobal(rawL, "print"))
            {
                throw new Exception("call xlua_setglobal fail!");
            }
            translator.Push(rawL, BuildPrintFunc(boot, LogType.Warning));
            if (0 != LuaAPI.xlua_setglobal(rawL, "printwarn"))
            {
                throw new Exception("call xlua_setglobal fail!");
            }
            translator.Push(rawL, BuildPrintFunc(boot, LogType.Error));
            if (0 != LuaAPI.xlua_setglobal(rawL, "printerror"))
            {
                throw new Exception("call xlua_setglobal fail!");
            }
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
        
        private static StringBuilder _sbCache = new StringBuilder(1024);
        private static lua_CSFunction BuildPrintFunc(BootTable boot, UnityEngine.LogType logType)
        {
            return (IntPtr invokeL) =>
            {
                try
                {
                    int n = LuaAPI.lua_gettop(invokeL);
                    _sbCache.Clear();
    #if UNITY_EDITOR // TODO 支持手机端debug模式
                    // := local currentline = boot.CurrentLine(3)
                    boot.FileLine.push(invokeL);
                    LuaAPI.xlua_pushinteger(invokeL, 3);
                    var err = LuaAPI.lua_pcall(invokeL, 1, 2, 0);
                    if (err != 0)
                    {
                        var errMsg = LuaAPI.lua_tostring(invokeL, -1);
                        return LuaAPI.luaL_error(invokeL, $"Future.new call failed: {errMsg}");
                    }
                    _sbCache.Append('(');
                    _sbCache.Append(Path.GetFileName(LuaAPI.lua_tostring(invokeL, -2)));
                    _sbCache.Append(':');
                    _sbCache.Append(LuaAPI.xlua_tointeger(invokeL, -1));
                    _sbCache.Append(") ");
                    LuaAPI.lua_settop(invokeL, n);  /* recover stack */
    #endif

                    if (0 != LuaAPI.xlua_getglobal(invokeL, "tostring"))
                    {
                        return LuaAPI.luaL_error(invokeL, "can not get tostring in print:");
                    }

                    for (int i = 1; i <= n; i++)
                    {
                        LuaAPI.lua_pushvalue(invokeL, -1);  /* function to be called */
                        LuaAPI.lua_pushvalue(invokeL, i);   /* value to print */
                        if (0 != LuaAPI.lua_pcall(invokeL, 1, 1, 0))
                        {
                            return LuaAPI.lua_error(invokeL);
                        }
                        _sbCache.Append(LuaAPI.lua_tostring(invokeL, -1));

                        if (i != n) _sbCache.Append('\t');

                        LuaAPI.lua_pop(invokeL, 1);  /* pop result */
                    }

    #if UNITY_EDITOR // TODO 支持手机端debug模式
                    // push stack info
                    _sbCache.AppendLine();
                    LuaAPI.luaL_traceback(invokeL, invokeL, IntPtr.Zero, 1);
                    _sbCache.Append(LuaAPI.lua_tostring(invokeL, -1));
                    LuaAPI.lua_pop(invokeL, 1);  /* pop result */
    #endif

                    UnityEngine.Debug.unityLogger.Log(logType, _sbCache.ToString());
                    return 0;
                }
                catch (System.Exception e)
                {
                    return LuaAPI.luaL_error(invokeL, "c# exception in print:" + e);
                }
            };
        }
    }
}
