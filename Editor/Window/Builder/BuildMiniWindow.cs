using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;
using Nianxie.Framework;
using Nianxie.Utils;

namespace Nianxie.Editor
{
    public class BuildMiniWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        private const string WND_NAME = "打包构建";
        
        [MenuItem("念写Mini/"+WND_NAME, false, 2)]
        public static void OpenBuildWindow()
        {
            BuildMiniWindow wnd = GetWindow<BuildMiniWindow>(WND_NAME, true);
            //wnd.titleContent = new GUIContent("BuildWindow");
            wnd.minSize = new Vector2(400, 400);
        }

        public static List<string> ListProjectFolders()
        {
            return Directory.Exists(NianxieConst.MiniPrefixPath)
                ?Directory.EnumerateDirectories(NianxieConst.MiniPrefixPath).Select((e) => new DirectoryInfo(e).Name).ToList()
                :new List<string>();
        }

        private string folder = "";
        
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // VisualElements objects can contain other VisualElement following a tree hierarchy.
            // VisualElement label = new Label("Hello World! From C#");
            // root.Add(label);

            // Instantiate UXML
            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            root.Add(labelFromUXML);
            
            // draw path drop down
            var pathList = ListProjectFolders();
            var miniProjectDropDown = root.Query<DropdownField>(nameof(folder)).First();
            miniProjectDropDown.choices = pathList;
            if (pathList.Count > 0)
            {
                miniProjectDropDown.SetValueWithoutNotify(pathList[0]);
                folder = miniProjectDropDown.value;
            }
            else
            {
                folder = "";
            }

            miniProjectDropDown.RegisterValueChangedCallback((e) =>
            {
                folder= miniProjectDropDown.value;
            });
            root.Query("Panel").First().Query<Button>(nameof(ExecuteBuild)).First().clicked+=()=>
            {
                ExecuteBuild(folder);
            };
            root.Query("Panel").First().Query<Button>(nameof(ExecutePack)).First().clicked+=()=>
            {
                ExecutePack(folder);
            };
        }

        public static void ExecuteBuild(string folder)
        {
            var envPaths = MiniEditorEnvPaths.Get(folder);
            envPaths.Build();
        }
        
        public static void ExecutePack(string folder)
        {
            var envPaths = MiniEditorEnvPaths.Get(folder);
            var notScriptGuids = CollectNotScript.Collect(envPaths.reflectEnv).Values.Select(a => a.guid).Where(a=>!string.IsNullOrEmpty(a));
            var scriptGuids = envPaths.collectScriptDict.Values.Select(a => a.guid);
            var guids = notScriptGuids.Concat(scriptGuids).ToArray();
            ShowExportPackageWindow(guids);
        }

        public static bool CopyTemplateAsProject(MiniCommonConfig config, string folder)
        {
            var srcPath = config.craftable?NianxieConst.TemplateSimpleCraft:NianxieConst.TemplateSimpleGame;
            var dstPath = $"{NianxieConst.MiniPrefixPath}/{folder}";
            if (!Directory.Exists(NianxieConst.MiniPrefixPath))
            {
                Directory.CreateDirectory(NianxieConst.MiniPrefixPath);
            }

            if (AssetDatabase.CopyAsset(srcPath, dstPath))
            {
                var miniEnvPaths = MiniEditorEnvPaths.Get(folder);
                if (miniEnvPaths!=null)
                {
                    miniEnvPaths.FlushName(config.name);
                }
                return true;
            }
            else
            {
                Debug.LogError($"project create error: copy maybe fail {srcPath} -> {dstPath}");
                return false;
            }
        }

        private static void ShowExportPackageWindow(ICollection<string> guids)
        {
            // 1. open window
            System.Type PackageExport = typeof(EditorWindow).Assembly.GetType($"UnityEditor.{nameof(PackageExport)}");
            FieldInfo m_IncludeDependencies = PackageExport.GetField(nameof(m_IncludeDependencies),
                BindingFlags.Instance | BindingFlags.NonPublic);
            var window = EditorWindow.GetWindow(PackageExport, true, "Export Package");
            // 2. disable dependencies
            m_IncludeDependencies.SetValue(window, false);
            // 3. build items
            System.Type ExportPackageItem = typeof(EditorWindow).Assembly.GetType($"UnityEditor.{nameof(ExportPackageItem)}");
            object itemArray;
            if (guids.Count > 0)
            {
                MethodInfo GetAssetItemsForExport = PackageExport.GetMethod(nameof(GetAssetItemsForExport),
                    BindingFlags.Static | BindingFlags.NonPublic);
                var itemEnumerable = GetAssetItemsForExport.Invoke(null, new object[] {guids, false, false});
                MethodInfo ToArray = typeof(System.Linq.Enumerable).GetMethod(nameof(ToArray), BindingFlags.Static | BindingFlags.Public);
                itemArray = ToArray.MakeGenericMethod(ExportPackageItem).Invoke(null, new object[] {itemEnumerable});
            }
            else
            {
                itemArray = Array.CreateInstance(ExportPackageItem, 0);
            }

            // 4. set item and repaint
            FieldInfo m_ExportPackageItems = PackageExport.GetField(nameof(m_ExportPackageItems), BindingFlags.Instance | BindingFlags.NonPublic);
            m_ExportPackageItems.SetValue(window, itemArray);
            FieldInfo m_Tree = PackageExport.GetField(nameof(m_Tree), BindingFlags.Instance | BindingFlags.NonPublic);
            m_Tree.SetValue(window, null);
            FieldInfo m_TreeViewState = PackageExport.GetField(nameof(m_TreeViewState), BindingFlags.Instance | BindingFlags.NonPublic);
            m_TreeViewState.SetValue(window, null);
            window.Repaint();
        }
    }
}
