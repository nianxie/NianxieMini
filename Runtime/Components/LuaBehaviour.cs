using System;
using Nianxie.Framework;
using UnityEngine;
using XLua;

namespace Nianxie.Components
{
	[DisallowMultipleComponent]
    public abstract class LuaBehaviour : MonoBehaviour
    {
        // 需要让带有LuaBehaviour的Luafab在Instantiate的时候持有luaModule的引用，由AssetModule遍历赋值
        [BlackList] public AbstractGameManager gameManager;
        private bool created = false;
        protected LuaTable _luaTable = null;
		public LuaTable luaTable
		{
			get
			{
				if (!created)
				{
					created = true;
					CreateLuaTable(out _luaTable);
				}
				return _luaTable;
			}
		}


		[BlackList] public string classPath = "";
        [BlackList] public string[] nestedKeys = EnvPaths.NESTED_KEYS_EMPTY;
		[BlackList] public string whichClass => $"{classPath}-[{string.Join(",", nestedKeys)}]";

        protected virtual void Awake()
        {
            // get 一下luaTable，以确保luaTable的创建
            var _ = luaTable;
        }

        protected virtual void OnDestroy()
        {
            if (_luaTable == null)
            {
	            Debug.LogError("destroy but lua table not created??");
                return;
            }
            _luaTable.Dispose();
            _luaTable = null;
        }

        private void CreateLuaTable(out LuaTable outLuaTable)
        {
	        if (_luaTable != null)
	        {
		        throw new Exception("lua table create more than once");
	        }
	        if (gameObject == null)
	        {
		        throw new Exception("game object is destroy but try to create lua table");
	        }

	        var reflectEnv = gameManager.reflectEnv;
	        var luaReflect = reflectEnv.GetWarmedReflect(classPath, nestedKeys);
	        var luaSelf = reflectEnv.NewTable();
	        // 在这里赋值一下luaTable到外面，以保证子节点能正确拿到父节点的luaTable
	        outLuaTable = luaSelf;

            // Init variables.
            luaSelf.Set("this", this);
            luaSelf.Set("gameObject", gameObject);
            luaSelf.Set("transform", gameObject.transform);
            luaSelf.Set("context", gameManager.context);
            var parentBehav = GetComponentInParent<LuaBehaviour>();
            if (parentBehav != null)
            {
				luaSelf.Set("parent", parentBehav.luaTable);
            }
            // TODO， 考虑挪到RuntimeReflectEnv中..吗？
            foreach (var injection in luaReflect.injections)
            {
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
							Debug.LogError($"[{whichClass}]ready luafab not loaded : path={luafabInjection.assetPath}");
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
					if (nodeInjection.collectionKind == InjectionMultipleKind.Single)
					{
						if (nodeInjection is ScriptInjection scriptInjection)
						{
							var scriptTable = scriptInjection.ToLuaScript(this, nodeInjection.nodePath);
							luaSelf.Set(injection.key, scriptTable);
						}
						else
						{
							var obj = nodeInjection.ToNodeObject(this, nodeInjection.nodePath);
							luaSelf.Set(injection.key, obj);
						}
					}
					else if(nodeInjection.collectionKind == InjectionMultipleKind.List)
					{
						var t = reflectEnv.NewTable();
						if (nodeInjection is ScriptInjection scriptInjection)
						{
							for (int i = 0; i < nodeInjection.nodePathList.Length; i++)
							{
								var scriptTable = scriptInjection.ToNodeObject(this, nodeInjection.nodePathList[i]);
								t.Set(i+1, scriptTable);
							}
						}
						else
						{
							for (int i = 0; i < nodeInjection.nodePathList.Length; i++)
							{
								var obj = nodeInjection.ToNodeObject(this, nodeInjection.nodePathList[i]);
								t.Set(i+1, obj);
							}
						}
						luaSelf.Set(injection.key, t);
					}
				}
            }
            reflectEnv.BindMeta(luaSelf, luaReflect);
        }
    }
}