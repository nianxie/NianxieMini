using System.Collections.Generic;

namespace Nianxie.Craft
{
    public class SlotScriptJson: AbstractSlotJson
    {
        public Dictionary<string, AbstractSlotJson> slotDict = new();
    }
}