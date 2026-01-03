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

            public void Init(UnityAction onBack)
            {
                backBtn.onClick.AddListener(onBack);
            }

            public void Show()
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

            public void Hide()
            {
                playCanvasGo.SetActive(false);
                videoPlayer.Stop();
                videoPlayer.gameObject.SetActive(false);
            }
        }

        [Serializable]
        public class MenuCanvas
        {
            [SerializeField]
            private GameObject menuCanvasGo;
            [SerializeField]
            private RectTransform playMenu;
            [SerializeField]
            private RectTransform editMenu;
            [SerializeField]
            private PreviewMiniButtons miniBtnPrefab;
            [SerializeField]
            private Toggle craft;
            private static PreviewMiniInfo[] ListMiniInfo()
            {
                var folderList = Directory.EnumerateDirectories(NianxieConst.MiniPrefixPath).Select((e) => new DirectoryInfo(e).Name).ToList();
                return folderList.Select(e => new PreviewMiniInfo(e)).ToArray();
            }
            public void Init(Action<string, string> loadPlay, Action<string, string> loadEdit)
            {
                var miniInfoList = ListMiniInfo();
                var playCount = miniInfoList.Length;
                var editCount = miniInfoList.Count(e => e.config.craftable);
                foreach (var miniInfo in miniInfoList)
                {
                    var playBtn = UnityEngine.Object.Instantiate(miniBtnPrefab, playMenu);
                    playBtn.gameObject.SetActive(true);
                    playBtn.Main(loadPlay, miniInfo);
                    if (miniInfo.config.craftable)
                    {
                        var editBtn = UnityEngine.Object.Instantiate(miniBtnPrefab, editMenu);
                        editBtn.gameObject.SetActive(true);
                        editBtn.Main(loadEdit, miniInfo);
                    }
                }

                playMenu.sizeDelta = new Vector2(0, playCount*200);
                playMenu.gameObject.SetActive(true);
                editMenu.sizeDelta = new Vector2(0, editCount*200);
                editMenu.gameObject.SetActive(false);
                craft.onValueChanged.AddListener((e) =>
                {
                    playMenu.gameObject.SetActive(!e);
                    editMenu.gameObject.SetActive(e);
                });
            }
            public void Show()
            {
                menuCanvasGo.SetActive(true);
            }
            public void Hide()
            {
                menuCanvasGo.SetActive(false);
            }
        }

        [SerializeField]
        private PlayCanvas playCanvas;
        [SerializeField]
        private MenuCanvas menuCanvas;
        
        [SerializeField]
        private PreviewEditView editViewPrefab;

        private PreviewGame previewGame;
        private static PreviewMiniInfo[] ListMiniInfo()
        {
            var folderList = Directory.EnumerateDirectories(NianxieConst.MiniPrefixPath).Select((e) => new DirectoryInfo(e).Name).ToList();
            return folderList.Select(e => new PreviewMiniInfo(e)).ToArray();
        }
        void Awake()
        {
            menuCanvas.Init(LoadPlay, LoadEdit);
            menuCanvas.Show();
            playCanvas.Init(Unload);
            playCanvas.Hide();
        }

        private void LoadPlay(string folder, string bundlePath)
        {
            UnityEngine.Assertions.Assert.IsNull(previewGame, "game is existed");
            menuCanvas.Hide();
            playCanvas.Show();
            PreviewGame.PlayGame playGame;
            if (string.IsNullOrEmpty(bundlePath))
            {
                playGame = new PreviewGame.PlayGame(folder);
            }
            else
            {
                var bundle = AssetBundle.LoadFromFile(bundlePath);
                playGame = new PreviewGame.PlayGame(bundle);
            }
            previewGame = playGame;
            UniTask.Create(async () =>
            {
                await playGame.Main(playCanvas.PlayEnding);
            }).Forget();
        }
        private void LoadEdit(string folder, string bundlePath)
        {
            UnityEngine.Assertions.Assert.IsNull(previewGame, "game is existed");
            menuCanvas.Hide();
            playCanvas.Show();

            void Open(PreviewGame.EditReopenArgs args)
            {
                PreviewGame.EditGame editGame;
                if (string.IsNullOrEmpty(bundlePath))
                {
                    editGame = new PreviewGame.EditGame(folder);
                }
                else
                {
                    var bundle = AssetBundle.LoadFromFile(bundlePath);
                    editGame = new PreviewGame.EditGame(bundle);
                }
                previewGame = editGame;
                UniTask.Create(async () =>
                {
                    await editGame.Main(EditViewMaker, args, (reopenArgs) =>
                    {
                        previewGame.Unload();
                        previewGame = null;
                        Open(reopenArgs);
                    });
                }).Forget();
            }
            Open(null);
        }

        private PreviewEditView EditViewMaker(Transform transform)
        {
            return Instantiate(editViewPrefab, transform);
        }

        private void Unload()
        {
            menuCanvas.Show();
            playCanvas.Hide();
            if (previewGame != null)
            {
                previewGame.Unload();
                previewGame = null;
            }
        }
    }
}
