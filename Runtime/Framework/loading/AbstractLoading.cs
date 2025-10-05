using System;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using XLua;
using Debug = UnityEngine.Debug;

namespace Nianxie.Framework
{

    // 使用UniTask的TaskSource包裹asset的加载，因为AssetHandle类无法支持互相等待功能
    [BlackList]
    public abstract class AbstractLoading<T>
    {
        enum LoadingStep
        {
            IDLE = 0,
            START = 1,
            DONE = 2,
        }
        
        public bool Done => loadingStep == LoadingStep.DONE;

        private LoadingStep loadingStep = LoadingStep.IDLE;
        private UniTaskCompletionSource<T> taskSource;
        protected string resPath;

        public AbstractLoading(string _resPath)
        {
            resPath = _resPath;
            taskSource = new UniTaskCompletionSource<T>();
        }

        public UniTask<T> WaitTask => taskSource.Task;

        protected abstract UniTask<T> LoadAsync();

        public void Start()
        {
            if (loadingStep != LoadingStep.IDLE)
            {
                return;
            }
            loadingStep = LoadingStep.START;
            UniTask.Create(async()=>{
                var watch = Stopwatch.StartNew();
                try
                {
                    var retHandle = await LoadAsync();
                    loadingStep = LoadingStep.DONE;
                    taskSource.TrySetResult(retHandle);
                    watch.Stop();
                    Debug.Log($"yoo asset load {resPath} time {watch.ElapsedMilliseconds}");
                }
                catch (Exception e)
                {
                    loadingStep = LoadingStep.DONE;
                    watch.Stop();
                    Debug.LogError($"yoo asset load failed {resPath} time {watch.ElapsedMilliseconds}");
                    taskSource.TrySetException(e);
                }
            }).Forget();
        }
    }
}