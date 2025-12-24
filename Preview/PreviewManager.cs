using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Nianxie.Preview
{
    public class PreviewManager: MonoBehaviour
    {
        public RectTransform menuRect;
        public RectTransform btnTpl;
        public Button backBtn;
        public VideoPlayer videoPlayer;
        public Toggle craftToggle;

        public PreviewEditGizmos previewGizmos;
        public PreviewGame previewGame;
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
                newRect.GetComponent<PreviewMiniButtons>().Main(LoadProject, project);
            }
        }

        private void LoadProject(string folder, string bundlePath)
        {
            menuRect.gameObject.SetActive(false);
            backBtn.gameObject.SetActive(true);
            videoPlayer.gameObject.SetActive(false);
            if (editCraft)
            {
                if (string.IsNullOrEmpty(bundlePath))
                {
                    previewGame = new PreviewGame.EditGame(previewGizmos, folder);
                }
                else
                {
                    var bundle = AssetBundle.LoadFromFile(bundlePath);
                    previewGame = new PreviewGame.EditGame(previewGizmos, bundle);
                }
                UniTask.Create(async () =>
                {
                    await previewGame.Main();
                }).Forget();
            }
            else
            {
                if (string.IsNullOrEmpty(bundlePath))
                {
                    previewGame = new PreviewGame.PlayGame(folder, PlayEnding);
                }
                else
                {
                    var bundle = AssetBundle.LoadFromFile(bundlePath);
                    previewGame = new PreviewGame.PlayGame(bundle, PlayEnding);
                }
                UniTask.Create(async () =>
                {
                    await previewGame.Main();
                }).Forget();
            }
        }

        private void PlayEnding(string previewVideoUrl)
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

        private void Unload()
        {
            videoPlayer.Stop();
            videoPlayer.gameObject.SetActive(false);
            menuRect.gameObject.SetActive(true);
            backBtn.gameObject.SetActive(false);
            if (previewGame != null)
            {
                previewGame.Unload();
                previewGame = null;
            }
        }
    }
}
