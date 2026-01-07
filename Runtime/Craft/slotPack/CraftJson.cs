using System;
using System.Collections.Generic;
using System.Linq;
using Nianxie.Riff;
using UnityEngine;

namespace Nianxie.Craft
{
    public class CraftJson:CustomJson
    {
        public override string version => "0.0.1";

        public virtual Dictionary<string, Type> FactoryBinderTypeMap()
        {
            var slotJsonType = typeof(AbstractSlotJson);
            // 使用反射获取contentType同命名空间、同程序集的派生类
            var asm = AppDomain.CurrentDomain.GetAssemblies().First(asm => asm.GetType(slotJsonType.FullName) != null);
            var jsonTypes = asm.GetTypes().Where(type => type.Namespace == slotJsonType.Namespace && type.IsSubclassOf(slotJsonType)).ToArray();
            var typeMap = jsonTypes.ToDictionary(type => type.Name);
            return typeMap;
        }
        public SlotBehavJson root;
    }

}
