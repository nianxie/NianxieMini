using System.Collections.Generic;
using XLua;

namespace Nianxie.Craft
{
    public class SlotBehavJson: AbstractSlotJson<LuaTable>
    {
        public Dictionary<string, AbstractSlotJson> singleDict = new();
        public Dictionary<string, AbstractSlotJson[]> listDict = new();
    }
}