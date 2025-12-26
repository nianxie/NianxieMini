using System;
using System.IO;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Nianxie.Utils;
using Nianxie.Preview;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZXing;
using ZXing.QrCode;

namespace Nianxie.Editor
{
    public class AccountWindow: EasyWindow<AccountWindow.View, AccountWindow.State>
    {
        private const string WND_NAME = "账号作品管理";
        [MenuItem("念写Mini/"+WND_NAME, false, 2)]
        public static void OpenAccountWindow()
        {
            AccountWindow wnd = GetWindow<AccountWindow>(WND_NAME, true);
            wnd.titleContent = new GUIContent(WND_NAME);
            wnd.minSize = new Vector2(500, 500);
        }
        
        public static void OpenAccountWindowForUploadFolder(string folder)
        {
            AccountWindow wnd = GetWindow<AccountWindow>(WND_NAME, true);
            wnd.titleContent = new GUIContent(WND_NAME);
            wnd.minSize = new Vector2(500, 500);
            wnd.state.showUpload = true;
            wnd.state.selectFolder = folder;
            wnd.view.signedView.folderDropdown.SetValueWithoutNotify(folder);
            wnd.Refresh();
        }
        
        [SerializeField]
        private VisualTreeAsset uxmlItemAsset = default;

        public class ItemView:EasyView<ItemView>
        {
            public VisualElement kindGame;
            public VisualElement kindCraft;
            public Button deleteBtn;
            public Label miniName;
        }

        public ItemView[] itemViews;
        
        public class View : EasyView<View>
        {
            public class SigninView : EasyView<SigninView>
            {
                public Button qrCodeBtn;
                public Button accountBtn;
                public TextField accountInput;
                public Button signinBtn;
                public VisualElement accountPanel;
                public VisualElement qrCodePanel;
            }

            public class SignedView : EasyView<SignedView>
            {
                public class UploadView: EasyView<UploadView>
                {
                    public Button cancelBtn;
                    public TextField nameField;
                    public VisualElement kindGame;
                    public VisualElement kindCraft;
                    public TextField miniVersion;
                    public TextField unityVersion;
                    public TextField iosBundle;
                    public TextField androidBundle;
                    public TextField webglBundle;
                    public Button thumbnailBtn;
                    public Button uploadBtn;
                    public Button openFolder;
                }
                public UploadView uploadView;
                public Button signoutBtn;
                public DropdownField folderDropdown;

                public class ListView : EasyView<ListView>
                {
                    public Button nextBtn;
                    public Button prevBtn;
                    public Label pageLabel;
                    public VisualElement container;
                }
                public ListView listView;
            }

            public SigninView signinView;
            public SignedView signedView;
        }
        public class State: EasyState
        {
            public enum LoginKind
            {
                Account = 1,
                QRCode = 2,
            }

            public LoginKind loginKind = LoginKind.Account;
            public bool showUpload = false;
            // folder
            private string _selectFolder = "";
            public MiniBundleManifest selectManifest { get; private set; }
            public MiniEditorEnvPaths envPaths { get; private set; }
            public string selectFolder
            {
                get { return _selectFolder; }
                set
                {
                    if (_selectFolder != value)
                    {
                        _selectFolder = value;
                        envPaths = null;
                        selectManifest = null;
                        if (!string.IsNullOrEmpty(value))
                        {
                            envPaths = MiniEditorEnvPaths.Get(value);
                            if (File.Exists(envPaths.finalManifest))
                            {
                                try
                                {
                                    var jsonBytes = File.ReadAllBytes(envPaths.finalManifest);
                                    selectManifest = MiniBundleManifest.FromJson(jsonBytes);
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError($"{envPaths.finalManifest}文件异常，请重新构建");
                                }
                            }
                        }
                    }
                }
            }
            // 缩略图
            private Texture2D _thumbnail;
            public Texture2D thumbnail
            {
                get { return _thumbnail; }
                set
                {
                    if (_thumbnail != null)
                    {
                        DestroyImmediate(_thumbnail);
                    }
                    _thumbnail = value;
                }
            }
            public bool signed => AccountController.signed;

            public AccountMiniPagination page = new();
        }

        protected override void Refresh()
        {
            if (state.signed)
            {
                view.signinView.SetDisplay(false);
                view.signedView.SetDisplay(true);
                view.signedView.Apply((self) =>
                {
                    self.uploadView.SetDisplay(state.showUpload);
                    self.folderDropdown.choices = ProjectWindow.ListProjectFolders();
                    if (string.IsNullOrEmpty(state.selectFolder))
                    {
                        self.uploadView.SetDisplay(false);
                    }
                    else
                    {
                        self.uploadView.SetDisplay(true);
                        self.uploadView.Apply((uploadView) =>
                        {
                            bool okay = true;
                            var tuple = new[]
                            {
                                (uploadView.iosBundle, BuildTarget.iOS),
                                (uploadView.androidBundle, BuildTarget.Android),
                                (uploadView.webglBundle, BuildTarget.WebGL),
                            };
                            foreach (var (bundleField, buildTarget) in tuple)
                            {
                                var path = state.envPaths.finalBundleDict[buildTarget];
                                if (File.Exists(path))
                                {
                                    bundleField.value = path;
                                }
                                else
                                {
                                    bundleField.value = "未构建";
                                    okay = false;
                                }
                            }
                            var config = state.selectManifest?.config;
                            if (config != null)
                            {
                                uploadView.nameField.value = config.name;
                                uploadView.kindCraft.SetDisplay(config.craftable);
                                uploadView.kindGame.SetDisplay(!config.craftable);
                                uploadView.miniVersion.value = config.miniVersion;
                                uploadView.unityVersion.value = config.unityVersion;
                                uploadView.uploadBtn.SetEnabled(okay);
                            }
                            else
                            {
                                uploadView.nameField.value = "";
                                uploadView.kindCraft.SetDisplay(false);
                                uploadView.kindGame.SetDisplay(false);
                                uploadView.uploadBtn.SetEnabled(false);
                                uploadView.miniVersion.value = "";
                                uploadView.unityVersion.value = "";
                            }

                            uploadView.thumbnailBtn.style.backgroundImage = new StyleBackground
                            {
                                value=Background.FromTexture2D(state.thumbnail)
                            };
                        });
                    }
                    self.listView.Apply((listView) =>
                    {
                        listView.container.Clear();
                        var itemDatas = state.page.miniItems;
                        itemViews = new ItemView[itemDatas.Length];
                        for (int i = 0; i < itemDatas.Length; i++)
                        {
                            var mini = itemDatas[i];
                            uxmlItemAsset.CloneTree(listView.container, out int index, out _);
                            var itemView = EasyView.CreateByQuery<ItemView>(listView.container[index]);
                            itemView.deleteBtn.clicked += () =>
                            {
                                Delete(mini);
                            };
                            itemView.kindCraft.SetDisplay(mini.craftable);
                            itemView.kindGame.SetDisplay(!mini.craftable);
                            itemView.miniName.text = mini.name;
                            itemViews[i] = itemView;
                        }
                    });
                });
            }
            else
            {
                view.signinView.SetDisplay(true);
                view.signedView.SetDisplay(false);
            }
            /*signPage.SetDisplay(!signed);
            dataPage.SetDisplay(signed);
            signPage.Refresh();
            dataPage.Refresh();*/
        }

        protected override void Setup()
        {
            view.signinView.Apply((self) =>
            {
                self.qrCodeBtn.clicked+=()=>
                {
                    state.loginKind = State.LoginKind.QRCode;
                    Refresh();
                };
                self.accountBtn.clicked+=()=>
                {
                    state.loginKind = State.LoginKind.Account;
                    Refresh();
                };
                self.accountInput.RegisterValueChangedCallback((e) =>
                {
                    if (e.newValue.EndsWith("\n"))
                    {
                    }
                    if (!Regex.IsMatch(e.newValue, @"^[a-zA-Z0-9_]*$"))
                    {
                        self.accountInput.SetValueWithoutNotify(e.previousValue);
                    }
                });
                self.signinBtn.clicked+=()=>
                {
                    if (!AccountController.signinRunning)
                    {
                        UniTask.Create(async () =>
                        {
                            await AccountController.Signin(self.accountInput.value);
                            Refresh();
                            state.page.NavPage(1, Refresh);
                        });
                    }
                };
            });
            view.signedView.Apply((self) =>
            {
                self.signoutBtn.clicked+=()=>
                {
                    AccountController.Signout();
                    Refresh();
                };
                self.folderDropdown.RegisterValueChangedCallback((e) =>
                {
                    state.selectFolder = e.newValue;
                    state.thumbnail = null;
                    Refresh();
                });
                self.uploadView.Apply((uploadView) =>
                {
                    uploadView.miniVersion.SetEnabled(false);
                    uploadView.unityVersion.SetEnabled(false);
                    uploadView.iosBundle.SetEnabled(false);
                    uploadView.androidBundle.SetEnabled(false);
                    uploadView.webglBundle.SetEnabled(false);
                    uploadView.cancelBtn.clicked+=()=>{
                        self.folderDropdown.SetValueWithoutNotify("");
                        state.selectFolder = "";
                        Refresh();
                    };
                    uploadView.thumbnailBtn.clicked += () =>
                    {
                        var thumbnailUrl = EditorUtility.OpenFilePanel("选择封面", "./", "png, jpg, jpeg");
                        var tex = new Texture2D(1, 1);
                        var imgData = File.ReadAllBytes(thumbnailUrl);
                        tex.LoadImage(imgData);
                        state.thumbnail = tex;
                        Refresh();
                    };
                    uploadView.uploadBtn.clicked += () =>
                    {
                        Upload();
                    };
                    uploadView.openFolder.clicked += () =>
                    {
                        var path = state.envPaths.finalManifest;
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
                });
            });
            Refresh();
            state.page.NavPage(1, Refresh);
        }

        private void Upload()
        {
            if (state.selectManifest == null)
            {
                Debug.LogError("上传异常，请重新构建");
                return;
            }
            var message = $"确认上传《{view.signedView.uploadView.nameField.value}》?\n\n" + (state.thumbnail==null?"注意，未选择封面":"");
            if (EditorUtility.DisplayDialog("确认上传？", message, "确认", "取消"))
            {
                UniTask.Create(async () =>
                {
                    try
                    {
                        await AccountController.UploadBundle(null, state.selectManifest.config, state.envPaths, (fileName, progress, total) =>
                        {
                            EditorUtility.DisplayProgressBar("上传文件", $"{progress}/{total} {fileName}", (progress*1.0f)/total);
                        });
                    } finally {
                        EditorUtility.ClearProgressBar();
                    }
                    state.page.NavPage(1, Refresh);
                }).Forget();
            }
        }

        private void Delete(DB_NxMini mini)
        {
            if (EditorUtility.DisplayDialog("确认删除？", $"确认删除《{mini.name}》吗?", "确认", "取消"))
            {
                UniTask.Create(async () =>
                {
                    await AccountController.DeleteMini(mini.miniId);
                    state.page.NavPage(1, Refresh);
                }).Forget();
            }
        }

        private void OnDestroy()
        {
            state.thumbnail = null;
        }
    }
}
