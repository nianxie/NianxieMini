using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZXing;
using ZXing.QrCode;

namespace Nianxie.Editor
{
    public class AccountWindow: EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;
        private VisualElement root => rootVisualElement;
        private const string WND_NAME = "创作管理";
        [MenuItem("念写Mini/"+WND_NAME, false, 1)]
        public static void OpenAccountWindow()
        {
            AccountWindow wnd = GetWindow<AccountWindow>(nameof(AccountWindow), true);
            wnd.titleContent = new GUIContent(WND_NAME);
            wnd.minSize = new Vector2(500, 500);
        }

        private AccountSignPage signPage;
        private AccountDataPage dataPage;

        class View : EasyHierarchy
        {
            public VisualElement signPage;
            public VisualElement dataPage;
        }

        private View view;
        private bool signed => AccountController.signed;

        private void refresh()
        {
            signPage.SetDisplay(!signed);
            dataPage.SetDisplay(signed);
            signPage.Refresh();
            dataPage.Refresh();
        }

        public void CreateGUI()
        {
            m_VisualTreeAsset.CloneTree(root);
            view = EasyHierarchy.CreateByQuery<View>(root);
            signPage = EasyViewModel.CreateViewModelAsNode<AccountSignPage>(view.signPage).Setup(() => { 
                dataPage.ScheduleRemoteRefresh();
                refresh();
            });
            dataPage = EasyViewModel.CreateViewModelAsNode<AccountDataPage>(view.dataPage).Setup(() => { 
                refresh();
            });
            refresh();
            //root.Query<VisualElement>("QRCode").First().style.backgroundImage = new StyleBackground(qrCodeTexture);
        }
    }
}
