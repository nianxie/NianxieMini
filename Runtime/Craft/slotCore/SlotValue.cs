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
        public T defaultValue;
        [NonSerialized]
        public T assignedValue;
        [NonSerialized] 
        public bool isAssigned;

        public T ReadValue()
        {
            return isAssigned ? assignedValue : defaultValue;
        }
        public void AssignValue(T value)
        {
            isAssigned = true;
            assignedValue = value;
        }

        public T SafeCast(object o)
        {
            if (o is T t)
            {
                return t;
            }
            else
            {
                throw new InvalidCastException($"{typeof(T)} expected but get {o.GetType()}");
            }
        }
    }
}