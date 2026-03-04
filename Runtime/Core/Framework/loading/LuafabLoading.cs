using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nianxie.Components;
using UnityEngine;
using XLua;

namespace Nianxie.Framework
{
    public class LuafabLoading : AbstractLoading<GameObject>
    {
        public LuaTable clsOpen { get; }
        private LuaBehaviour luaBehav;
        private LuaTable readyFuture;
        private WarmedReflectClass warmedReflect { get; }
        private ICacheLoader cacheLoader;
        private SpawnBehaviour[] spawnChildren;

        [BlackList]
        public LuafabLoading(string resPath, ICacheLoader cacheLoader) : base(resPath)
        {
            this.cacheLoader = cacheLoader;
            var luaEnv = cacheLoader.GetGameManager().reflectEnv;
            var classPath = luaEnv.envPaths.assetPath2classPath(resPath);
            warmedReflect = luaEnv.GetFileWarmedReflect(classPath);
            clsOpen = warmedReflect.clsOpen;
        }

        [HintReturn(typeof(LuaTable), false)]
        public LuaTable Fork(Transform parent)
        {
            return ForkBehav(parent).luaTable;
        }
        
        [BlackList]
        public LuaBehaviour ForkBehav(Transform parent)
        {
            foreach(var spawnChild in spawnChildren)
            {
                spawnChild.gameManager = cacheLoader.GetGameManager();
            }
            var newBehav = Object.Instantiate(luaBehav, parent, false);
            foreach(var spawnChild in spawnChildren)
            {
                spawnChild.gameManager = null;
            }
            return newBehav;
        }
        
        [HintReturn(typeof(LuafabLoading), true)]
        public LuaTable ReadyFuture() {
            if (readyFuture == null)
            {
                var reflectEnv = cacheLoader.GetGameManager().reflectEnv;
                readyFuture = reflectEnv.bootNewFuture.Func<LuaTable>();
                Start();
                reflectEnv.AsyncCompleteFuture(readyFuture, async () =>
                {
                    await LoadAsync();
                    return this;
                });
            }
            return readyFuture;
        }

        private void addLoadTaskByReflect(List<UniTask> taskList, WarmedReflectClass reflectInfo)
        {
            // 处理lua依赖的加载
            foreach (var injection in reflectInfo.injections)
            {
                if (injection is LuafabInjection luafabInjection && !luafabInjection.lazy)
                {
                    var childLoading = cacheLoader.CacheLuafabLoading(luafabInjection.assetPath, false);
                    if (!childLoading.Done)
                    {
                        taskList.Add(childLoading.WaitTask);
                    }
                }
                else if(injection is SubAssetInjection subAssetInjection)
                {
                    var childLoading = cacheLoader.CacheSubAssetsLoading(subAssetInjection.assetPath);
                    if (!childLoading.Done)
                    {
                        taskList.Add(childLoading.WaitTask);
                    }
                } else if (injection is AssetInjection assetInjection)
                {
                    foreach (var assetPath in assetInjection.assetPathList)
                    {
                        var childLoading = cacheLoader.CacheAssetLoading(assetPath, assetInjection.csharpType);
                        if (!childLoading.Done)
                        {
                            taskList.Add(childLoading.WaitTask);
                        }
                    }
                }
            }
        }

        protected override async UniTask<GameObject> LoadAsync()
        {
            var luaEnv = cacheLoader.GetGameManager().reflectEnv;
            var taskList = new List<UniTask>();
            var selfLoading = cacheLoader.CacheAssetLoading(resPath, typeof(GameObject));
            if (!selfLoading.Done)
            {
                taskList.Add(selfLoading.WaitTask);
            }
            // 处理lua依赖的加载
            addLoadTaskByReflect(taskList, warmedReflect);

            var go = (await selfLoading.WaitTask) as GameObject;
            spawnChildren = go.GetComponentsInChildren<SpawnBehaviour>(true);
            foreach (var spawnChild in spawnChildren)
            {
                if (spawnChild is LuaBehaviour luaChild)
                {
                    // prefab中通过节点引用的prefab也需要处理lua依赖的加载
                    var childLuafabPath = luaEnv.envPaths.classPath2luafabPath(luaChild.classPath);
                    if (childLuafabPath != resPath)
                    {
                        var childLoading = cacheLoader.CacheLuafabLoading(childLuafabPath, false);
                        if (!childLoading.Done)
                        {
                            taskList.Add(childLoading.WaitTask);
                        }
                    }
                    else if(luaChild.nestedKeys.Length>0)
                    {
                        var reflectInfo = luaEnv.GetWarmedReflect(luaChild.classPath, luaChild.nestedKeys);
                        addLoadTaskByReflect(taskList, reflectInfo);
                    }
                }
            }
            await UniTask.WhenAll(taskList);
            if (go.TryGetComponent<LuaBehaviour>(out var _behav))
            {
                luaBehav = _behav;
            }
            return go;
        }
    }
}