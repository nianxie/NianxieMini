using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;
using System.Web.UI.WebControls;
using Cysharp.Threading.Tasks;
using Nianxie.Framework;
using Nianxie.Utils;
using Button = UnityEngine.UIElements.Button;

namespace Nianxie.Editor
{
    public class BuildMiniWindow : EasyWindow<BuildMiniWindow.View, BuildMiniWindow.State>
    {
        public class State : EasyState
        {
            public enum CreatingKind
            {
                NONE=0,
                GAME=1,
                CRAFT=2,
            }

            public CreatingKind creating;
            public string folder = "";
        }

        public class View: EasyHierarchy<View>
        {
            public class CreateView: EasyHierarchy<CreateView>
            {
                public VisualElement createBtns;
                public Button createGameBtn;
                public Button createCraftBtn;
                public VisualElement createForm;
                public Button cancelBtn;
                public TextField miniName;
                public TextField miniFolderPrefix;
                public TextField miniFolder;
                public VisualElement kindGame;
                public VisualElement kindCraft;
                public Button submitBtn;
            }
            public CreateView createView;

            public class BuildView : EasyHierarchy<BuildView>
            {
                public DropdownField folder;
                public Button ExecutePack;
                public Button ExecuteBuild;
            }

            public BuildView buildView;
        }

        private const string WND_NAME = "打包构建";
        
        [UnityEditor.MenuItem("念写Mini/"+WND_NAME, false, 2)]
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

        protected override void Refresh()
        {
            view.createView.Apply((createView) =>
            {
                createView.createBtns.SetDisplay(state.creating == State.CreatingKind.NONE);
                createView.createForm.SetDisplay(state.creating != State.CreatingKind.NONE);
                createView.kindCraft.SetDisplay(state.creating == State.CreatingKind.CRAFT);
                createView.kindGame.SetDisplay(state.creating == State.CreatingKind.GAME);
            });
        }

        protected override void Setup()
        {
            var pathList = ListProjectFolders();
            view.buildView.folder.choices = pathList;
            if (pathList.Count > 0)
            {
                view.buildView.folder.SetValueWithoutNotify(pathList[0]);
                state.folder = view.buildView.folder.value;
            }
            else
            {
                state.folder = "";
            }

            // binding create view
            view.createView.Apply((createView) =>
            {
                createView.miniFolderPrefix.SetEnabled(false);
                string autoProjectName()
                {
                    string defaultFolder = "newProject";
                    string validFolder = defaultFolder;
                    int k = 1;
                    while (Directory.Exists($"{NianxieConst.MiniPrefixPath}/{validFolder}"))
                    {
                        validFolder = $"{defaultFolder}_{k++}";
                    }
                    return validFolder;
                }
                createView.createGameBtn.clicked += () =>
                {
                    state.creating = State.CreatingKind.GAME;
                    createView.miniFolder.value = autoProjectName();
                    Refresh();
                };
                createView.createCraftBtn.clicked += () =>
                {
                    state.creating = State.CreatingKind.CRAFT;
                    createView.miniFolder.value = autoProjectName();
                    Refresh();
                };
                createView.cancelBtn.clicked += () =>
                {
                    state.creating = State.CreatingKind.NONE;
                    Refresh();
                };
                createView.submitBtn.clicked += () =>
                {
                    if (state.creating != State.CreatingKind.NONE)
                    {
                        CopyTemplateAsProject(view.createView.miniFolder.value, state.creating == State.CreatingKind.CRAFT, view.createView.miniName.value);
                        state.creating = State.CreatingKind.NONE;
                        Refresh();
                    }
                };
            });
            // binding build view
            view.buildView.Apply((buildView) =>
            {
                buildView.folder.RegisterValueChangedCallback((e) =>
                {
                    state.folder = view.buildView.folder.value;
                });
                buildView.ExecuteBuild.clicked+=()=>
                {
                    ExecuteBuild(state.folder);
                };
                buildView.ExecutePack.clicked+=()=>
                {
                    ExecutePack(state.folder);
                };
            });
        }
        
        /**
         * 重命名, 通过一些hack重命名bundle，下个版本统一重构
         */
        public static void ExecuteRename(string folder, Guid targetGuid)
        {
            AssetBundle.UnloadAllAssetBundles(true);
            var envPaths = MiniEditorEnvPaths.Get(folder);
            UniTask.Create(async () =>
            {
                foreach (var (platform,path) in envPaths.finalBundleDict)
                {
                    // 解压->重命名->压缩
                    var originPath = envPaths.finalBundleDict[BuildTarget.iOS];
                    var uncompressPath = $"{envPaths.buildDir}/temp_{envPaths.folder}_uncompress.bundle";
                    var finalPath = $"{envPaths.buildDir}/{envPaths.folder}_{platform}.bundle";
                    await AssetBundle.RecompressAssetBundleAsync(originPath, uncompressPath, BuildCompression.Uncompressed).ToUniTask();
                    var bundleBytes = await File.ReadAllBytesAsync(uncompressPath);
                    MiniEditorEnvPaths.RenameMagicBundle(bundleBytes, targetGuid);
                    await File.WriteAllBytesAsync(uncompressPath, bundleBytes);
                    await AssetBundle.RecompressAssetBundleAsync(uncompressPath, finalPath, BuildCompression.LZ4Runtime).ToUniTask();
                }
            }).Forget();
        }

        private static void ExecuteBuild(string folder)
        {
            var envPaths = MiniEditorEnvPaths.Get(folder);
            envPaths.Build();
        }

        private static void ExecutePack(string folder)
        {
            var envPaths = MiniEditorEnvPaths.Get(folder);
            var notScriptGuids = CollectNotScript.Collect(envPaths.reflectEnv).Values.Select(a => a.guid).Where(a=>!string.IsNullOrEmpty(a));
            var scriptGuids = envPaths.collectScriptDict.Values.Select(a => a.guid);
            var guids = notScriptGuids.Concat(scriptGuids).ToArray();
            ShowExportPackageWindow(guids);
        }

        private static void CopyTemplateAsProject(string folder, bool craftable, string name)
        {
            var srcPath = craftable?NianxieConst.TemplateSimpleCraft:NianxieConst.TemplateSimpleGame;
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
                    miniEnvPaths.FlushName(name);
                }
            }
            else
            {
                Debug.LogError($"project create error: copy maybe fail {srcPath} -> {dstPath}");
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
