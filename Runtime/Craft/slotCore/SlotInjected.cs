using System.Linq;
using XLua;

namespace Nianxie.Craft
{
    public abstract class SlotInjected
    {
        public readonly SlotBehaviour ancestor;
        public readonly AbstractNodeInjection injection;
        protected SlotInjected(SlotBehaviour ancestorBehav, AbstractNodeInjection nodeInjection)
        {
            ancestor = ancestorBehav;
            injection = nodeInjection;
        }

        public abstract SlotInjected FieldChildInjected(SlotBehaviour slotBehav, AbstractNodeInjection nodeInjection);
        public abstract SlotInjected IndexChildDefaultInjected(int index);
        public abstract SlotInjected IndexChildDynamicInjected();
        public abstract bool IsList();

        public class RootInjected : DefaultInjected
        {
            public RootInjected() : base(new string[]{}, null, null)
            {
            }

            public override bool IsList()
            {
                return false;
            }
        }

        // prefab中写定的slot会持有这个injected
        public class DefaultInjected:SlotInjected
        {
            public readonly string[] keys;
            protected DefaultInjected(string[] defaultKeys, SlotBehaviour ancestor, AbstractNodeInjection nodeInjection) : base(ancestor, nodeInjection)
            {
                keys = defaultKeys;
            }
            public override SlotInjected FieldChildInjected(SlotBehaviour slotBehav, AbstractNodeInjection nodeInjection)
            {
                return new DefaultInjected(keys.Concat(new [] {nodeInjection.key}).ToArray(), slotBehav, nodeInjection);
            }

            public override SlotInjected IndexChildDefaultInjected(int index)
            {
                return new DefaultInjected(keys.Concat(new [] {index.ToString()}).ToArray(), ancestor, injection);
            }

            public override SlotInjected IndexChildDynamicInjected()
            {
                return new DynamicInjected(ancestor, injection);
            }
            public override bool IsList()
            {
                return injection.multipleKind == InjectionMultipleKind.List;
            }
        }

        // 通过添加操作动态创建的slot会持有这个injected
        public class DynamicInjected:SlotInjected
        {
            public DynamicInjected(SlotBehaviour ancestor, AbstractNodeInjection nodeInjection) : base(ancestor, nodeInjection)
            {
            }

            public override SlotInjected FieldChildInjected(SlotBehaviour slotBehav, AbstractNodeInjection nodeInjection)
            {
                return new DynamicInjected(slotBehav, nodeInjection);
            }

            public override SlotInjected IndexChildDefaultInjected(int index)
            {
                return new DynamicInjected(ancestor, injection);
            }

            public override SlotInjected IndexChildDynamicInjected()
            {
                return new DynamicInjected(ancestor, injection);
            }
            public override bool IsList()
            {
                return true;
            }
        }
    }
}