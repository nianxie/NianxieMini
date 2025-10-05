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
    }
}