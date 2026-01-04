using UnityEngine;
using Cysharp.Threading.Tasks;
using XLua;

namespace Nianxie.Framework
{
    public abstract class AbstractGameManager : MonoBehaviour
    {
        public RuntimeReflectEnv reflectEnv { get; private set; }
        // TODO 这里添加一个interface来包裹context的方法?
        public LuaTable context { get; private set; }
        public LuafabLoading rootLuafabLoading { get; private set; }

        protected async UniTask InitGameModule()
        {
            var moduleSequence = gameObject.GetComponents<AbstractGameModule>();
            // 1. module Init
            await UniTask.WhenAll(moduleSequence.Select(e => e.Init()));
            // 2. init lua env
            reflectEnv = CreateReflectEnv();
        }

        protected async UniTask PrepareContextAndRoot()
        {
            context = reflectEnv.CreateContext();
            // 加载root prefab
            rootLuafabLoading = reflectEnv.assetModule.AttachLuafabLoading(reflectEnv.envPaths.rootLuafabPath, false);
            await rootLuafabLoading.WaitTask;
        }

        protected void OnDestroy()
        {
            // 0. module PreInit
            var moduleSequence = gameObject.GetComponents<AbstractGameModule>();
            foreach (var module in moduleSequence)
            {
                module.Destroy().Forget();
            }
        }

        public abstract IAssetLoader GetAssetLoader();
        protected abstract RuntimeReflectEnv CreateReflectEnv();

        public virtual void OnInjectGameHelper(LuaTable script, string key, System.Type helperType)
        {
            Debug.LogError("OnInjectGameHelper not implement");
        }

        // Update 时执行tick
        void Update()
        {
            if (reflectEnv == null) return;
            reflectEnv.Tick();
        }
    }
}
