using System;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Nianxie.Preview
{
    public class PreviewManager: MonoBehaviour
    {
        [Serializable]
        public class PlayCanvas
        {
            [SerializeField]
            private GameObject playCanvasGo;
            [SerializeField]
            private Button backBtn;
            [SerializeField]
            private VideoPlayer videoPlayer;

            public void RegisterOnBack(UnityAction onBack)
            {
                Debug.Log("hello");
                backBtn.onClick.AddListener(onBack);
            }

            public void Enter()
            {
                playCanvasGo.SetActive(true);
                videoPlayer.gameObject.SetActive(false);
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

            public void Leave()
            {
                playCanvasGo.SetActive(false);
                videoPlayer.Stop();
                videoPlayer.gameObject.SetActive(false);
            }
        }

        [SerializeField]
        private PlayCanvas playCanvas;
        
        [SerializeField]
        private RectTransform menuRect;
        
        [SerializeField]
        private PreviewMiniButtons miniBtnPrefab;
        [SerializeField]
        private PreviewEditView editViewPrefab;
        
        public Toggle craftToggle;

        public PreviewGame previewGame;
        public bool editCraft => craftToggle.isOn;
        public static PreviewMiniInfo[] ListMiniInfo()
        {
            var folderList = Directory.EnumerateDirectories(NianxieConst.MiniPrefixPath).Select((e) => new DirectoryInfo(e).Name).ToList();
            return folderList.Select(e => new PreviewMiniInfo(e)).ToArray();
        }
        void Awake()
        {
            var miniInfoList = ListMiniInfo();
            for (int i = 0; i < miniInfoList.Length; i++)
            {
                var newBtn = UnityEngine.Object.Instantiate(miniBtnPrefab, menuRect);
                newBtn.gameObject.SetActive(true);
                var miniInfo = miniInfoList[i];
                newBtn.Main(LoadProject, miniInfo);
            }
            playCanvas.RegisterOnBack(Unload);
        }

        private void LoadProject(string folder, string bundlePath)
        {
            menuRect.gameObject.SetActive(false);
            playCanvas.Enter();
            if (editCraft)
            {
                if (string.IsNullOrEmpty(bundlePath))
                {
                    previewGame = new PreviewGame.EditGame(folder, EditViewMaker);
                }
                else
                {
                    var bundle = AssetBundle.LoadFromFile(bundlePath);
                    previewGame = new PreviewGame.EditGame(bundle, EditViewMaker);
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
                    previewGame = new PreviewGame.PlayGame(folder, playCanvas.PlayEnding);
                }
                else
                {
                    var bundle = AssetBundle.LoadFromFile(bundlePath);
                    previewGame = new PreviewGame.PlayGame(bundle, playCanvas.PlayEnding);
                }
                UniTask.Create(async () =>
                {
                    await previewGame.Main();
                }).Forget();
            }
        }

        private PreviewEditView EditViewMaker(Transform transform)
        {
            return Instantiate(editViewPrefab, transform);
        }

        private void Unload()
        {
            playCanvas.Leave();
            menuRect.gameObject.SetActive(true);
            if (previewGame != null)
            {
                previewGame.Unload();
                previewGame = null;
            }
        }
    }
}
