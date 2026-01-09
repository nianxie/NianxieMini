using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;
using System.Web.UI.WebControls;
using Cysharp.Threading.Tasks;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEditor.UIElements;
using Button = UnityEngine.UIElements.Button;
using Label = UnityEngine.UIElements.Label;

namespace Nianxie.Editor
{
    public class ProjectWindow : EasyWindow<ProjectWindow.View, ProjectWindow.State>
    {
        private const string WND_NAME = "本地项目管理";
        
        [UnityEditor.MenuItem("念写Mini/"+WND_NAME, false, 1)]
        public static void OpenBuildWindow()
        {
            ProjectWindow wnd = GetWindow<ProjectWindow>(WND_NAME, true);
            wnd.titleContent = new GUIContent(WND_NAME);
            wnd.minSize = new Vector2(500, 500);
        }
        
        [SerializeField]
        private VisualTreeAsset uxmlItemAsset = default;
        
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

        public class ItemView:EasyView<ItemView>
        {
            public Button selectBtn;
            public VisualElement kindGame;
            public VisualElement kindCraft;
            public Label miniName;
            public Label miniFolder;
        }

        public ItemView[] itemViews;

        public class View: EasyView<View>
        {
            public class CreateView: EasyView<CreateView>
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

            public VisualElement selectView;
            public class ManagerView : EasyView<ManagerView>
            {

                public Button cancelBtn;
                public Toggle iosBuild;
                public Toggle androidBuild;
                public Toggle webglBuild;
                public TextField bundleManifest;
                public TextField iosBundle;
                public TextField androidBundle;
                public TextField webglBundle;
                public Button executePack;
                public Button executeBuild;
                public Button openFolder;
                public Button gotoUpload;
                public class DetailView: EasyView<DetailView>
                {
                    public Label miniName;
                    public VisualElement kindGame;
                    public VisualElement kindCraft;
                    public ObjectField folderField;
                }
                public DetailView detailView;
            }
            public ManagerView managerView;
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
            if (string.IsNullOrEmpty(state.folder))
            {
                view.selectView.SetDisplay(true);
                view.managerView.SetDisplay(false);
                var pathList = ListProjectFolders();
                view.selectView.Clear();
                itemViews = new ItemView[pathList.Count];
                for (int i = 0; i < pathList.Count; i++)
                {
                    var folder = pathList[i];
                    uxmlItemAsset.CloneTree(view.selectView, out int index, out _);
                    var itemView = EasyView.CreateByQuery<ItemView>(view.selectView[index]);
                    itemView.selectBtn.clicked+=()=>{
                        state.folder = folder;
                        Refresh();
                    };
                    var envPaths = MiniEditorEnvPaths.GetOrCreate(folder);
                    var craftable = envPaths.config.craftable;
                    itemView.kindCraft.SetDisplay(craftable);
                    itemView.kindGame.SetDisplay(!craftable);
                    itemView.miniFolder.text = envPaths.folder;
                    itemView.miniName.text = envPaths.config.name;
                    itemViews[i] = itemView;
                }
            } else
            {
                view.selectView.SetDisplay(false);
                view.managerView.SetDisplay(true);
                var envPaths = MiniEditorEnvPaths.GetOrCreate(state.folder);
                view.managerView.Apply((managerView) =>
                {
                    managerView.detailView.Apply((self) =>
                    {
                        var craftable = envPaths.config.craftable;
                        self.kindCraft.SetDisplay(craftable);
                        self.kindGame.SetDisplay(!craftable);
                        self.miniName.text = envPaths.config.name;
                        self.folderField.value = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(envPaths.pathPrefix);
                    });
                    if (File.Exists(envPaths.finalManifest))
                    {
                        managerView.bundleManifest.value = envPaths.finalManifest;
                    }
                    else
                    {
                        managerView.bundleManifest.value = "未构建";
                    }
                    var tuple = new[]
                    {
                        (managerView.iosBundle, BuildTarget.iOS),
                        (managerView.androidBundle, BuildTarget.Android),
                        (managerView.webglBundle, BuildTarget.WebGL),
                    };
                    foreach (var (line, buildTarget) in tuple)
                    {
                        var path = envPaths.finalBundleDict[buildTarget];
                        if (File.Exists(path))
                        {
                            line.value = path;
                        }
                        else
                        {
                            line.value = "未构建";
                        }
                    }
                });
            }
        }

        protected override void Setup()
        {
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
            view.managerView.Apply((managerView) =>
            {
                managerView.cancelBtn.clicked += () =>
                {
                    state.folder = "";
                    Refresh();
                };
                foreach (var bundleLine in new[] {managerView.bundleManifest, managerView.iosBundle, managerView.androidBundle, managerView.webglBundle})
                {
                    bundleLine.SetEnabled(false);
                }
                managerView.openFolder.clicked+=() =>
                {
                    var path = managerView.bundleManifest.value;
                    if (File.Exists(path))
                    {
                        EditorUtility.RevealInFinder(path);
                    }
                    else
                    {
                        if (!Directory.Exists(NianxieConst.MiniBundlesOutput))
                        {
                            Directory.CreateDirectory(NianxieConst.MiniBundlesOutput);
                        }
                        EditorUtility.RevealInFinder(NianxieConst.MiniBundlesOutput);
                        Debug.LogError($"项目未构建{path}");
                    }
                };
                managerView.executeBuild.clicked+=()=>
                {
                    if (EditorUtility.DisplayDialog("确认打包？", $"确认打包{state.folder}", "确认", "取消"))
                    {
                        var arr = new[]
                        {
                            (managerView.iosBuild.value, BuildTarget.iOS),
                            (managerView.androidBuild.value, BuildTarget.Android),
                            (managerView.webglBuild.value, BuildTarget.WebGL),
                        };
                        var targets = arr.Where(x=>x.Item1).Select(x=>x.Item2).ToArray();
                        ExecuteBuild(state.folder, targets);
                        Refresh();
                    }
                };
                managerView.executePack.clicked+=()=>
                {
                    ExecutePack(state.folder);
                };
                managerView.gotoUpload.clicked+=()=>
                {
                    AccountWindow.OpenAccountWindowForUploadFolder(state.folder);
                };
            });
            Refresh();
        }

        private static void ExecuteBuild(string folder, BuildTarget[] targets)
        {
            var envPaths = MiniEditorEnvPaths.GetOrCreate(folder);
            envPaths.Build(targets);
        }

        private static void ExecutePack(string folder)
        {
            var envPaths = MiniEditorEnvPaths.GetOrCreate(folder);
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
                var miniEnvPaths = MiniEditorEnvPaths.GetOrCreate(folder);
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
