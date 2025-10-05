using Nianxie.Framework;
using UnityEngine.EventSystems;
using XLua;

namespace Nianxie.Components
{
    public class DragBeginEndSubHandler:SubBehaviour<DragBeginEndVtbl>, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            subTable.OnInitializePotentialDrag?.Action(self, eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            subTable.OnBeginDrag?.Action(self, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            subTable.OnEndDrag?.Action(self, eventData);
        }
    }
}