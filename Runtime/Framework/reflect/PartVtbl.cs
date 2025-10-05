using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nianxie.Components;
using UnityEngine;
using XLua;

namespace Nianxie.Framework
{
    public abstract class PartVtbl
    {
        private static readonly Dictionary<Type, string[]> type2fields = new();
        private static T TryCreate<T>(Dictionary<string, LuaFunction> vtbl) where T:PartVtbl, new()
        {
            var type = typeof(T);
            if (!type2fields.TryGetValue(type, out var fields))
            {
                var fieldList = new List<string>(6);
                foreach(var field in type.GetFields(BindingFlags.Public|BindingFlags.Instance))
                {
                    if (field.FieldType == typeof(LuaFunction))
                    {
                        fieldList.Add(field.Name);
                    }
                    else
                    {
                        Debug.LogError($"vtbl can only take LuaFunction but got {field.FieldType}");
                    }
                }
                fields = fieldList.ToArray();
                type2fields[type] = fields;
            }

            T subVtbl = null;
            foreach (var field in fields)
            {
                if (vtbl.TryGetValue(field, out var method))
                {
                    subVtbl ??= new T();
                    type.GetField(field).SetValue(subVtbl, method);
                }
            }
            return subVtbl;
        }
        
        public static MiniVtbl CreateMiniVtbl(Dictionary<string, LuaFunction> vtbl)
        {
            return TryCreate<MiniVtbl>(vtbl) ?? new MiniVtbl();
        }

        public static PartVtbl[] CreateSubArrayFromVtbl(Dictionary<string, LuaFunction> vtbl)
        {
            var ret = new PartVtbl[]
            {
                TryCreate<FixedUpdateVtbl>(vtbl),
                TryCreate<UpdateVtbl>(vtbl),
                TryCreate<LateUpdateVtbl>(vtbl),
                TryCreate<VisibleVtbl>(vtbl),
                TryCreate<PhysicsVtbl>(vtbl),
                TryCreate<Physics2DVtbl>(vtbl),
                // handlers
                TryCreate<DragBeginEndVtbl>(vtbl),
                TryCreate<DragVtbl>(vtbl),
                TryCreate<DropVtbl>(vtbl),
                TryCreate<PointerVtbl>(vtbl),
            };
            return ret.Where(e => e != null).ToArray();
        }

        public abstract SubBehaviour AddComponent(MiniBehaviour miniBehaviour);
    }

    public abstract class PartVtbl<TBehav>:PartVtbl where TBehav : SubBehaviour
    {
        public override SubBehaviour AddComponent(MiniBehaviour miniBehaviour)
        {
            var behav = miniBehaviour.gameObject.AddComponent<TBehav>();
            behav.Init(miniBehaviour.enabled, miniBehaviour.luaTable, this);
            return behav;
        }
    }

    public class MiniVtbl:PartVtbl
    {
        public LuaFunction Awake;
        public LuaFunction Start;
        public LuaFunction OnDestroy;
        public LuaFunction OnEnable;
        public LuaFunction OnDisable;
        public override SubBehaviour AddComponent(MiniBehaviour miniBehaviour)
        {
            throw new NotImplementedException();
        }
    }
    #region // sub behaviours exclude handler
    public class FixedUpdateVtbl:PartVtbl<FixedUpdateSubBehaviour>
    {
        public LuaFunction FixedUpdate;
    }
    public class UpdateVtbl:PartVtbl<UpdateSubBehaviour>
    {
        public LuaFunction Update;
    }
    public class LateUpdateVtbl:PartVtbl<LateUpdateSubBehaviour>
    {
        public LuaFunction LateUpdate;
    }
    public class VisibleVtbl:PartVtbl<VisibleSubBehaviour>
    {
		public LuaFunction OnBecameVisible;
		public LuaFunction OnBecameInvisible;
    }
    public class PhysicsVtbl:PartVtbl<PhysicsSubBehaviour>
    {
		public LuaFunction OnTriggerEnter;
		public LuaFunction OnTriggerStay;
		public LuaFunction OnTriggerExit;
		public LuaFunction OnCollisionEnter;
		public LuaFunction OnCollisionStay;
		public LuaFunction OnCollisionExit;
    }
    public class Physics2DVtbl:PartVtbl<Physics2DSubBehaviour>
    {
		public LuaFunction OnTriggerEnter2D;
		public LuaFunction OnTriggerStay2D;
		public LuaFunction OnTriggerExit2D;
		public LuaFunction OnCollisionEnter2D;
		public LuaFunction OnCollisionStay2D;
		public LuaFunction OnCollisionExit2D;
    }
    #endregion
    #region // sub handlers
    public class PointerVtbl:PartVtbl<PointerSubHandler>
    {
		public LuaFunction OnPointerDown;
		public LuaFunction OnPointerUp;
		public LuaFunction OnPointerClick;
    }
    public class DragBeginEndVtbl:PartVtbl<DragBeginEndSubHandler>
    {
        public LuaFunction OnInitializePotentialDrag;
		public LuaFunction OnBeginDrag;
		public LuaFunction OnEndDrag;
    }
    public class DragVtbl:PartVtbl<DragSubHandler>
    {
		public LuaFunction OnDrag;
    }
    public class DropVtbl:PartVtbl<DropSubHandler>
    {
		public LuaFunction OnDrop;
    }
    #endregion
}