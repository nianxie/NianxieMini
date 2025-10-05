using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XLua;

namespace Nianxie.Framework
{
    // 需要暴露接口到Lua层的Module需要继承AbstractGameHelper，并以xxxxHelper命名
    public abstract class AbstractGameHelper: AbstractGameModule
    {
    }
}

