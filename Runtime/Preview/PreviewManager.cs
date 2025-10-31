using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XLua;

namespace Nianxie.Preview
{
    public class PreviewManager: MonoBehaviour
    {
        public RectTransform menuRect;
        public RectTransform btnTpl;
        public Button backBtn;
        public Toggle craftToggle;

        public PreviewGizmos previewGizmos;
        public PreviewBridge previewBridge;
        public MiniGameManager miniManager;
        public bool editCraft => craftToggle.isOn;
        public static List<string> ListProject()
        {
            return Directory.EnumerateDirectories(NianxieConst.MiniPrefixPath).Select((e) => new DirectoryInfo(e).Name).ToList();
        }
        void Awake()
        {
            var projectList = ListProject();
            backBtn.onClick.AddListener(Unload);
            for (int i = 0; i < projectList.Count; i++)
            {
                var newRect = UnityEngine.Object.Instantiate(btnTpl, menuRect);
                var pos = newRect.anchoredPosition;
                newRect.anchoredPosition = new Vector2(pos.x, pos.y-i*btnTpl.rect.height*2.2f);
                newRect.gameObject.SetActive(true);
                var project = projectList[i];
                newRect.GetComponent<PreviewMiniButtons>().Main(this, project);
            }
        }

        public void LoadProject(string folder, string bundlePath)
        {
            menuRect.gameObject.SetActive(false);
            backBtn.gameObject.SetActive(true);
            if (string.IsNullOrEmpty(bundlePath))
            {
                previewBridge = new PreviewBridge(previewGizmos, folder);
            }
            else
            {
                var bundle = AssetBundle.LoadFromFile(bundlePath);
                previewBridge = new PreviewBridge(previewGizmos, bundle);
            }
            UniTask.Create(async () =>
            {
                await previewBridge.Main(editCraft);
            }).Forget();
        }

        public void Unload()
        {
            menuRect.gameObject.SetActive(true);
            backBtn.gameObject.SetActive(false);
            if (previewBridge != null)
            {
                previewBridge.Unload();
                previewBridge = null;
            }
        }
    }
}
