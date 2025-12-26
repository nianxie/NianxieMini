using Nianxie.Utils;
using TMPro;
using UnityEngine;

namespace Nianxie.Preview
{
    public class PreviewAssets : ScriptableObject, NianxieDefaultAssets
    {
        [SerializeField]
        private TMPModify_ShellFont m_ShellFont;
        public TMPModify_ShellFont shellFont => m_ShellFont;
        
        [SerializeField]
        private TextAsset m_MiniBoot;
        public TextAsset miniBoot => m_MiniBoot;

        [SerializeField]
        private Sprite m_Sliced9;
        public Sprite sliced9 => m_Sliced9;
        
        [SerializeField]
        private Sprite m_IconCraft;
        public Sprite iconCraft => m_IconCraft;
        [SerializeField]
        private Sprite m_IconGame;
        public Sprite iconGame => m_IconGame;
        
        private static PreviewAssets _instance;

        public static PreviewAssets instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_EDITOR
                    _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<PreviewAssets>($"{NianxieConst.MiniDefaultAssets}/PreviewAssets.asset");
#else
                    throw new NotImplementedException("preview assets is only used when runtime");
#endif
                }
                return _instance;
            }
        }
    }
}
