using Nianxie.Framework;
using UnityEngine.EventSystems;

namespace Nianxie.Components
{
    public class DragSubHandler:SubBehaviour<DragVtbl>, IDragHandler
    {
        public void OnDrag(PointerEventData eventData)
        {
            subTable.OnDrag.Action(self, eventData);
        }
    }
}