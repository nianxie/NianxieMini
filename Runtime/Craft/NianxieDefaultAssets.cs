using TMPro;
using UnityEngine;

namespace Nianxie.Craft
{
    // editor模式下以及preview的时候，拿取一些默认资源
    public interface NianxieDefaultAssets
    {
        public TMPModify_ShellFont shellFont { get; }
        public Sprite sliced9 { get; }
    }
}