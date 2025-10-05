using UnityEngine;

namespace Nianxie.Utils
{
    public static class NianxieUnityExtension {
        /// <returns>If the game object has been marked as destroyed by UnityEngine</returns>
        public static bool IsDestroyed(this GameObject gameObject)
        {
            // UnityEngine overloads the == opeator for the GameObject type
            // and returns null when the object has been destroyed, but 
            // actually the object is still there but has not been cleaned up yet
            // if we test both we can determine if the object has been destroyed.
            return gameObject == null;
        }

        public static void OverrideBy(this RectTransform target, RectTransform source)
        {
            target.anchorMin = source.anchorMin;
            target.anchorMax = source.anchorMax;
            target.pivot = source.pivot;
            target.offsetMin = source.offsetMin;
            target.offsetMax = source.offsetMax;
            target.anchoredPosition = source.anchoredPosition;
            target.sizeDelta = source.sizeDelta;
            target.rotation = source.rotation;
            target.localScale = source.localScale;
        }

        public static Rect CalcRelativeRect(this RectTransform inTrans, RectTransform outTrans)
        {
            var inRect = inTrans.rect;
            var outWorldToLocalMatrix = outTrans.worldToLocalMatrix;
            var inLocalToWorldMatrix = inTrans.localToWorldMatrix;
            var outRect = outTrans.rect;
            Vector3 inLeftBottom = outWorldToLocalMatrix.MultiplyPoint3x4(inLocalToWorldMatrix.MultiplyPoint(inRect.min));
            Vector3 inRightTop = outWorldToLocalMatrix.MultiplyPoint3x4(inLocalToWorldMatrix.MultiplyPoint(inRect.max));
            Vector3 outLeftBottom = outWorldToLocalMatrix.MultiplyPoint3x4(outTrans.localToWorldMatrix.MultiplyPoint(outRect.min));
            Vector3 rectPos = inLeftBottom - outLeftBottom;
            Vector3 rectSize = inRightTop - inLeftBottom;
            return new Rect(rectPos.x, rectPos.y, rectSize.x, rectSize.y);
        }

        public static Vector3 ToVector3(this Vector2 vec2, float z=0.0f)
        {
            return new Vector3(vec2.x, vec2.y, z);
        }
        public static Vector2 ToVector2(this Vector3 vec3)
        {
            return new Vector2(vec3.x, vec3.y);
        }
        public static Component Fork(this Component com, Transform parent) {
	        return Object.Instantiate(com, parent, false).GetComponent(com.GetType());
        }
        public static GameObject Fork(this GameObject go, Transform parent)
        {
            return Object.Instantiate(go, parent, false);
        }
    }
}