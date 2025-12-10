using UnityEngine;
using UnityEngine.UI;

namespace Nianxie.Components
{
    public class CustomGraphicRaycaster : GraphicRaycaster
    {
        [SerializeField]
        private int raycasterPriority = -1;
        public override int sortOrderPriority {
            get
            {
                return raycasterPriority;
            }
        }
    }
}
