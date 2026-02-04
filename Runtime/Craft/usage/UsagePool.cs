using System;
using System.Collections.Generic;
using System.Linq;
using Nianxie.Framework;
using Nianxie.Riff;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{

    public abstract class AbstractUsagePool {
        public bool packPrepared { get; protected set; }
        public Action<UnityEngine.Object> releaseFn { get; protected set; }
        public void ResetPackPrepared()
        {
            packPrepared = false;
        }
    }

    public class UsagePool<TUsage> : AbstractUsagePool where TUsage: AbstractUsage
    {
        private HashSet<TUsage> usageSet = new();
        private Dictionary<int, TUsage> unpackUsageDict = new();
        private Dictionary<string, TUsage> builtinUsageDict = new();

        public TUsage FindUsage(IUsageLocator locator)
        {
            var builtinPath = locator.GetBuiltinPath();
            if (!string.IsNullOrEmpty(builtinPath))
            {
                return builtinUsageDict[builtinPath];
            }
            else
            {
                return unpackUsageDict[locator.GetRiffIndex()];
            }
        }

        [BlackList]
        public TUsage GetBuiltinTextureUsage(string builtinPath)
        {
            return builtinUsageDict[builtinPath];
        }
        
        [BlackList]
        public TUsage GetUnpackTextureUsage(int riffIndex)
        {
            return unpackUsageDict[riffIndex];
        }

        public TUsage[] PreparePackableUsages()
        {
            var usages = usageSet.Where(usage => usage.sourceInfo is PackableSourceInfo).ToArray();
            for (int i = 0; i < usages.Length; i++)
            {
                (usages[i].sourceInfo as PackableSourceInfo).packRiffIndex = i;
            }
            packPrepared = true;
            return usages;
        }

        protected void AddUsage(TUsage usage)
        {
            ResetPackPrepared();
            usageSet.Add(usage);
            if (usage.sourceInfo is RiffSourceInfo riffSourceKind)
            {
                unpackUsageDict[riffSourceKind.unpackRiffIndex] = usage;
            } else if (usage.sourceInfo is BuiltinSourceInfo builtinSourceKind)
            {
                builtinUsageDict[builtinSourceKind.builtinPath] = usage;
            }
        }

        public void Clear()
        {
            foreach (var usage in usageSet)
            {
                usage.Clear();
            }
            usageSet.Clear();
        }
    }
}