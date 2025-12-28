using System.Collections.Generic;

namespace Nianxie.Craft
{
    public class SlotBehavJson: AbstractSlotJson
    {
        public Dictionary<string, AbstractSlotJson> singleDict = new();
        public Dictionary<string, AbstractSlotJson[]> listDict = new();
    }
}