using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEngine;

namespace XLua
{
    public class RuntimeReflectEnv:AbstractReflectEnv
    {
        public static RuntimeReflectEnv Create(AbstractGameManager gameManager, EnvPaths envPaths, byte[] miniBoot)
        {
            var env = new RuntimeReflectEnv(gameManager, envPaths);
            env.Bootstrap(miniBoot);
            env.Warmup();
            return env;
        }
        
        private readonly AssetModule assetModule;
        private readonly TaskModule baseHelper;
        
        private RuntimeReflectEnv(AbstractGameManager gameManager, EnvPaths vEnvPaths) : base(vEnvPaths)
        {
            assetModule = gameManager.assetModule;
            baseHelper = gameManager.baseHelper;
        }

        public LuaTable CreateContext()
        {
            return contextNew.Func<TaskModule, LuaTable>(baseHelper);
        }

        protected override WarmedReflectClass FallbackReflect(string clsPath, string[] nestedPath, string message)
        {
            return null;
        }

        public override IReadOnlyDictionary<string, TextAsset> scriptAssetDict => assetModule.GetScriptAssetDict();

        public void WrapLuaTaskOut<T>(UniTask<T> uniTask, out LuaTable outTask)
        {
            var ltask = boot.task.Func<TaskModule, LuaTable>(baseHelper);
            outTask = ltask;
            UniTask.Create(async () =>
            {
                try
                {
                    var ret = await uniTask;
                    boot.complete.Action(ltask, true, ret);
                }
                catch (Exception e)
                {
                    boot.complete.Action(ltask, false, $"{e.GetType()}:{e.Message}:{e.StackTrace}");
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

        public LuaFunction bootTask => boot.task;
        public LuaFunction bootSleep => boot.sleep;
    }
}