using System;
using UnityEngine;

namespace Nianxie.Craft
{
    /// <summary>
    /// 资源的usage分为三种：
    /// 1. builtin，来源于prefab中
    /// 2. upload，来源于当前编辑期间上传
    /// 3. riff，来源于上次编辑的保存结果
    /// upload和riff都会被打包进入RiffPackage文件中。
    /// </summary>
    public abstract class UsageSourceInfo
    {
        public AbstractUsagePool usagePool { get; }

        protected UsageSourceInfo(AbstractUsagePool usagePool)
        {
            this.usagePool = usagePool;
        }
    }

    public class ReleasedSourceInfo:UsageSourceInfo
    {
        public static ReleasedSourceInfo Instance = new();
        private ReleasedSourceInfo():base(null)
        {
        }
    }

    public abstract class PackableSourceInfo:UsageSourceInfo
    {
        private int _packRiffIndex = -1;
        public int packRiffIndex
        {
            get {
                if (!usagePool.packPrepared)
                {
                    throw new Exception("not prepared");
                }
                return _packRiffIndex;
            }
            set
            {
                _packRiffIndex = value;
            }
        }

        protected PackableSourceInfo(AbstractUsagePool usagePool):base(usagePool)
        {
        }
    }

    public class UploadSourceInfo:PackableSourceInfo
    {
        public UploadSourceInfo(AbstractUsagePool usagePool):base(usagePool)
        {
        }

        public void ReleaseSource(UnityEngine.Object sourceObj)
        {
            usagePool.releaseFn(sourceObj);
        }
    }
    public class RiffSourceInfo : PackableSourceInfo
    {
        public readonly int unpackRiffIndex;
        public RiffSourceInfo(AbstractUsagePool usagePool, int riffIndex):base(usagePool)
        {
            unpackRiffIndex = riffIndex;
        }
    }

    public class BuiltinSourceInfo : UsageSourceInfo
    {
        public readonly string builtinPath;
        public BuiltinSourceInfo(AbstractUsagePool usagePool, string builtinPath):base(usagePool)
        {
            this.builtinPath = builtinPath;
        }
    }
}