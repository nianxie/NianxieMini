using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using XLua;

namespace Nianxie.Utils
{
    public static class SceneAsyncUtility
    {
        // unity中，scene的异步加载不返回scene的对象，需要单独GetSceneAt获取，以异步的方式调用两次时或可能导致拿到的scene不一致，所以这里使用lock强行保护一下
        private static bool sceneLocked = false;
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
    }
}
