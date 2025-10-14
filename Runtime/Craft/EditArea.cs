using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nianxie.Components;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using XLua;

namespace Nianxie.Craft
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class EditArea : MonoBehaviour, IPointerClickHandler
    {
        private EditRoot editRoot;
        void Awake()
        {
            editRoot = GetComponentInParent<EditRoot>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            editRoot.OnSelect(null);
        }
    }
}
