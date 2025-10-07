using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nianxie.Framework;
using UnityEngine;
using XLua.LuaDLL;

namespace XLua
{
    public class RuntimeReflectEnv:AbstractReflectEnv
    {
        public static RuntimeReflectEnv Create(AbstractGameManager gameManager, EnvPaths envPaths, byte[] miniBoot)
        {
            var env = new RuntimeReflectEnv(gameManager, envPaths);
            env.Warmup();
            env.Bootstrap(miniBoot);
            return env;
        }
        
        private readonly AssetModule assetModule;
        private readonly AsyncHelper baseHelper;
        public BootTable boot { get; private set; }
        
        private RuntimeReflectEnv(AbstractGameManager gameManager, EnvPaths vEnvPaths) : base(vEnvPaths)
        {
            assetModule = gameManager.assetModule;
            baseHelper = gameManager.baseHelper;
        }

        public LuaTable CreateContext()
        {
            return contextNew.Func<AsyncHelper, LuaTable>(baseHelper);
        }
        
        /// <summary>
        /// 启动：加载boot，mini模式下加载miniBoot，shell模式下require boot.boot
        /// </summary>
        protected void Bootstrap(byte[] miniBoot) 
        {
            if (boot != null)
            {
                throw new Exception("ReflectEnv.Bootstrap called than once");
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
            boot.InitHelper.Action(baseHelper);
        }

        protected override WarmedReflectClass FallbackReflect(string clsPath, string[] nestedPath, string message)
        {
            return null;
        }

        public override IReadOnlyDictionary<string, TextAsset> scriptAssetDict => assetModule.GetScriptAssetDict();

        public void WrapLuaTaskOut<T>(UniTask<T> uniTask, out LuaTable outTask)
        {
            var ltask = boot.NewFuture.Func<LuaTable>();
            outTask = ltask;
            UniTask.Create(async () =>
            {
                try
                {
                    var ret = await uniTask;
                    boot.CompleteFuture.Action(ltask, true, ret);
                }
                catch (Exception e)
                {
                    boot.CompleteFuture.Action(ltask, false, $"{e.GetType()}:{e.Message}:{e.StackTrace}");
                }
            }).Forget();
        }

        public LuaTable WrapLuaTask<T>(UniTask<T> uniTask)
        {
            WrapLuaTaskOut(uniTask, out var ltask);
            return ltask;
        }

        public LuaTable RapidjsonDecode(byte[] data)
        {
            return boot.rapidjsonDecode.Func<byte[], LuaTable>(data);
        }

        public void BindMeta(LuaTable self, WarmedReflectClass warmedReflect)
        {
            luaSetmetatable.Action(self, warmedReflect.meta);
        }

        public void Repl(string script)
        {
            boot.Repl.Action(script??"");
        }

        private lua_CSFunction SafeAsyncBegin<TSelf>(TSelf self, Func<IntPtr, int> asyncEnd)
        {
            return (IntPtr asyncL) =>
            {
                try
                {
                    var first = translator.GetObject(asyncL, 1, typeof(TSelf));
                    UnityEngine.Assertions.Assert.IsTrue(first == (object)self, $"self {first} {self} not match when future invoke");
                    return asyncEnd(asyncL);
                }
                catch (Exception e)
                {
                    return Lua.luaL_error(asyncL, "c# exception:" + e);
                }
            };
        }
        
        private int SafeAsyncEndResult<T>(IntPtr asyncL, UniTask<T> resultFn)
        {
            boot.NewFuture.push(asyncL);
            var err = Lua.lua_pcall(asyncL, 0, 1, 0);
            if (err != 0)
            {
                var errMsg = Lua.lua_tostring(asyncL, -1);
                return Lua.luaL_error(asyncL, $"Future.new call failed: {errMsg}");
            }
            Lua.lua_pushvalue(asyncL, -1);
            var future = new LuaTable(Lua.luaL_ref(asyncL), this);
            UniTask.Create(async () =>
            {
                try
                {
                    var ret = await resultFn;
                    boot.CompleteFuture.Action(future, true, ret);
                }
                catch (Exception e)
                {
                    boot.CompleteFuture.Action(future, false, $"{e.GetType()}:{e.Message}:{e.StackTrace}");
                }
            }).Forget();
            return 1;
        }
        
        private int SafeAsyncEndVoid(IntPtr asyncL, UniTask voidFn)
        {
            boot.NewFuture.push(asyncL);
            var err = Lua.lua_pcall(asyncL, 0, 1, 0);
            if (err != 0)
            {
                return Lua.luaL_error(asyncL, "Future.new call failed");
            }
            Lua.lua_pushvalue(asyncL, -1);
            var future = new LuaTable(Lua.luaL_ref(asyncL), this);
            UniTask.Create(async () =>
            {
                try
                {
                    await voidFn;
                    boot.CompleteFuture.Action(future, true);
                }
                catch (Exception e)
                {
                    boot.CompleteFuture.Action(future, false, $"{e.GetType()}:{e.Message}:{e.StackTrace}");
                }
            }).Forget();
            return 1;
        }

        public lua_CSFunction AsyncAction<T1, T2>(T1 self, Func<T2, UniTask> action)
        {
            return SafeAsyncBegin(self, (asyncL) =>
            {
                translator.Get(asyncL, 2, out T2 arg2);
                return SafeAsyncEndVoid(asyncL, action(arg2));
            });
        }
        
        public lua_CSFunction AsyncFunc<T1, T2, TResult>(T1 self, Func<T2, UniTask<TResult>> func)
        {
            return SafeAsyncBegin(self, (asyncL) =>
            {
                translator.Get(asyncL, 2, out T2 arg2);
                return SafeAsyncEndResult(asyncL, func(arg2));
            });
        }
        public lua_CSFunction AsyncFunc<T1, T2, T3, TResult>(T1 self, Func<T2, T3, UniTask<TResult>> func)
        {
            return SafeAsyncBegin(self, (asyncL) =>
            {
                translator.Get(asyncL, 2, out T2 arg2);
                translator.Get(asyncL, 3, out T3 arg3);
                return SafeAsyncEndResult(asyncL, func(arg2, arg3));
            });
        }
        public lua_CSFunction AsyncFunc<T1, T2, T3, T4, TResult>(T1 self, Func<T2, T3, T4, UniTask<TResult>> func)
        {
            return SafeAsyncBegin(self, (asyncL) =>
            {
                translator.Get(asyncL, 2, out T2 arg2);
                translator.Get(asyncL, 3, out T3 arg3);
                translator.Get(asyncL, 4, out T4 arg4);
                return SafeAsyncEndResult(asyncL, func(arg2, arg3, arg4));
            });
        }
        public LuaFunction bootSleep => boot.Sleep;
    }
}