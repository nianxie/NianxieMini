using System.Collections;
using System.Collections.Generic;
using Unity.Plastic.Antlr3.Runtime.Misc;
using UnityEngine;

namespace Nianxie.Craft
{
    public class ListSlot : AbstractSlotCom
    {
        [SerializeField]
        private Vector2 delta = new Vector2(1, 1);
        [SerializeField]
        private AbstractSlotCom template;
        [SerializeField]
        private List<AbstractSlotCom> list;
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

        public void OperAppend()
        {
            var com = UnityEngine.Object.Instantiate(template, transform);
            list.Add(com);
        }
        public void OperRemoveElement(AbstractNodeSlot nodeSlot)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].gameObject == nodeSlot.gameObject)
                {
                    list.RemoveAt(i);
                    UnityEngine.Object.Destroy(nodeSlot.gameObject);
                    return;
                }
            }
        }
    }
}
