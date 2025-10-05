using UnityEngine;

namespace Nianxie.Craft
{
    public class PositionSlot:AbstractSlotCom<PositionJson, Vector3, Vector3>
    {
        protected override PositionJson PackFromSource(AbstractPackContext packContext, Vector3 source)
        {
            return new PositionJson
            {
                x = source.x,
                y = source.y,
                z = source.z,
            };
        }

        protected override Vector3 UnpackToTarget(CraftUnpackContext unpackContext, PositionJson slotJson)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnChangeValue()
        {
            throw new System.NotImplementedException();
        }

        protected override Vector3 ValueProcess(Vector3 source)
        {
            return source;
        }
    }
}