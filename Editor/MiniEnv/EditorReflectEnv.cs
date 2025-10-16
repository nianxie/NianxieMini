using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Nianxie.Components;
using Nianxie.Editor;
using UnityEngine;
using Nianxie.Utils;
using Nianxie.Framework;
using UnityEditor;

namespace XLua
{
    public class EditorReflectEnv:AbstractReflectEnv
    {
        public static EditorReflectEnv Create(EditorEnvPaths envPaths, byte[] miniBoot)
        {
            var env = new EditorReflectEnv(envPaths);
            try
            {
                env.Bootstrap(miniBoot);
                env.Warmup();
            }
            catch (Exception e)
            {
                Debug.LogError($"EditorReflectEnv warmup error {e}");
            }
            return env;
        }
        
        public override IReadOnlyDictionary<string, TextAsset> scriptAssetDict { get; }

        private EditorReflectEnv(EditorEnvPaths envPaths) : base(envPaths)
        {
            scriptAssetDict = new ReadonlyScriptAssetDictionary(envPaths.collectScriptDict);
        }
        
        protected override WarmedReflectClass FallbackReflect(string clsPath, string[] nestedPath, string message)
        {
            return new ErrorReflectClass(this, clsPath, nestedPath, message);
        }


        /// <summary>
        /// 收集lua层引用的assets
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> CollectReferenceAssetPaths()
        {
            var absPathToValidGuid = new Dictionary<string, string>();
            foreach (var warmedReflect in fileWarmedReflectDict.Values)
            {
                var pathSet = new HashSet<string>();
                warmedReflect.CollectReference(pathSet);
                foreach (var path in pathSet)
                {
                    var absPath = $"{envPaths.pathPrefix}/{path}";
                    var guid = AssetDatabase.AssetPathToGUID(absPath);
                    if (Directory.Exists(absPath))
                    {
                        Debug.LogError($"collect error in {warmedReflect.classPath}: {absPath} is a folder");
                    }
                    else if(string.IsNullOrEmpty(guid))
                    {
                        Debug.LogError($"collect error in {warmedReflect.classPath}: {absPath} not existed");
                    }
                    else
                    {
                        absPathToValidGuid[absPath] = guid;
                    }
                }
            }
            return absPathToValidGuid;
        }
        

        public bool CheckClassFieldMissing(LuaBehaviour behav, AbstractReflectInjection injection)
        {
            if (injection is AbstractNodeInjection nodeInjection)
            {
                foreach (var nodePath in nodeInjection.EachNodePath())
                {
                    if (nodeInjection.ToNodeObject(behav, nodePath) == null)
                    {
                        return true;
                    }
                }
            }
            else if(injection is AssetInjection assetInjection)
            {
                foreach (var assetPath in assetInjection.EachAssetPath())
                {
                    if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) != assetInjection.csharpType)
                    {
                        return true;
                    }
                }
            } else if (injection is SubAssetInjection subAssetInjection)
            {
                if (AssetDatabase.GetMainAssetTypeAtPath(subAssetInjection.assetPath) == null)
                {
                    return true;
                }
            } else if (injection is LuafabInjection luafabInjection) {
                if (AssetDatabase.GetMainAssetTypeAtPath(luafabInjection.assetPath) == null)
                {
                    return true;
                }
            }
            return false;
        }
        public bool CheckFieldClassMatch(ScriptInjection injection, LuaBehaviour behav)
        {
            var cls1 = injection.clsOpen;
            var cls2  = GetWarmedReflect(behav.classPath, behav.nestedKeys).clsOpen;
            return luaRawequal.Func<LuaTable, LuaTable, bool>(cls1, cls2);
        }
    }
}
