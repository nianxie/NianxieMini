using System;
using System.IO;
using Cysharp.Threading.Tasks;
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
        private CraftEdit craftEdit;
        public void Main(CraftEdit craftEdit, Action<ReopenKind> reopen)
        {
            this.craftEdit = craftEdit;
            gizmos.Main(craftEdit);
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
            UniTask.Create(async () => { 
                var packBytes = await craftEdit.PackCraftAsync<SimplePackContext>();
                var selectPath = UnityEditor.EditorUtility.SaveFilePanel("Save Craft", Directory.GetCurrentDirectory(), "untitle", NianxieConst.Ext.CRAFT);
                selectPath = $"{Path.GetDirectoryName(selectPath)}/{Path.GetFileNameWithoutExtension(selectPath)}.{NianxieConst.Ext.CRAFT}";
                await File.WriteAllBytesAsync(selectPath, packBytes);
                UnityEditor.EditorUtility.RevealInFinder(Path.GetDirectoryName(selectPath));
            }).Forget();;
#else
            throw new NotImplementedException("not implement here");
#endif
        }
    }
}
