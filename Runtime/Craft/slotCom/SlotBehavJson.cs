using System;
using System.Collections.Generic;
using Nianxie.Framework;
using XLua;

namespace Nianxie.Craft
{
    public class SlotBehavJson: AbstractSlotJson<LuaTable>
    {
        public string classPath = "";
        public string[] nestedKeys = EnvPaths.NESTED_KEYS_EMPTY;
        public Dictionary<string, AbstractSlotJson> singleDict = new();
        public Dictionary<string, AbstractSlotJson[]> listDict = new();
        public override LuaTable Export(AssetUsageCenter usageCenter)
        {
            var slotTable = usageCenter.NewTable();
            foreach (var (k, v) in singleDict)
            {
                slotTable.Set(k, v.Export(usageCenter));
            }

            foreach (var (k, v) in listDict)
            {
                var valueTable = usageCenter.NewTable();
                for (int i = 0; i < v.Length; i++)
                {
                    valueTable.Set(i+1, v[i].Export(usageCenter));
                }
                slotTable.Set(k, valueTable);
            }

            return slotTable;
        }
    }
}