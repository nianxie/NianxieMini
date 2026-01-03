using System.Collections.Generic;
using System;

namespace Nianxie.Riff
{
    public abstract class AbstractRiffJson
    {
        private static Dictionary<Type, Func<string, AbstractRiffJson>> DeserializeDict = new();
        private static Dictionary<Type, Func<AbstractRiffJson, string>> SerializeDict;
    }
}