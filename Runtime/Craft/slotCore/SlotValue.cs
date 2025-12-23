using System;

namespace Nianxie.Craft
{
    [Serializable]
    public struct SlotValue<T>
    {
        public T defaultValue;
        [NonEditable]
        public T assignedValue;
        [NonEditable]
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