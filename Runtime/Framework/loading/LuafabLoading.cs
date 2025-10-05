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
        private LuaTable lazyTask;
        private WarmedReflectClass warmedReflect { get; }
        private ICacheLoader cacheLoader;

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
            return RawFork(parent).luaTable;
        }
        
        [BlackList]
        public LuaBehaviour RawFork(Transform parent)
        {
            return Object.Instantiate(luaBehav, parent, false);
        }
        
        [HintReturn(typeof(LuaTable), true)]
        public LuaTable WithLazyTask() {
            if (lazyTask == null)
            {
                Start();
                var gameManager = cacheLoader.GetGameManager();
                gameManager.reflectEnv.WrapLuaTaskOut(UniTask.Create(async()=>
                {
                    await LoadAsync();
                    return this;
                }), out lazyTask);
            }
            return lazyTask;
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
                    foreach (var assetPath in assetInjection.EachAssetPath())
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
            var luaChildren = go.GetComponentsInChildren<LuaBehaviour>(true);
            foreach (var child in luaChildren)
            {
                child.gameManager = cacheLoader.GetGameManager();
                // prefab中通过节点引用的prefab也需要处理lua依赖的加载
                var childLuafabPath = luaEnv.envPaths.classPath2luafabPath(child.classPath);
                if (childLuafabPath != resPath)
                {
                    var childLoading = cacheLoader.CacheLuafabLoading(childLuafabPath, false);
                    if (!childLoading.Done)
                    {
                        taskList.Add(childLoading.WaitTask);
                    }
                }
                else if(child.nestedKeys.Length>0)
                {
                    var reflectInfo = luaEnv.GetWarmedReflect(child.classPath, child.nestedKeys);
                    addLoadTaskByReflect(taskList, reflectInfo);
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