using Nianxie.Framework;
using UnityEngine;
using System;
using System.Collections.Generic;
using Nianxie.Components;

namespace XLua
{
    public abstract class AbstractReflectInjection
    {
        protected readonly WarmedReflectClass reflectClass;
        public readonly string key;
        public readonly Type csharpType;
        protected AbstractReflectInjection(WarmedReflectClass cls, RawReflectInjection rawInjection)
        {
            reflectClass = cls;
            csharpType = rawInjection.csharpType;
            key = rawInjection.key;
        }

        public void ConstructTable(AbstractGameManager gameManager, LuaBehaviour behav, LuaTable luaSelf)
        {
	        // TODO 是否考虑改成虚函数的实现方式？
	        var reflectEnv = gameManager.reflectEnv;
	        var injection = this;
			if (injection is HelperInjection)
			{
				gameManager.OnInjectGameHelper(luaSelf, injection.key, injection.csharpType);
			} else if (injection is LuafabInjection luafabInjection)
			{
				var luafabLoading = gameManager.assetModule.AttachLuafabLoading(luafabInjection.assetPath, true);
				luaSelf.Set(injection.key, luafabLoading);
				if (!luafabInjection.lazy)
				{
					if (!luafabLoading.Done)
					{
						Debug.LogError($"[{reflectClass.classPath}] ready luafab not loaded : path={luafabInjection.assetPath}");
					}
				}
			} else if (injection is AssetInjection assetInjection)
			{
				if (assetInjection.multipleKind == InjectionMultipleKind.Single)
				{
					var obj = gameManager.assetModule.GetTypedAsset(assetInjection.assetPath, assetInjection.csharpType);
					luaSelf.Set(injection.key, obj);
				}
				else if(assetInjection.multipleKind == InjectionMultipleKind.List)
				{
					var t = reflectEnv.NewTable();
					for (int i = 0; i < assetInjection.assetPathList.Length; i++)
					{
						var obj = gameManager.assetModule.GetTypedAsset(assetInjection.assetPathList[i], assetInjection.csharpType);
						t.Set(i + 1, obj);
					}
					luaSelf.Set(injection.key, t);
				}
			} else if (injection is SubAssetInjection subAssetInjection) {
				if (subAssetInjection.collectionKind == InjectionMultipleKind.Single)
				{
					var obj = gameManager.assetModule.GetSubAsset(subAssetInjection.assetPath, subAssetInjection.subName);
					luaSelf.Set(injection.key, obj);
				}
				else if(subAssetInjection.collectionKind == InjectionMultipleKind.List)
				{
					var t = reflectEnv.NewTable();
					for (int i = 0; i < subAssetInjection.subNameList.Length; i++)
					{
						var obj = gameManager.assetModule.GetSubAsset(subAssetInjection.assetPath, subAssetInjection.subNameList[i]);
						t.Set(i, obj);
					}
					luaSelf.Set(injection.key, t);
				}
			} else if (injection is AbstractNodeInjection nodeInjection) {
				if (nodeInjection.multipleKind == InjectionMultipleKind.Single)
				{
					if (nodeInjection is ScriptInjection scriptInjection)
					{
						var scriptTable = scriptInjection.ToLuaScript(behav, nodeInjection.nodePath);
						luaSelf.Set(injection.key, scriptTable);
					}
					else
					{
						var obj = nodeInjection.ToNodeObject(behav, nodeInjection.nodePath);
						luaSelf.Set(injection.key, obj);
					}
				}
				else if(nodeInjection.multipleKind == InjectionMultipleKind.List)
				{
					var t = reflectEnv.NewTable();
					if (nodeInjection is ScriptInjection scriptInjection)
					{
						for (int i = 0; i < nodeInjection.nodePathList.Length; i++)
						{
							var scriptTable = scriptInjection.ToNodeObject(behav, nodeInjection.nodePathList[i]);
							t.Set(i+1, scriptTable);
						}
					}
					else
					{
						for (int i = 0; i < nodeInjection.nodePathList.Length; i++)
						{
							var obj = nodeInjection.ToNodeObject(behav, nodeInjection.nodePathList[i]);
							t.Set(i+1, obj);
						}
					}
					luaSelf.Set(injection.key, t);
				}
			}
        }
    }
}