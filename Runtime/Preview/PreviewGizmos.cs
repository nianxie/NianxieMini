using Nianxie.Craft;
using UnityEngine;

namespace Nianxie.Preview
{
    public class PreviewGizmos : MonoBehaviour
    {
        [SerializeField]
        private PreviewManager previewManager;

        private CraftEdit craftEdit;

        public void Refresh(CraftEdit _craftEdit)
        {
            var rectTransform = (RectTransform) transform;
            craftEdit = _craftEdit;
            if (craftEdit == null || craftEdit.selectAssetSlot == null)
            {
                gameObject.SetActive(false);
                rectTransform.SetParent(previewManager.transform);
                rectTransform.localPosition = Vector3.zero;
            }
            else
            {
                var selectTransform = craftEdit.selectAssetSlot.transform;
                if (transform.parent != craftEdit.canvas.transform)
                {
                    gameObject.SetActive(true);
                    rectTransform.SetParent(craftEdit.canvas.transform);
                    rectTransform.localScale = Vector3.one;
                }

                var screenPoint = RectTransformUtility.WorldToScreenPoint(craftEdit.camera, selectTransform.position);
                rectTransform.anchoredPosition = screenPoint;
            }
        }

        public void OnEdit()
        {
            if (craftEdit == null || craftEdit.selectAssetSlot == null)
            {
                return;
            }
            Debug.Log("on edit");
        }
        public void OnAppend()
        {
            if (craftEdit == null || craftEdit.selectAssetSlot == null)
            {
                return;
            }
            var slot = craftEdit.selectAssetSlot.GetComponent<ListSlot>();
            slot.Append();
        }
        public void OnRemove()
        {
            if (craftEdit == null || craftEdit.selectAssetSlot == null)
            {
                return;
            }
        }
    }
}
