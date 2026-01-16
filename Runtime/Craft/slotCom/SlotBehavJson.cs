using System;
using System.Collections.Generic;
using XLua;

namespace Nianxie.Craft
{
    public class SlotBehavJson: AbstractSlotJson<LuaTable>
    {
        public Dictionary<string, AbstractSlotJson> singleDict = new();
        public Dictionary<string, AbstractSlotJson[]> listDict = new();
        public override LuaTable Export(IGetAsset getAsset)
        {
            var slotTable = getAsset.NewTable();
            foreach (var (k, v) in singleDict)
            {
                slotTable.Set(k, v.Export(getAsset));
            }

            foreach (var (k, v) in listDict)
            {
                var valueTable = getAsset.NewTable();
                for (int i = 0; i < v.Length; i++)
                {
                    valueTable.Set(i+1, v[i].Export(getAsset));
                }
                slotTable.Set(k, valueTable);
            }

            return slotTable;
        }
    }
}