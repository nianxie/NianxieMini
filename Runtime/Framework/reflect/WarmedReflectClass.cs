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
        public RawReflectInjection[] injections;
        public Dictionary<string, LuaFunction> vtbl;
        public LuaTable meta;
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
        public LuaTable meta => rawReflect.meta;
        public LuaTable clsOpen { get; protected set; }
        private RawReflectClass rawReflect;

        //public readonly WarmedReflectClass ancestor;
        public readonly Dictionary<string, ScriptInjection> nestedInjectionDict = new();
        public IEnumerable<ScriptInjection> eachNestedInjection => nestedInjectionDict.Values;
        public AbstractNodeInjection[] nodeInjections { get; private set; }

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
            var rawReflect = clsOpen.Cast<RawReflectClass>();
            var reflectClass = new WarmedReflectClass(env, classPath, nestedKeys)
            {
                clsOpen = clsOpen,
                rawReflect = rawReflect,
                subVtbls = PartVtbl.CreateSubArrayFromVtbl(rawReflect.vtbl),
                miniVtbl = PartVtbl.CreateMiniVtbl(rawReflect.vtbl),
            };
            var injections = rawReflect.injections.Select(r=>r.Create(reflectClass, nestedKeys)).ToArray();
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
            this.message = message;
        }
    }
}