using Nianxie.Framework;
using UnityEngine.EventSystems;

namespace Nianxie.Components
{
    public class DropSubHandler:SubBehaviour<DropVtbl>, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            subTable.OnDrop.Action(self, eventData);
        }
    }
}