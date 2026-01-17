using System;
using UnityEditorInternal;

namespace Nianxie.Craft
{
    public abstract class AbstractSlotJson<TSlotTarget>:AbstractSlotJson
    {
        public new abstract TSlotTarget Export(UnpackContext unpackContext);

        protected override object ExportAsObject(UnpackContext unpackContext)
        {
            return Export(unpackContext);
        }
    }
    
    public abstract class AbstractSlotJson
    {
        public object Export(UnpackContext unpackContext)
        {
            return ExportAsObject(unpackContext);
        }

        protected abstract object ExportAsObject(UnpackContext unpackContext);
    }
}