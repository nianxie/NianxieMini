using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    [DisallowMultipleComponent]
    public abstract class AbstractSlotSelect:MonoBehaviour
    {
        protected const string SELECT_BODY_NAME = "::select";
#if UNITY_EDITOR
        [BlackList]
        public virtual void EditorInspectorUpdate()
        {
        }
        [BlackList]
        public virtual void EditorLocalUpdate()
        {
        }
#endif
    }
}