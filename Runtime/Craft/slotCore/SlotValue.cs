using System;
using UnityEngine;

namespace Nianxie.Craft
{
    public class SlotValueAttribute:PropertyAttribute
    {
    }

    [Serializable]
    public struct SlotValue<T>
    {
        public T defaultValue;
        public T assignedValue;
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