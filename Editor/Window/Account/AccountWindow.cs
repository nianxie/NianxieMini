using System.IO;
using System.Text.RegularExpressions;
using System.Web.UI;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Networking;
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
            wnd.state.folder = folder;
            wnd.view.signedView.folderDropdown.SetValueWithoutNotify(folder);
            wnd.Refresh();
        }
        
        [SerializeField]
        private VisualTreeAsset uxmlItemAsset = default;

        public class ItemView:EasyHierarchy<ItemView>
        {
            public VisualElement kindGame;
            public VisualElement kindCraft;
            public Button deleteBtn;
            public Label miniName;
        }

        public ItemView[] itemViews;
        
        public class View : EasyHierarchy<View>
        {
            public class SigninView : EasyHierarchy<SigninView>
            {
                public Button qrCodeBtn;
                public Button accountBtn;
                public TextField accountInput;
                public Button signinBtn;
                public VisualElement accountPanel;
                public VisualElement qrCodePanel;
            }

            public class SignedView : EasyHierarchy<SignedView>
            {
                public class UploadView: EasyHierarchy<UploadView>
                {
                    public Button cancelBtn;
                    public TextField nameField;
                    public TextField iosBundle;
                    public TextField androidBundle;
                    public TextField webglBundle;
                    public Button thumbnailBtn;
                    public Button uploadBtn;
                }
                public UploadView uploadView;
                public Button signoutBtn;
                public DropdownField folderDropdown;

                public class ListView : EasyHierarchy<ListView>
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
            public string folder = "";
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

            public AccountMiniItemPagination page = new();
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
                    self.folderDropdown.choices = BuildMiniWindow.ListProjectFolders();
                    if (string.IsNullOrEmpty(state.folder))
                    {
                        self.uploadView.SetDisplay(false);
                    }
                    else
                    {
                        var envPaths = MiniEditorEnvPaths.Get(state.folder);
                        self.uploadView.SetDisplay(true);
                        self.uploadView.Apply((uploadView) =>
                        {
                            uploadView.thumbnailBtn.style.backgroundImage = new StyleBackground
                            {
                                value=Background.FromTexture2D(state.thumbnail)
                            };
                            var tuple = new[]
                            {
                                (uploadView.iosBundle, BuildTarget.iOS),
                                (uploadView.androidBundle, BuildTarget.Android),
                                (uploadView.webglBundle, BuildTarget.WebGL),
                            };
                            foreach (var (bundleField, buildTarget) in tuple)
                            {
                                var path = envPaths.finalBundleDict[buildTarget];
                                if (File.Exists(path))
                                {
                                    bundleField.value = path;
                                }
                                else
                                {
                                    bundleField.value = "未构建";
                                }
                            }
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
                            var itemView = EasyHierarchy.CreateByQuery<ItemView>(listView.container[index]);
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
                    state.folder = e.newValue;
                    state.thumbnail = null;
                    Refresh();
                });
                self.uploadView.Apply((uploadView) =>
                {
                    uploadView.iosBundle.SetEnabled(false);
                    uploadView.androidBundle.SetEnabled(false);
                    uploadView.webglBundle.SetEnabled(false);
                    uploadView.cancelBtn.clicked+=()=>{
                        self.folderDropdown.SetValueWithoutNotify("");
                        state.folder = "";
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
                });
            });
            Refresh();
            state.page.NavPage(1, Refresh);
        }

        private void Upload()
        {
            if (EditorUtility.DisplayDialog("确认上传？", $"确认上传《{name}》", "确认", "取消"))
            {
                UniTask.Create(async () =>
                {
                    try
                    {
                        var envPaths = MiniEditorEnvPaths.Get(state.folder);
                        await AccountController.UploadBundle(null, envPaths, (name, progress, total) =>
                        {
                            EditorUtility.DisplayProgressBar("上传文件", $"{progress}/{total} {name}", (progress*1.0f)/total);
                        });
                    } finally {
                        EditorUtility.ClearProgressBar();
                    }
                    state.page.NavPage(1, Refresh);
                });
            }
        }

        private void Delete(DB_Mini mini)
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
