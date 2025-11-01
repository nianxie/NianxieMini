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
using UnityEngine.Video;
using XLua;

namespace Nianxie.Preview
{
    public class PreviewManager: MonoBehaviour
    {
        public RectTransform menuRect;
        public RectTransform btnTpl;
        public Button backBtn;
        public VideoPlayer videoPlayer;
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
            videoPlayer.gameObject.SetActive(false);
            if (string.IsNullOrEmpty(bundlePath))
            {
                previewBridge = new PreviewBridge(previewGizmos, folder, PlayEnding);
            }
            else
            {
                var bundle = AssetBundle.LoadFromFile(bundlePath);
                previewBridge = new PreviewBridge(previewGizmos, bundle, PlayEnding);
            }
            UniTask.Create(async () =>
            {
                await previewBridge.Main(editCraft);
            }).Forget();
        }

        public void PlayEnding(string previewVideoUrl)
        {
            if (string.IsNullOrEmpty(previewVideoUrl))
            {
                Debug.Log("假装播放一下结束视频, 如果想预览一下结束视频，可以配置config.txt中的previewVideoUrl（注意，该值仅用于开发）");
            }
            else
            {
                videoPlayer.url = previewVideoUrl;
            }
            videoPlayer.gameObject.SetActive(true);
            videoPlayer.Play();
        }

        public void Unload()
        {
            videoPlayer.Stop();
            videoPlayer.gameObject.SetActive(false);
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
