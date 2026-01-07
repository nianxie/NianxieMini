using System;
using System.IO;
using Nianxie.Craft;
using Nianxie.Preview;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Nianxie.Preview
{
    public class PreviewEditView : MonoBehaviour
    {
        public enum ReopenKind
        {
            LOAD = 1,
            RESET = 2,
        }

        public PreviewEditGizmos gizmos;
        [SerializeField]
        private Button load;
        [SerializeField]
        private Button reset;
        [SerializeField]
        private Button save;
        private DefaultCraftEntry craftEntry;
        public void Main(DefaultCraftEntry craftEntry, Action<ReopenKind> reopen)
        {
            this.craftEntry = craftEntry;
            gizmos.Main(craftEntry.craftEdit);
            save.onClick.AddListener(Save);
            load.onClick.AddListener(() =>
            {
                reopen(ReopenKind.LOAD);
            });
            reset.onClick.AddListener(() =>
            {
                reopen(ReopenKind.RESET);
            });
        }

        private void Save()
        {
#if UNITY_EDITOR
            var packContext = new SimplePackContext();
            var packBytes = packContext.PackRoot(craftEntry.rootSlot);
            var selectPath = UnityEditor.EditorUtility.SaveFilePanel("Save Craft", Directory.GetCurrentDirectory(), "untitle", NianxieConst.Ext.CRAFT);
            selectPath = $"{Path.GetDirectoryName(selectPath)}/{Path.GetFileNameWithoutExtension(selectPath)}.{NianxieConst.Ext.CRAFT}";
            File.WriteAllBytes(selectPath, packBytes);
            UnityEditor.EditorUtility.RevealInFinder(Path.GetDirectoryName(selectPath));
#else
            throw new NotImplementedException("not implement here");
#endif
        }
    }
}
