using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nianxie.Craft
{
    public class ListSlot : AbstractSlotCom
    {
        [SerializeField]
        private Vector2 delta = new Vector2(1, 1);
        [SerializeField]
        private AbstractElementSlot template;
        [SerializeField]
        private List<AbstractElementSlot> list;
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

        public void Append()
        {
            var com = UnityEngine.Object.Instantiate(template, transform);
            list.Add(com);
        }
    }
}
