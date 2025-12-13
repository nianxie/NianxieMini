using System.Collections;
using System.Collections.Generic;
using Unity.Plastic.Antlr3.Runtime.Misc;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    public class TableSlot : AbstractSlotCom
    {
        [SerializeField]
        private Vector2 delta = new Vector2(1, 1);
        [SerializeField]
        private List<AbstractNodeSlot> list;

        private bool tableDirty = true;
        private LuaTable table;
        public override AbstractSlotJson PackToJson(AbstractPackContext packContext)
        {
            throw new System.NotImplementedException();
        }

        public override void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson)
        {
            throw new System.NotImplementedException();
        }

        public LuaTable ReadTable()
        {
            if (!tableDirty)
            {
                return table;
            }

            if (table == null)
            {
                table = slotCallback.NewTable();
            }
            else
            {
                var len = table.Length;
                for (int i = 0; i < len; i++)
                {
                    table.Set<int, AbstractSlotCom>(i, null);
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                table.Set(i, list[i]);
            }
            return table;
        }

        public void DuplicateChild(AbstractNodeSlot nodeSlot)
        {
            foreach (var child in list)
            {
                if (child.gameObject == nodeSlot.gameObject)
                {
                    var newObj = Instantiate(child, transform);
                    foreach (var dupNodeSlot in newObj.GetComponentsInChildren<AbstractNodeSlot>())
                    {
                        dupNodeSlot.PostDuplicate();
                    }
                    list.Add(newObj);
                    return;
                }
            }
        }
        public void DeleteChild(AbstractNodeSlot nodeSlot)
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
