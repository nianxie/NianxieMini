using System.Collections;
using System.Collections.Generic;
using Nianxie.Editor;
using UnityEditor;
using XLua;

namespace Nianxie.Editor
{
    public class AbstractCollectAsset
    {
        public string guid { get; protected set; }
        public string path { get; protected set; }

        protected AbstractCollectAsset() {}
    }
}
