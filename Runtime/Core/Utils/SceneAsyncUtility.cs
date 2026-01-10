using System;
using Cysharp.Threading.Tasks;
using Nianxie.Framework;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using XLua;
using UnityEngine;

namespace Nianxie.Utils
{
    public static class SceneAsyncUtility
    {
        // unity中，scene的异步加载不返回scene的对象，需要单独GetSceneAt获取，以异步的方式调用两次时或可能导致拿到的scene不一致，所以这里使用lock强行保护一下
        private static bool sceneLocked = false;
        [BlackList]
        public static async UniTask<Scene> LoadSceneAsync(string sceneName)
        {
            if (sceneLocked)
            {
                await UniTask.WaitUntil(() => !sceneLocked);
            }
            try
            {
                LoadSceneParameters param = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.None);
                await SceneManager.LoadSceneAsync(sceneName, param);
            }
            catch (Exception)
            {
                Debug.LogError($"load scene '{sceneName}' failed");
                sceneLocked = false;
                throw;
            }
            var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            sceneLocked = false;
            return scene;
        }
        [BlackList]
        public static async UniTask UnloadSceneAsync(Scene scene)
        {
            if (sceneLocked)
            {
                await UniTask.WaitUntil(() => !sceneLocked);
            }
            try
            {
                await SceneManager.UnloadSceneAsync(scene);
            }
            catch (Exception e)
            {
                Debug.LogError($"Unload scene {scene} failed {e}");
                sceneLocked = false;
            }
            sceneLocked = false;
        }

        [BlackList]
        public static async UniTask<MiniGameManager> CreateMiniGameAsync(MiniBridge miniBridge)
        {
            MiniGameManager mini = null;
            try
            {
                var scene = await SceneAsyncUtility.LoadSceneAsync(NianxieConst.MiniSceneName);
                var objList = scene.GetRootGameObjects();
                mini = objList[0].GetComponent<MiniGameManager>();
                SceneManager.SetActiveScene(scene);
                Assert.IsTrue(objList.Length == 1, "mini scene's root object is not one and only one");
                await mini.Init(miniBridge);
            }
            catch (Exception e)
            {
                if(mini!=null) {
                    UnityEngine.Object.Destroy(mini);
                    Debug.LogError($"create mini failed");
                }
                throw;
            }
            return mini;
        }
    }
}
