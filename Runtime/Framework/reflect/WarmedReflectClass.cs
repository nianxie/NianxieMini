using System;
using System.Collections.Generic;
using System.Linq;
using Nianxie.Components;
using Nianxie.Framework;
using UnityEngine;

namespace XLua
{
    public class RawReflectClass
    {
        public LuaTable meta;
    }
    public class RawReflectMeta
    {
        public RawReflectInjection[] __injections;
        public Dictionary<string, LuaFunction> __index;
    }

    public class WarmedReflectClass
    {
        public string whichClass => $"{classPath}-[{string.Join(",", nestedKeys)}]";
        public readonly AbstractReflectEnv reflectEnv;
        public readonly string classPath;
        public readonly string[] nestedKeys;
        public AbstractReflectInjection[] injections { get; protected set; }
        public PartVtbl[] subVtbls { get; protected set; }
        public MiniVtbl miniVtbl { get; protected set; }
        public LuaTable clsMeta { get; protected set; }
        public LuaTable clsOpen { get; protected set; }

        //public readonly WarmedReflectClass ancestor;
        public readonly Dictionary<string, ScriptInjection> nestedInjectionDict = new();
        public IEnumerable<ScriptInjection> eachNestedInjection => nestedInjectionDict.Values;
        public AbstractNodeInjection[] nodeInjections { get; protected set; }

        // 不使用构造函数，使用WarmedReflectClass.Create
        protected WarmedReflectClass(AbstractReflectEnv env, string path, string[] keys)
        {
            reflectEnv = env;
            classPath = path;
            nestedKeys = keys;
        }


        public bool TryNestGet(string nestedKey, out WarmedReflectClass warmedReflect)
        {
            if (nestedInjectionDict.TryGetValue(nestedKey, out var injection))
            {
                warmedReflect = injection.nestedClass;
                return true;
            }
            else
            {
                warmedReflect = null;
                return false;
            }
        }
        public void CollectReference(HashSet<string> collection)
        {
            foreach (var injection in injections)
            {
                if (injection is LuafabInjection luafabInjection)
                {
                    collection.Add(luafabInjection.assetPath);
                } else if (injection is AssetInjection assetInjection)
                {
                    foreach (var assetPath in assetInjection.EachAssetPath())
                    {
                        collection.Add(assetPath);
                    }
                } else if (injection is SubAssetInjection subAssetInjection)
                {
                    collection.Add(subAssetInjection.assetPath);
                }
            }
            foreach (var nestedInjection in nestedInjectionDict.Values)
            {
                nestedInjection.nestedClass.CollectReference(collection);
            }
        }
        public static WarmedReflectClass Create(AbstractReflectEnv env, LuaTable clsOpen, string classPath, string[] nestedKeys)
        {
            var rawReflectClass = clsOpen.Cast<RawReflectClass>();
            var rawReflectMeta = rawReflectClass.meta.Cast<RawReflectMeta>();
            var reflectClass = new WarmedReflectClass(env, classPath, nestedKeys)
            {
                clsOpen = clsOpen,
                clsMeta = rawReflectClass.meta,
                subVtbls = PartVtbl.CreateSubArrayFromVtbl(rawReflectMeta.__index),
                miniVtbl = PartVtbl.CreateMiniVtbl(rawReflectMeta.__index),
            };
            var injections = rawReflectMeta.__injections.Select(r=>r.Create(reflectClass, nestedKeys)).ToArray();
            reflectClass.injections = injections;
            reflectClass.nodeInjections = injections.OfType<AbstractNodeInjection>().ToArray();
            foreach (var injection in reflectClass.injections)
            {
                if (injection is ScriptInjection scriptInjection && scriptInjection.isNested)
                {
                    reflectClass.nestedInjectionDict[injection.key] = scriptInjection;
                }
            }
            return reflectClass;
        }
    }

    public class ErrorReflectClass:WarmedReflectClass
    {
        public string message;
        public ErrorReflectClass(AbstractReflectEnv env, string path, string[] keys, string message) : base(env, path, keys)
        {
            injections = new AbstractReflectInjection[] {};
            subVtbls = new PartVtbl[] { };
            miniVtbl = new MiniVtbl();
            nodeInjections = new AbstractNodeInjection[]{};
            this.message = message;
        }
    }
}