using System.Collections.Generic;

namespace Nianxie.Craft
{
    public class BehavJson: AbstractSlotJson
    {
        public Dictionary<string, AbstractSlotJson> slotDict = new();
    }
}