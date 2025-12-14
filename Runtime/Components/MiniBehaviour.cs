using System;
using System.Collections.Generic;
using Nianxie.Utils;
using Nianxie.Framework;
using UnityEngine;
using XLua;

namespace Nianxie.Components
{
    public class MiniBehaviour : LuaBehaviour
    {
        private SubBehaviour[] subBehavs;
        private MiniVtbl miniVtbl;
        protected override void Awake()
        {
            // get 一下luaTable，以确保luaTable的创建
            var _ = luaTable;
	        var reflectEnv = gameManager.reflectEnv;
	        var warmedReflect = reflectEnv.GetWarmedReflect(classPath, nestedKeys);
            miniVtbl = warmedReflect.miniVtbl;
            subBehavs = new SubBehaviour[warmedReflect.subVtbls.Length];
            for (int i = 0; i < warmedReflect.subVtbls.Length;i++)
            {
                subBehavs[i] = warmedReflect.subVtbls[i].AddComponent(this);
            }
            miniVtbl.Awake?.Action(luaTable);
        }

        void Start()
        {
            miniVtbl.Start?.Action(luaTable);
        }

        void OnEnable()
        {
            foreach (var subBehav in subBehavs)
            {
                subBehav.enabled = true;
            }
            miniVtbl.OnEnable?.Action(luaTable);
        }
        void OnDisable()
        {
            foreach (var subBehav in subBehavs)
            {
                subBehav.enabled = false;
            }
            miniVtbl.OnDisable?.Action(luaTable);
        }
        protected override void OnDestroy()
        {
            try
            {
                miniVtbl.OnDestroy?.Action(luaTable);
            }
            finally
            {
                base.OnDestroy();
            }
        }
        protected override void CreateLuaTable(ref LuaTable luaSelf)
        {
	        if (luaSelf != null)
	        {
		        throw new Exception("lua table create more than once");
	        }
	        if (gameObject == null)
	        {
		        throw new Exception("game object is destroy but try to create lua table");
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
    }
}