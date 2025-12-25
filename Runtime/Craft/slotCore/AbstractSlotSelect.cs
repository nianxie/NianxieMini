using Nianxie.Utils;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    [DisallowMultipleComponent]
    public abstract class AbstractSlotSelect:MonoBehaviour
    {
        protected const string SELECT_BODY_NAME = "::body";
#if UNITY_EDITOR
        [BlackList]
        public virtual void EditorInspectorUpdate(NianxieDefaultAssets defaultAssets)
        {
        }
        [BlackList]
        public virtual void EditorLocalUpdate(NianxieDefaultAssets defaultAssets)
        {
        }
#endif
    }
}