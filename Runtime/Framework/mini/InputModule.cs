using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using XLua;

namespace Nianxie.Framework
{
    using Components;
    // InputModule现在没有用
    public class InputModule : AbstractGameModule
    {
        public InputActionAsset inputActionAsset;
        private InputAction press;
        private InputAction pos;
        private InputAction delta;
        
        public override async UniTask Init()
        {
            //TouchSimulation.Enable();
            var posValue = Vector2.zero;
            pos = inputActionAsset.FindAction("Pos");
            delta = inputActionAsset.FindAction("Delta");
            press = inputActionAsset.FindAction("Press");
            pos.performed += context =>
            {
                //posValue = (Vector2)context.ReadValueAsObject();
                //Debug.Log($"input pos {posValue}");
            };
            delta.performed += context =>
            {
                /*
                var deltaPos = (Vector2)context.ReadValueAsObject();
                if (entityTouch != null)
                {
                    var fromPos = posValue - deltaPos;
                    entityTouch.TouchDrag(fromPos, posValue);
                }*/
            };
            press.performed += context =>
            {
                /*
                var pointerEventData = new PointerEventData(EventSystem.current)
                {
                    position = posValue
                };
                var raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerEventData, raycastResults);
                if (raycastResults.Count > 0)
                {
                    // if raycast on ui then return
                    return;
                }

                var col= luaModule.iMainContext._OnTouchRaycast(posValue);
                if (col!=null)
                {
                    entityTouch = new PhysicsEntityTouch(col, posValue);
                }*/
            };
            press.canceled += context =>
            {
                /*
                if (entityTouch != null)
                {
                    entityTouch.TouchUp(posValue);
                    entityTouch = null;
                }*/
            };
            /*
            pos.Enable();
            delta.Enable();
            press.Enable();
            */
        }
    }
}
