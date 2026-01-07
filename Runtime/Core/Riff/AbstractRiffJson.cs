
using System.Collections.Generic;
using System;

namespace Nianxie.Riff
{
    public abstract class AbstractRiffJson
    {
        public string fullName => GetType().FullName;
        public abstract string version { get; }

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