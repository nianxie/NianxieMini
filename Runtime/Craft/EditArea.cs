using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nianxie.Components;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

namespace Nianxie.Craft
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class EditArea : MonoBehaviour, IPointerClickHandler
    {
        private CraftManager craftManager;
        void Awake()
        {
            craftManager = GetComponentInParent<CraftManager>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            (craftManager as ISlotHandler).OnSelect(null);
        }
    }
}
