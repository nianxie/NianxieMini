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

        protected abstract void CreateLuaTable(ref LuaTable luaSelf);
    }
}