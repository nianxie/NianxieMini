using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Nianxie.Components;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEditor;
using UnityEngine;
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

        protected EditorEnvPaths() : base()
        {
        }

        protected EditorEnvPaths(string folder) : base(folder)
        {
        }
        public static bool TryMapEnvPaths(string assetPath, out EditorEnvPaths envPaths)
        {
            if (assetPath.StartsWith(NianxieConst.ShellResPath))
            {
                envPaths = ShellEditorEnvPaths.Instance;
                return true;
            }
            if (assetPath.StartsWith(NianxieConst.MiniPrefixPath))
            {
                var splitArr = assetPath.Split("/");
                if (splitArr.Length >= 3 && !string.IsNullOrEmpty(splitArr[2]))
                {
                    var folder = splitArr[2];
                    var miniEnvPaths = MiniEditorEnvPaths.Get(folder);
                    envPaths = miniEnvPaths;
                    return true;
                }
            }
            envPaths = null;
            return false;
        }
    }
}
