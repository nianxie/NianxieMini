using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nianxie.Craft
{
    public class ListSlot : AbstractSlotCom
    {
        [SerializeField]
        private MonoBehaviour element;
        public override AbstractSlotJson PackToJson(AbstractPackContext packContext)
        {
            throw new System.NotImplementedException();
        }

        public override void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson)
        {
            throw new System.NotImplementedException();
        }

        public override object ReadData()
        {
            throw new System.NotImplementedException();
        }
    }
}
