using System;

namespace Nianxie.Craft
{
    public abstract class AbstractSlotJson
    {
        public virtual object MakeTarget(IGetAsset getAsset)
        {
            throw new NotImplementedException("Read data");
        }
    }
}