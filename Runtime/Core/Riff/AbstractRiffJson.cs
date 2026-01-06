
namespace Nianxie.Riff
{
    public abstract class AbstractRiffJson
    {
        public abstract string kind { get; }
        public abstract string version { get; }

        public string Dump()
        {
            return JsonCodec.Dump(this);
        }
    }
}