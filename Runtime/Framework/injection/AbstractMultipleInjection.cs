namespace XLua
{
    /// <summary>
    /// 支持list和dict的injection
    /// </summary>
    public abstract class AbstractMultipleInjection:AbstractReflectInjection
    {
        public readonly InjectionMultipleKind multipleKind;
        protected int _count;
        public int count => _count;

        protected AbstractMultipleInjection(WarmedReflectClass cls, RawReflectInjection rawInjection, InjectionMultipleKind kind):base(cls, rawInjection)
        {
            multipleKind = kind;
        }

    }
}