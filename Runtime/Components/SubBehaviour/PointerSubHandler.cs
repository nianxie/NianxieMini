using Nianxie.Framework;
using UnityEngine.EventSystems;
using XLua;

namespace Nianxie.Components
{
    public class PointerSubHandler:SubBehaviour<PointerVtbl>, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            subTable.OnPointerDown?.Action(self, eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            subTable.OnPointerClick?.Action(self, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            subTable.OnPointerUp?.Action(self, eventData);
        }
    }
}