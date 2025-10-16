using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Nianxie.Utils;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZXing;
using ZXing.QrCode;

namespace Nianxie.Editor
{
    public class AccountDataPage: EasyViewModel
    {
        public enum CreatingKind
        {
            NONE=0,
            GAME=1,
            CRAFT=2,
        }

        public class OutViewHierarchy:EasyHierarchy
        {
            public MiddleViewHierarchy middleView;
            public ScrollView scrollView;
            public Button signoutBtn;
            public Button refreshBtn;
        }

        public class MiddleViewHierarchy : EasyHierarchy
        {
            public VisualElement createBtns;
            public Button createGameBtn;
            public Button createCraftBtn;
            public VisualElement createForm;
            public Button cancelBtn;
            public TextField miniName;
            public Button submitBtn;
            public Toggle craft;
        }

        public OutViewHierarchy outView;
        public MiddleViewHierarchy middleView => outView.middleView;
        public List<AccountMiniItem> items = new();
        private const int SELECT_NOTHING = -1;
        public int selectIndex = SELECT_NOTHING;
        public CreatingKind creating = CreatingKind.NONE;

        public AccountDataPage Setup(Action onSignout)
        {
            outView = EasyHierarchy.CreateByQuery<OutViewHierarchy>(self);
            outView.signoutBtn.clicked += () =>
            {
                AccountController.Signout();
                onSignout();
            };
            outView.refreshBtn.clicked += () =>
            {
                UniTask.Create(async () =>
                {
                    await AccountController.RefreshList();
                    Refresh();
                });
            };
            middleView.craft.SetEnabled(false);
            middleView.createGameBtn.clicked += () =>
            {
                creating = CreatingKind.GAME;
                Refresh();
            };
            middleView.createCraftBtn.clicked += () =>
            {
                creating = CreatingKind.CRAFT;
                Refresh();
            };
            middleView.cancelBtn.clicked += () =>
            {
                creating = CreatingKind.NONE;
                Refresh();
            };
            middleView.submitBtn.clicked += () =>
            {
                UniTask.Create(async () =>
                {
                    await AccountController.CreateMini(middleView.miniName.value, false);
                    /*
                    var srcPath = NianxieConst.TemplateSimpleGame;
                    var dstPath = $"{NianxieConst.MiniPrefixPath}/{middleView.miniName.value}";
                    if (!Directory.Exists(NianxieConst.MiniPrefixPath))
                    {
                        Directory.CreateDirectory(NianxieConst.MiniPrefixPath);
                    }

                    if (AssetDatabase.CopyAsset(srcPath, dstPath))
                    {
                        Debug.LogError("TODO update config after created");
                    }
                    else
                    {
                        Debug.LogError($"project create error: copy maybe fail {srcPath} -> {dstPath}");
                    }
                    Refresh();*/
                    creating = CreatingKind.NONE;
                    selectIndex = SELECT_NOTHING;
                    Refresh();
                }).Forget();
            };
            ScheduleRemoteRefresh();
            Refresh();
            return this;
        }

        public void ScheduleRemoteRefresh()
        {
            UniTask.Create(async () =>
            {
                await AccountController.RefreshList();
                Refresh();
            }).Forget();
        }

        public void Refresh()
        {
            middleView.createBtns.SetDisplay(creating == CreatingKind.NONE);
            middleView.createForm.SetDisplay(creating != CreatingKind.NONE);
            middleView.craft.value = creating == CreatingKind.CRAFT;
            for (int i = 0; i < AccountController.miniStateDatas.Count; i++)
            {
                AccountMiniItem curItemView;
                if (i >= items.Count)
                {
                    var element = new VisualElement();
                    curItemView = EasyViewModel.CreateViewModelAsChild<AccountMiniItem>(element);
                    outView.scrollView.Add(curItemView.self);
                    items.Add(curItemView);
                    curItemView.Setup((index) =>
                    {
                        if (selectIndex == index)
                        {
                            selectIndex = SELECT_NOTHING;
                        }
                        else
                        {
                            selectIndex = index;
                        }
                        Refresh();
                    }, Refresh);
                }
                else
                {
                    curItemView = items[i];
                }
                curItemView.Refresh(i==selectIndex, i, AccountController.miniStateDatas[i]);
            }

            for (int i = items.Count-1; i >= AccountController.miniStateDatas.Count; i--)
            {
                items[i].RemoveSelf();
                items.RemoveAt(i);
            }
        }

    }
}
