using System.Collections.Generic;
using System.Text;
using Nianxie.Craft;
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
        private LuaTable _luaTable = null;
		public LuaTable luaTable
		{
			get
			{
				if (!created)
				{
					created = true;
					CreateLuaTable(ref _luaTable);
				}
				return _luaTable;
			}
		}


		[BlackList] public string classPath = "";
        [BlackList] public string[] nestedKeys = EnvPaths.NESTED_KEYS_EMPTY;
		[BlackList] public string whichClass => $"{classPath}-[{string.Join(",", nestedKeys)}]";

        protected virtual void Awake()
        {
        }

        protected virtual void OnDestroy()
        {
            if (created)
            {
	            if (_luaTable == null)
	            {
					Debug.LogError("destroy but lua table not created??");
					return;
	            }
				_luaTable.Dispose();
				_luaTable = null;
            }
        }

        protected virtual void CreateLuaTable(ref LuaTable luaSelf)
        {
	        if (luaSelf != null)
	        {
		        throw new System.Exception("lua table create more than once");
	        }
	        if (gameObject == null)
	        {
		        throw new System.Exception("game object is destroy but try to create lua table");
	        }

	        var reflectEnv = gameManager.reflectEnv;
	        var luaReflect = reflectEnv.GetWarmedReflect(classPath, nestedKeys);
	        // 在这里提前赋值luaTable以保证子节点能正确拿到父节点的luaTable
	        luaSelf = reflectEnv.NewTable();

            // Init variables.
            luaSelf.Set("this", this);
            luaSelf.Set("gameObject", gameObject);
            luaSelf.Set("transform", gameObject.transform);
            luaSelf.Set("context", gameManager.context);
            foreach (var injection in luaReflect.injections)
            {
	            injection.ConstructTable(gameManager, this, luaSelf);
            }
            reflectEnv.BindMeta(luaSelf, luaReflect);
        }


#if UNITY_EDITOR
	    // 这部分代码用来刷新HierarchyWindow中的显示，运行时不需要。
	    protected HashSet<int> cacheSlotIds;
	    private Dictionary<int, StringBuilder> cacheGoIdToKeys;

	    [BlackList]
	    protected virtual void CachePutSlot(Component com)
	    {
		    // implement in SlotBehaviour
	    }

	    [BlackList]
        public void CacheRefresh(AbstractReflectEnv reflectEnv)
        {
	        cacheSlotIds ??= new HashSet<int>();
	        cacheSlotIds.Clear();
	        cacheGoIdToKeys ??= new Dictionary<int, StringBuilder>();
	        cacheGoIdToKeys.Clear();
			var nodeInjections = reflectEnv.GetWarmedReflect(classPath, nestedKeys).nodeInjections;
			foreach(var injection in nodeInjections) {
				foreach (var nodePath in injection.nodePathList)
				{
					GameObject go = null;
					if (injection is GameObjectInjection goInjection)
					{
						go = goInjection.ToGameObject(this, nodePath);
					}
					else if(injection is ScriptInjection scriptInjection)
					{
						var childBehav = scriptInjection.ToLuaBehaviour(this, nodePath);
						if (childBehav != null)
						{
							go = childBehav.gameObject;
						}
					}
					else if(injection is ComponentInjection comInjection)
					{
						var com = comInjection.ToComponent(this, nodePath);
						if (com != null)
						{
							go = com.gameObject;
						}
					}

					if (go != null)
					{
						if (!cacheGoIdToKeys.TryGetValue(go.GetInstanceID(), out var sb))
						{
							sb = new StringBuilder();
							cacheGoIdToKeys[go.GetInstanceID()] = sb;
						}
						sb.Append(injection.key);
						sb.Append(",");
					}
				}
			}
        }
        [BlackList]
        public string CacheGetGoFields(AbstractReflectEnv reflectEnv, int goInstanceId)
        {
	        if (cacheGoIdToKeys == null)
	        {
		        CacheRefresh(reflectEnv);
	        }

	        if (cacheGoIdToKeys.TryGetValue(goInstanceId, out var sb))
	        {
		        return sb.ToString();
	        }
	        else
	        {
		        return null;
	        }
        }
        [BlackList]
        public bool CacheContainSlot(AbstractReflectEnv reflectEnv, int slotInstanceId)
        {
	        if (cacheSlotIds == null)
	        {
		        CacheRefresh(reflectEnv);
	        }

	        return cacheSlotIds.Contains(slotInstanceId);
        }
#endif
    }
}