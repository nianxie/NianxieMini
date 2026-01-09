using System.Collections.Generic;
using Nianxie.Framework;
using Nianxie.Utils;
using XLua;

namespace Nianxie.Editor
{
    public abstract class EditorEnvPaths : EnvPaths
    {
        protected abstract EditorReflectEnv CreateReflectEnv();

        private SortedDictionary<string, CollectScript> _collectScriptDict;
        public SortedDictionary<string, CollectScript> collectScriptDict
        {
            get
            {
                if (_collectScriptDict == null)
                {
                    _collectScriptDict = CollectScript.Collect(this);
                }
                return _collectScriptDict;
            }
        }
        private EditorReflectEnv _reflectEnv;
        public EditorReflectEnv reflectEnv {
            get
            {
                if (_reflectEnv == null)
                {
                    _reflectEnv = CreateReflectEnv();
                }
                return _reflectEnv;
            }
        }

        public void SetObsolete()
        {
            // 清理掉当前的script和env，用的时候以lazy方式加载
            _collectScriptDict = null;
            _reflectEnv?.Dispose();
            _reflectEnv = null;
        }

        protected EditorEnvPaths(string vPrefix, string vContextName, string vRootLuafabPath):base(vPrefix, vContextName, vRootLuafabPath)
        {
        }

        protected EditorEnvPaths(string folder) : base(folder)
        {
        }
    }
}
