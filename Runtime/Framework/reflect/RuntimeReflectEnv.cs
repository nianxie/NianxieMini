using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nianxie.Framework;
using UnityEngine;
using XLua.LuaDLL;

namespace XLua
{
    public class RuntimeReflectEnv : AbstractReflectEnv
    {
        public static RuntimeReflectEnv Create(AbstractGameManager gameManager, EnvPaths envPaths, byte[] miniBoot)
        {
            var env = new RuntimeReflectEnv(gameManager, envPaths);
            env.Bootstrap(miniBoot);
            env.Warmup();
            return env;
        }

        private readonly AssetModule assetModule;
        private readonly AsyncHelper asyncHelper;

        private RuntimeReflectEnv(AbstractGameManager gameManager, EnvPaths vEnvPaths) : base(vEnvPaths)
        {
            assetModule = gameManager.assetModule;
            asyncHelper = gameManager.baseHelper;
        }

        protected override void Bootstrap(byte[] miniBoot)
        {
            base.Bootstrap(miniBoot);
            boot.InitHelper.Action(asyncHelper);
        }

        public LuaTable CreateContext()
        {
            return contextNew.Func<AsyncHelper, LuaTable>(asyncHelper);
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

        [Obsolete("TODO Use Future")]
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
            luaSetmetatable.Action(self, warmedReflect.clsMeta);
        }

        public void Repl(string script)
        {
            boot.Repl.Action(script ?? "");
        }

        public lua_CSFunction AsyncNotImplement(object self, string name)
        {
            return (asyncL)=>
            {
                var err = $"{name} for {self.GetType().Name} not implement";
                return Lua.luaL_error(asyncL, err);
            };
        }

        private lua_CSFunction SafeAsyncBegin(object self, Func<IntPtr, int> asyncEnd)
        {
            return (IntPtr asyncL) =>
            {
                try
                {
                    var first = translator.GetObject(asyncL, 1, self.GetType());
                    UnityEngine.Assertions.Assert.IsTrue(first == self, $"self {first} {self} not match when future invoke");
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
        
        public lua_CSFunction AsyncAction(object self, Func<UniTask> action)
        {
            return SafeAsyncBegin(self, (asyncL) =>
            {
                return SafeAsyncEndVoid(asyncL, action());
            });
        }

        public lua_CSFunction AsyncAction<T2>(object self, Func<T2, UniTask> action)
        {
            return SafeAsyncBegin(self, (asyncL) =>
            {
                translator.Get(asyncL, 2, out T2 arg2);
                return SafeAsyncEndVoid(asyncL, action(arg2));
            });
        }
        
        public lua_CSFunction AsyncAction<T2, T3>(object self, Func<T2, T3, UniTask> action)
        {
            return SafeAsyncBegin(self, (asyncL) =>
            {
                translator.Get(asyncL, 2, out T2 arg2);
                translator.Get(asyncL, 3, out T3 arg3);
                return SafeAsyncEndVoid(asyncL, action(arg2, arg3));
            });
        }
        
        public lua_CSFunction AsyncFunc<TResult>(object self, Func<UniTask<TResult>> func)
        {
            return SafeAsyncBegin(self, (asyncL) =>
            {
                return SafeAsyncEndResult(asyncL, func());
            });
        }
        public lua_CSFunction AsyncFunc<T2, TResult>(object self, Func<T2, UniTask<TResult>> func)
        {
            return SafeAsyncBegin(self, (asyncL) =>
            {
                translator.Get(asyncL, 2, out T2 arg2);
                return SafeAsyncEndResult(asyncL, func(arg2));
            });
        }
        public lua_CSFunction AsyncFunc<T2, T3, TResult>(object self, Func<T2, T3, UniTask<TResult>> func)
        {
            return SafeAsyncBegin(self, (asyncL) =>
            {
                translator.Get(asyncL, 2, out T2 arg2);
                translator.Get(asyncL, 3, out T3 arg3);
                return SafeAsyncEndResult(asyncL, func(arg2, arg3));
            });
        }
        public lua_CSFunction AsyncFunc<T2, T3, T4, TResult>(object self, Func<T2, T3, T4, UniTask<TResult>> func)
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
        public LuaFunction bootNewFuture => boot.NewFuture;
    }
}