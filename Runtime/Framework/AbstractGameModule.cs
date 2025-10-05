using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XLua;

namespace Nianxie.Framework
{

    public abstract class AbstractGameModule: MonoBehaviour
    {
        [SerializeField] 
        protected AbstractGameManager gameManager;
        protected RuntimeReflectEnv reflectEnv => gameManager.reflectEnv;
        public virtual async UniTask Init()
        {
        }
        
        public virtual async UniTask LateInit()
        {
        }

        public virtual async UniTask Destroy()
        {
        }

#if UNITY_EDITOR
        private void Reset()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            var type = this.GetType();
            foreach(var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)){
                if (field.FieldType.IsSubclassOf(typeof(AbstractGameModule)) || field.FieldType == typeof(AbstractGameManager))
                {
                    var otherModule = GetComponent(field.FieldType);
                    if ((Component) field.GetValue(this) != otherModule)
                    {
                        field.SetValue(this, otherModule);
                    }
                }
            }
        }
#endif
    }
}

