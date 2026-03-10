using System;
using UnityEngine;

namespace Nianxie.Craft
{
    public abstract class AbstractSlotValue
    {
    }

    [Serializable]
    public class SlotValue<T>:AbstractSlotValue
    {
        public T defaultValue = default(T);
        private T assignedValue;
        private bool isAssigned;

        public T Get()
        {
            return isAssigned ? assignedValue : defaultValue;
        }
        public T Set(T value)
        {
            isAssigned = true;
            var oldValue = assignedValue;
            assignedValue = value;
            return oldValue;
        }
    }
}