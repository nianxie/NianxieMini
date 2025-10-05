namespace XLua
{
    /// <summary>
    /// TODO reference injection 暂时未实现
    /// </summary>
    public class ReferenceInjection:AbstractReflectInjection
    {
        public ReferenceInjection(WarmedReflectClass cls, RawReflectInjection rawInjection) : base(cls, rawInjection)
        {
        }
    }
}