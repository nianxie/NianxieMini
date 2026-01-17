using System.Linq;
using XLua;

namespace Nianxie.Craft
{
    public abstract class SlotInjected
    {
        public readonly SlotBehaviour behav;
        public readonly AbstractNodeInjection injection;
        protected SlotInjected(SlotBehaviour slotBehav, AbstractNodeInjection nodeInjection)
        {
            behav = slotBehav;
            injection = nodeInjection;
        }

        public abstract SlotInjected FieldChildInjected(SlotBehaviour slotBehav, AbstractNodeInjection nodeInjection);
        public abstract SlotInjected IndexChildDefaultInjected(int index);
        public abstract SlotInjected IndexChildDynamicInjected();

        public class RootInjected : DefaultInjected
        {
            public RootInjected() : base(new string[]{}, null, null)
            {
            }
        }

        public class DefaultInjected:SlotInjected
        {
            public readonly string[] keys;
            protected DefaultInjected(string[] defaultKeys, SlotBehaviour slotBehav, AbstractNodeInjection nodeInjection) : base(slotBehav, nodeInjection)
            {
                keys = defaultKeys;
            }
            public override SlotInjected FieldChildInjected(SlotBehaviour slotBehav, AbstractNodeInjection nodeInjection)
            {
                return new DefaultInjected(keys.Concat(new [] {nodeInjection.key}).ToArray(), slotBehav, nodeInjection);
            }

            public override SlotInjected IndexChildDefaultInjected(int index)
            {
                return new DefaultInjected(keys.Concat(new [] {index.ToString()}).ToArray(), behav, injection);
            }

            public override SlotInjected IndexChildDynamicInjected()
            {
                return new DynamicInjected(behav, injection);
            }
        }

        public class DynamicInjected:SlotInjected
        {
            public DynamicInjected(SlotBehaviour slotBehav, AbstractNodeInjection nodeInjection) : base(slotBehav, nodeInjection)
            {
            }

            public override SlotInjected FieldChildInjected(SlotBehaviour slotBehav, AbstractNodeInjection nodeInjection)
            {
                return new DynamicInjected(slotBehav, nodeInjection);
            }

            public override SlotInjected IndexChildDefaultInjected(int index)
            {
                return new DynamicInjected(behav, injection);
            }

            public override SlotInjected IndexChildDynamicInjected()
            {
                return new DynamicInjected(behav, injection);
            }
        }
    }
}