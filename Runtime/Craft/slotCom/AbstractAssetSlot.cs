using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XLua;

namespace Nianxie.Craft
{
    
    [ExecuteAlways]
    public abstract class AbstractAssetSlot : AbstractRenderSlot
    {
        public virtual void WriteRawData(object rawData)
        {
            throw new NotImplementedException();
        }
    }
    
}
