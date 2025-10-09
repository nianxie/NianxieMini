using Nianxie.Framework;
using UnityEngine;
using System;
using System.Collections.Generic;
using Nianxie.Components;

namespace XLua
{
    public enum InjectionMultipleKind
    {
        Single = 0,
        List = 1,
        Dict = 2, // Dict TODO
    }

    public class RawReflectInjection
    {
        public string key;
        public LuaTable clsOpen;
        public Type indexType;
        public Type csharpType;
        public string nodePath;
        public string assetPath;
        public LuaTable nodePathTable;
        public LuaTable assetPathTable;
        public bool lazy; // if luafab is lazy
        public bool table; // if is table
        protected AbstractReflectEnv reflectEnv;

        private AbstractNodeInjection CreateNodeInjection(WarmedReflectClass cls, string[] nestedKeys, InjectionMultipleKind multipleKind)
        {
            if (csharpType == typeof(LuaBehaviour) || csharpType.IsSubclassOf(typeof(LuaBehaviour)))
            {
                return new ScriptInjection(cls, nestedKeys, this, multipleKind);
            }
            else if(csharpType == typeof(GameObject))
            {
                return new GameObjectInjection(cls, this, multipleKind);
            }
            else if(csharpType == typeof(Component) || csharpType.IsSubclassOf(typeof(Component)))
            {
                return new ComponentInjection(cls, this, multipleKind);
            }
            else
            {
                Debug.LogError($"invalid node injection type: {csharpType}, only Component or GameObject can be inject as node, maybe inject it as asset?");
                throw new Exception("invalid node injection type");
            }
        }

        public AbstractReflectInjection Create(WarmedReflectClass cls, string[] nestedKeys)
        {
            if (!table)
            {
                if (csharpType.IsSubclassOf(typeof(AbstractGameHelper)))
                {
                    return new HelperInjection(cls, this);
                } else if(csharpType == typeof(LuafabLoading))
                {
                    return new LuafabInjection(cls, this);
                } else if (assetPath == null && nodePath != null)
                {
                    return CreateNodeInjection(cls, nestedKeys, InjectionMultipleKind.Single);
                } else if (assetPath != null)
                {
                    if (nodePath != null)
                    {
                        return new SubAssetInjection(cls, this, InjectionMultipleKind.Single);
                    }
                    else
                    {
                        return new AssetInjection(cls, this, InjectionMultipleKind.Single);
                    }
                }
                else
                {
                    return new ReferenceInjection(cls, this);
                }
            }
            else
            {
                var collectionKind = InjectionMultipleKind.List;
                if (indexType != null)
                {
                    Debug.LogError("injection Dict TODO");
                    collectionKind = InjectionMultipleKind.Dict;
                    return new ReferenceInjection(cls, this);
                }

                if (assetPath == null && assetPathTable == null && nodePathTable != null)
                {
                    return CreateNodeInjection(cls, nestedKeys, InjectionMultipleKind.List);
                } else if (assetPathTable != null)
                {
                    return new AssetInjection(cls, this, collectionKind);
                }
                else if(assetPath != null && nodePathTable != null)
                {
                    return new SubAssetInjection(cls, this, collectionKind);
                }
                return new ReferenceInjection(cls, this);
            }
        }
    }
}