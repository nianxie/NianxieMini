using System;
using System.Collections;
using System.Collections.Generic;
using Nianxie.Craft;
using UnityEngine;

namespace Nianxie.Components
{
        
    public enum FitViewAxis
    {
        Horizontal = 0,
        Vertical = 1,
    }
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class OrthoCameraFit : MonoBehaviour
    {
        private Camera _camera;
    
        // 提供公共访问属性
        public Camera camera
        {
            get 
            {
                // 延迟初始化：如果尚未获取，则在首次访问时获取
                if (_camera == null)
                {
                    _camera = GetComponent<Camera>();
                }
                return _camera;
            }
        }


        public FitViewAxis fitViewAxis;
        
        public float fitSize = 5;
        
        void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        void OnEnable()
        {
            Refresh();
        }

        void Refresh()
        {
            var orthoCam = camera;
            if (!orthoCam.orthographic)
            {
                orthoCam.orthographic = true;
            }

            if (fitViewAxis == FitViewAxis.Vertical)
            {
                orthoCam.orthographicSize = fitSize;
            }
            else
            {
                var camRect = orthoCam.pixelRect;
                // 根据水平尺寸计算对应的垂直尺寸
                // 正交相机的size是半高，所以需要除以2
                float verticalSize = fitSize * camRect.height / camRect.width;
                orthoCam.orthographicSize = verticalSize;
            }
        }
        
#if UNITY_EDITOR
        void OnValidate()
        {
            Refresh();
        }

        void Reset()
        {
            Refresh();
        }
#endif
    }
}
