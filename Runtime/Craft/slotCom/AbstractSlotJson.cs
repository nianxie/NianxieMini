using System;

namespace Nianxie.Craft
{
    public abstract class AbstractSlotJson<TSlotTarget>:AbstractSlotJson
    {
        public new abstract TSlotTarget Export(IGetAsset getAsset);

        protected override object ExportAsObject(IGetAsset getAsset)
        {
            return Export(getAsset);
        }
    }

    public abstract class AbstractSlotJson
    {
        public object Export(IGetAsset getAsset)
        {
            return ExportAsObject(getAsset);
        }

        protected abstract object ExportAsObject(IGetAsset getAsset);
    }
}