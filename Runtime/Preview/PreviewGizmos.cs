using Nianxie.Craft;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
            if (craftEdit == null || craftEdit.selectNodeSlot == null)
            {
                gameObject.SetActive(false);
                rectTransform.SetParent(previewManager.transform);
                rectTransform.localPosition = Vector3.zero;
            }
            else
            {
                var selectTransform = craftEdit.selectNodeSlot.transform;
                if (transform.parent != craftEdit.editCanvas.transform)
                {
                    gameObject.SetActive(true);
                    rectTransform.SetParent(craftEdit.editCanvas.transform);
                    rectTransform.localScale = Vector3.one;
                }

                var screenPoint = RectTransformUtility.WorldToScreenPoint(craftEdit.editCamera, selectTransform.position);
                rectTransform.anchoredPosition = screenPoint;
            }
        }

        public void OnEdit()
        {
            if (craftEdit == null || craftEdit.selectNodeSlot == null)
            {
                return;
            }
            Debug.Log("on edit");
        }
        public void OnAppend()
        {
            if (craftEdit == null || craftEdit.selectNodeSlot == null)
            {
                return;
            }

            if (craftEdit.selectNodeSlot.TryGetComponent<ListSlot>(out var listSlot))
            {
                listSlot.OperAppend();
            }
            else
            {
                Debug.LogError("list slot");
            }
        }
        public void OnRemove()
        {
            if (craftEdit == null || craftEdit.selectNodeSlot == null)
            {
                return;
            }
            craftEdit.selectNodeSlot.OperRemoveSelf();
        }
    }
}
