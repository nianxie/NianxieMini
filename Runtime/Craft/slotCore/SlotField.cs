using XLua;

namespace Nianxie.Craft
{
    public class SlotField
    {
        public readonly SlotBehaviour behav;
        public readonly AbstractNodeInjection injection;
        public SlotField(SlotBehaviour slotBehav, AbstractNodeInjection nodeInjection)
        {
            behav = slotBehav;
            injection = nodeInjection;
        }
    }
}