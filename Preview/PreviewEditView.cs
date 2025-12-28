using System;
using System.IO;
using Nianxie.Craft;
using UnityEngine;
using UnityEngine.UI;

namespace Nianxie.Preview
{
    public class PreviewEditView : MonoBehaviour
    {
        public PreviewEditGizmos gizmos;
        [SerializeField]
        private Button load;
        [SerializeField]
        private Button save;
        private CraftEdit craftEdit;
        public void Main(CraftEdit craftEdit, Action<bool> reopen)
        {
            this.craftEdit = craftEdit;
            gizmos.Main(craftEdit);
            save.onClick.AddListener(Save);
            load.onClick.AddListener(() =>
            {
                reopen(false);
            });
        }

        private void Save()
        {
#if UNITY_EDITOR
            var packContext = new PngPackContext();
            packContext.PackRoot(craftEdit.rootSlot);
            var (jsonBytes, pngData) = packContext.DumpJsonPng();
            var selectPath = UnityEditor.EditorUtility.SaveFilePanel("Save Craft", Path.Combine(Application.dataPath, ".."), "craft", "json,png");
            var (jsonPath, pngPath) = ToJsonPngPath(selectPath);
            File.WriteAllBytes(jsonPath, jsonBytes.data);
            File.WriteAllBytes(pngPath, pngData);
            UnityEditor.EditorUtility.RevealInFinder(Path.GetDirectoryName(jsonPath));
#else
            throw new NotImplementedException("not implement here");
#endif
        }
        private (string, string) ToJsonPngPath(string selectPath)
        {
            var jsonPath = $"{Path.GetDirectoryName(selectPath)}/{Path.GetFileNameWithoutExtension(selectPath)}.json";
            var pngPath = $"{Path.GetDirectoryName(selectPath)}/{Path.GetFileNameWithoutExtension(selectPath)}.png";
            return (jsonPath, pngPath);
        }
    }
}
