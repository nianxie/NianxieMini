namespace Nianxie.Craft
{
    public abstract class AbstractUsage
    {
        public UsageSourceInfo sourceInfo { get; protected set; }

        public virtual void Clear()
        {
        }
    }
}