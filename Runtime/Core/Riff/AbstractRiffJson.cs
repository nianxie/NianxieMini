
using System.Collections.Generic;
using System;

namespace Nianxie.Riff
{
    public abstract class AbstractRiffJson
    {
        /// <summary>
        /// 用来在反序列化时映射到具体类
        /// </summary>
        public string fullName => GetType().FullName;
        /// <summary>
        /// 可能会拿来实现一些版本兼容，但目前没有用到
        /// </summary>
        public abstract string version { get; }

        /// <summary>
        /// 内部需要支持多态的结构体，通过这个方法控制JsonSerializerSettings的ISerializationBinder
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<string, Type> FactoryBinderTypeMap()
        {
            return null;
        }
        public string Dump()
        {
            return JsonCodec.Dump(this);
        }
    }
}