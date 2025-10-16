using UnityEditor.AssetImporters;
using UnityEngine;

namespace Nianxie.Editor
{
    [ScriptedImporter(1, new []{ "lua" })]
    public class LuaScriptImporter: ScriptedImporter
    {
        private static XLua.LuaEnv _compileEnv;
        protected static XLua.LuaEnv compileEnv
        {
            get
            {
                if (_compileEnv == null)
                {
                    _compileEnv = new XLua.LuaEnv();
                }
                return _compileEnv;
            }
        }
        
        public override void OnImportAsset(AssetImportContext ctx)
        {
            string textData = System.IO.File.ReadAllText(ctx.assetPath);
            try
            {
                compileEnv.LoadString(textData, ctx.assetPath);
            }
            catch (System.Exception e)
            {
                ctx.LogImportError(e.Message);
            }
            var textAsset = new TextAsset(textData);
            ctx.AddObjectToAsset("script", textAsset);
            ctx.SetMainObject(textAsset);
        }
    }
}