using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Cysharp.Threading.Tasks;
using Nianxie.Preview;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Nianxie.Preview
{
    public class PreviewManager: MonoBehaviour
    {
        public RectTransform menuRect;
        public RectTransform btnTpl;
        public Button backBtn;
        public Toggle craftToggle;

        public PreviewBridge previewBridge;
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

        public void LoadProject(string miniId, string bundlePath)
        {
            menuRect.gameObject.SetActive(false);
            backBtn.gameObject.SetActive(true);
            previewBridge = gameObject.AddComponent<PreviewBridge>();
            previewBridge.Main(this, miniId, bundlePath);
        }

        public async UniTask<(Scene, MiniGameManager)> LoadScene()
        {
            LoadSceneParameters param = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.None);
            await SceneManager.LoadSceneAsync(NianxieConst.MiniSceneName, param);
            var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            var objList = scene.GetRootGameObjects();
            var miniManager = objList[0].GetComponent<MiniGameManager>();
            return (scene, miniManager);
        }

        public void Unload()
        {
            menuRect.gameObject.SetActive(true);
            backBtn.gameObject.SetActive(false);
            if (previewBridge != null)
            {
                Destroy(previewBridge);
                previewBridge = null;
            }
        }
    }
}
