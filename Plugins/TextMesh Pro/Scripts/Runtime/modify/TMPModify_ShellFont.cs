using System.Linq;
using UnityEngine;

namespace TMPro
{
    
    [CreateAssetMenu(fileName = "ShellFont", menuName = "TextMeshPro/Shell Font")]
    public class TMPModify_ShellFont:ScriptableObject
    {
        private const string OUTLINE_ON = "OUTLINE_ON";
        private const string UNDERLAY_ON = "UNDERLAY_ON";
        [TMPModify_ShellFontName] 
        public string fontName;
        public Color Face_Color = Color.white;
        [Range(0f, 1f)]
        public float Face_Softness;
        [Range(-1f, 1f)]
        public float Face_Dilate;
        public Color Outline_Color = Color.black;
        [Range(0f, 1f)]
        public float Outline_Thickness;

        public bool Underlay_Enable = false;
        public Color Underlay_Color = Color.black;
        [Range(-1f, 1f)]
        public float Underlay_OffsetX;
        [Range(-1f, 1f)]
        public float Underlay_OffsetY;
        [Range(-1f, 1f)]
        public float Underlay_Dilate;
        [Range(0f, 1f)]
        public float Underlay_Softness;

        private TMP_FontAsset m_FontAsset;
        public TMP_FontAsset fontAsset {
            get
            {
                if (m_FontAsset == null)
                {
                    Refresh();
                }
                return m_FontAsset;
            }
        }
        private Material m_Material;
        public Material material
        {
            get
            {
                if (m_Material == null)
                {
                    RefreshMaterial();
                }
                return m_Material;
            }
        }

        public void Refresh()
        {
            var matchNameFont = TMP_Settings.rawShellFonts.FirstOrDefault(nameFont => nameFont.name == fontName);
            if (matchNameFont == null)
            {
                matchNameFont = TMP_Settings.rawShellFonts[0];
                fontName = matchNameFont.name;
            }

            m_FontAsset = matchNameFont.fontAsset;

            if (m_Material != null)
            {
                DestroyImmediate(m_Material);
            }
            RefreshMaterial();
        }

        public void RefreshMaterial()
        {
            if (m_Material == null)
            {
                m_Material = Instantiate(fontAsset.material);
                m_Material.SetTexture(ShaderUtilities.ID_MainTex, fontAsset.atlasTexture);
            }

            // face
            m_Material.SetColor(ShaderUtilities.ID_FaceColor, Face_Color);
            m_Material.SetFloat(ShaderUtilities.ID_OutlineSoftness, Face_Softness);
            m_Material.SetFloat(ShaderUtilities.ID_FaceDilate, Face_Dilate);
            
            //outline
            m_Material.SetColor(ShaderUtilities.ID_OutlineColor, Outline_Color);
            if (!m_Material.HasProperty(ShaderUtilities.ID_OutlineTex))
            {
                if (Outline_Thickness > 0)
                {
                    m_Material.EnableKeyword(OUTLINE_ON);
                }
                else
                {
                    m_Material.DisableKeyword(OUTLINE_ON);
                }
            }
            m_Material.SetFloat(ShaderUtilities.ID_OutlineWidth, Outline_Thickness);
            
            //underlay
            if (Underlay_Enable)
            {
                m_Material.EnableKeyword(UNDERLAY_ON);
            }
            else
            {
                m_Material.DisableKeyword(UNDERLAY_ON);
            }

            m_Material.SetColor(ShaderUtilities.ID_UnderlayColor, Underlay_Color);
            m_Material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, Underlay_OffsetX);
            m_Material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, Underlay_OffsetY);
            m_Material.SetFloat(ShaderUtilities.ID_UnderlayDilate, Underlay_Dilate);
            m_Material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, Underlay_Softness);
        }
        
        public void WriteBack()
        {

            // face
            Face_Color = m_Material.GetColor(ShaderUtilities.ID_FaceColor);
            Face_Softness = m_Material.GetFloat(ShaderUtilities.ID_OutlineSoftness);
            Face_Dilate = m_Material.GetFloat(ShaderUtilities.ID_FaceDilate);
            
            //outline
            Outline_Color = m_Material.GetColor(ShaderUtilities.ID_OutlineColor);
            if (!m_Material.HasProperty(ShaderUtilities.ID_OutlineTex))
            {
                if (!m_Material.IsKeywordEnabled(OUTLINE_ON))
                {
                    Outline_Thickness = 0;
                }
                else
                {
                    Outline_Thickness = m_Material.GetFloat(ShaderUtilities.ID_OutlineWidth);
                }
            }
            else
            {
                Outline_Thickness = m_Material.GetFloat(ShaderUtilities.ID_OutlineWidth);
            }
            
            //underlay
            Underlay_Enable = m_Material.IsKeywordEnabled(UNDERLAY_ON);

            Underlay_Color = m_Material.GetColor(ShaderUtilities.ID_UnderlayColor);
            Underlay_OffsetX = m_Material.GetFloat(ShaderUtilities.ID_UnderlayOffsetX);
            Underlay_OffsetY = m_Material.GetFloat(ShaderUtilities.ID_UnderlayOffsetY);
            Underlay_Dilate = m_Material.GetFloat(ShaderUtilities.ID_UnderlayDilate);
            Underlay_Softness = m_Material.GetFloat(ShaderUtilities.ID_UnderlaySoftness);
        }
        
        void OnDestroy()
        {
            if (m_Material != null)
            {
                Destroy(m_Material);
                m_Material = null;
            }
        }
    }
}