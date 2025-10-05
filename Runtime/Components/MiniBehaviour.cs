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
            base.Awake();
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
    }
}