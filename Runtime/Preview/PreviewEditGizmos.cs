using System.Collections.Generic;
using System.IO;
using Nianxie.Craft;
using Nianxie.Riff;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nianxie.Preview
{
    public class PreviewEditGizmos : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private CraftManager craftManager;
        /// <summary>
        /// 外部加载的Texture相关资源需要销毁，在这里记录
        /// </summary>
        private Dictionary<int, UnityEngine.Object> refObjDict = new();

        public void Main(CraftManager craftManager)
        {
            this.craftManager = craftManager;
            Refresh();
        }

        public void Refresh()
        {
            var rectTransform = (RectTransform) transform;
            if (craftManager.slotSelect == null)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
                var rect = craftManager.ToCanvasRect(craftManager.slotSelect);
                rectTransform.anchoredPosition = rect.min;
                rectTransform.sizeDelta = rect.size;
            }
        }

        public void Release(UnityEngine.Object obj)
        {
            if (refObjDict.ContainsKey(obj.GetInstanceID()))
            {
                refObjDict.Remove(obj.GetInstanceID());
                UnityEngine.Object.Destroy(obj);
            }
        }

        private void OnDestroy()
        {
            foreach (var v in refObjDict.Values)
            {
                UnityEngine.Object.Destroy(v);
            }
            refObjDict.Clear();
        }

        public void OnEdit()
        {
            if (craftManager.slotSelect == null)
            {
                return;
            }

            if (craftManager.slotSelect.renderSlot is SpriteSlot spriteSlot)
            {
                var imagePath = OpenImageFile();
                var imageBytes = File.ReadAllBytes(imagePath);
                var tex = new Texture2D(2,2);
                tex.LoadImage(imageBytes);
                refObjDict[tex.GetInstanceID()] = tex;
                var texUsage = craftManager.assetUsageCenter.UploadTexture(tex);
                spriteSlot.Assign(texUsage);
            }
        }
        public void OnAppend()
        {
            if (craftManager.slotSelect == null)
            {
                return;
            }
            craftManager.slotSelect.DuplicateSelf();
        }
        public void OnRemove()
        {
            if (craftManager.slotSelect == null)
            {
                return;
            }
            craftManager.slotSelect.DeleteSelf();
        }

        public string OpenImageFile()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.OpenFilePanel("select image", "./", "png,jpg,jpeg");
#else
            throw new System.NotImplementedException("not implement here");
#endif
        }
        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (craftManager.slotSelect == null)
            {
                return;
            }
            if (craftManager.slotSelect.posSlot != null)
            {
                var delta = eventData.delta;
                var selectTrans = craftManager.slotSelect.posSlot.transform;
                selectTrans.position += craftManager.editCamera.ScreenToWorldPoint(delta) - craftManager.editCamera.ScreenToWorldPoint(Vector3.zero);
                (craftManager as ISlotHandler).ShellRefresh();
            }
        }
    }
}
