using System.Collections.Generic;
using System.IO;
using Nianxie.Craft;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nianxie.Preview
{
    public class PreviewGizmos : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [SerializeField]
        private PreviewManager previewManager;

        private CraftEdit craftEdit;
        /// <summary>
        /// 外部加载的Texture相关资源需要销毁，在这里记录
        /// </summary>
        private Dictionary<int, UnityEngine.Object> refObjDict = new();

        public void Refresh(CraftEdit _craftEdit)
        {
            var rectTransform = (RectTransform) transform;
            craftEdit = _craftEdit;
            if (craftEdit == null || craftEdit.slotSelect == null)
            {
                gameObject.SetActive(false);
                rectTransform.SetParent(previewManager.transform);
                rectTransform.localPosition = Vector3.zero;
            }
            else
            {
                if (transform.parent != craftEdit.editCanvas.transform)
                {
                    gameObject.SetActive(true);
                    rectTransform.SetParent(craftEdit.editCanvas.transform);
                    rectTransform.localScale = Vector3.one;
                }

                var rect = craftEdit.ToCanvasRect(craftEdit.slotSelect);
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

        public void OnEdit()
        {
            if (craftEdit == null || craftEdit.slotSelect == null)
            {
                return;
            }

            if (craftEdit.slotSelect.renderSlot is SpriteSlot spriteSlot)
            {
                var imagePath = OpenImageFile();
                var imageBytes = File.ReadAllBytes(imagePath);
                var tex = new Texture2D(2,2);
                tex.LoadImage(imageBytes);
                refObjDict[tex.GetInstanceID()] = tex;
                var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one*0.5f);
                spriteSlot.SetValue(sprite);
                UnityEngine.Object.Destroy(sprite);
            }
        }
        public void OnAppend()
        {
            if (craftEdit == null || craftEdit.slotSelect == null)
            {
                return;
            }
            craftEdit.slotSelect.DuplicateSelf();
        }
        public void OnRemove()
        {
            if (craftEdit == null || craftEdit.slotSelect == null)
            {
                return;
            }
            craftEdit.slotSelect.DeleteSelf();
        }

        public string OpenImageFile()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.OpenFilePanel("select image", "./", "png,jpg,jpeg");
#else
            throw new NotImplementedException("not implement here");
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
            if (craftEdit == null || craftEdit.slotSelect == null)
            {
                return;
            }
            if (craftEdit.slotSelect.posSlot != null)
            {
                var delta = eventData.delta;
                var selectTrans = craftEdit.slotSelect.posSlot.transform;
                selectTrans.position += craftEdit.editCamera.ScreenToWorldPoint(delta) - craftEdit.editCamera.ScreenToWorldPoint(Vector3.zero);
                craftEdit.ShellRefresh();
            }
        }
    }
}
