using UnityEngine;
using XLua;

namespace Nianxie.Editor
{
    public class ShellEditorEnvPaths : EditorEnvPaths
    {
        public static ShellEditorEnvPaths Instance = new ShellEditorEnvPaths();

        protected override EditorReflectEnv CreateReflectEnv()
        {
            Debug.Log($"shell refresh editor reflect env : {pathPrefix}");
            return EditorReflectEnv.Create(this, null);
        }
    }
}
