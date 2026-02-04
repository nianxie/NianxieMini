using System;
using UnityEditorInternal;

namespace Nianxie.Craft
{
    public abstract class AbstractSlotJson<TSlotTarget>:AbstractSlotJson
    {
        public new abstract TSlotTarget Export(AssetUsageCenter usageCenter);

        protected override object ExportAsObject(AssetUsageCenter usageCenter)
        {
            return Export(usageCenter);
        }
    }
    
    public abstract class AbstractSlotJson
    {
        public object Export(AssetUsageCenter usageCenter)
        {
            return ExportAsObject(usageCenter);
        }

        protected abstract object ExportAsObject(AssetUsageCenter usageCenter);
    }
}