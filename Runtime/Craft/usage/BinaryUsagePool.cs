using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Nianxie.Framework;
using Nianxie.Riff;
using UnityEngine;
using WebP;
using XLua;

namespace Nianxie.Craft
{
    public class BinaryUsagePool: UsagePool<BinaryUsage>
    {
        public BinaryUsagePool(CraftManager craftManager) 
        {
            releaseFn = craftManager.ShellRelease;
        }
        
        public async UniTask<(ManifestRiffJson.BinaryMeta[], List<byte[]>)> PackBinariesAndList()
        {
            return (new ManifestRiffJson.BinaryMeta[]{}, new List<byte[]>());
        }
    }
}