using XLua;

namespace Nianxie.Craft
{
    public class SlotInjected
    {
        public readonly SlotBehaviour behav;
        public readonly AbstractNodeInjection injection;
        public SlotInjected(SlotBehaviour slotBehav, AbstractNodeInjection nodeInjection)
        {
            behav = slotBehav;
            injection = nodeInjection;
        }
    }
}