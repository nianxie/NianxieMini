using System;
using Nianxie.Utils;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Nianxie.Craft
{
    public class SlotCreate
    {
        class StandardResource
        {
            public TMPModify_ShellFont shellFont;
            public Sprite sliced9;
        }

        private static StandardResource standRes;

        private static StandardResource LoadStandRes()
        {
            if (standRes == null)
            {
                standRes = new();
                var guids = AssetDatabase.FindAssets($"t:{nameof(TMPModify_ShellFont)}", new[] {NianxieConst.MiniDefaultAssets});
                if (guids.Length > 0)
                {
                    standRes.shellFont = AssetDatabase.LoadAssetAtPath<TMPModify_ShellFont>(AssetDatabase.AssetPathToGUID(guids[0]));
                }
                standRes.sliced9 = AssetDatabase.LoadAssetAtPath<Sprite>(NianxieConst.Sliced9Path);
            }
            return standRes;
        }

        [MenuItem("GameObject/NianxieCraft/"+nameof(TextSlot), false, 100)]
        public static void AddTextSlot(MenuCommand command)
        {
            AddSlotCom<TextSlot>(command, (com) =>
            {
                var textMesh = com.GetComponent<TextMeshPro>();
                textMesh.enableAutoSizing = true;
                textMesh.fontSizeMin = 0.1f;
                textMesh.fontSizeMax = 72.0f;
                textMesh.sortingOrder = 2;
                textMesh.alignment = TextAlignmentOptions.Center;
                textMesh.horizontalAlignment = HorizontalAlignmentOptions.Center;
                textMesh.text = "(text slot)";
                var shellFont = LoadStandRes().shellFont;
                if (shellFont != null)
                {
                    textMesh.SetFont(shellFont, null);
                }
                var rect = com.GetComponent<RectTransform>();
                var defaultSize = new Vector2(5, 1);
                rect.sizeDelta = defaultSize;
                GameObject go = new GameObject("background");
                go.transform.parent = com.transform;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                var bgSprite = go.AddComponent<SpriteRenderer>();
                bgSprite.drawMode = SpriteDrawMode.Sliced;
                bgSprite.sortingOrder = 1;
                bgSprite.sprite = LoadStandRes().sliced9;
                bgSprite.size = defaultSize;
                bgSprite.color = new Color(0,0,0, 0.8f);
                com.background = bgSprite;
            });
        }
        
        [MenuItem("GameObject/NianxieCraft/"+nameof(SpriteSlot), false, 101)]
        public static void AddSpriteSlot(MenuCommand command)
        {
            AddSlotCom<SpriteSlot>(command);
        }

        private static void AddSlotCom<T>(MenuCommand command, Action<T> extraOper=null) where T : AbstractSlotCom
        {
            var comName = typeof(T).Name;
            GameObject go = new GameObject(comName);
            var parent = command.context as GameObject;
            if (parent != null)
            {
                go.transform.parent = parent.transform;
            }
            else
            {
                StageUtility.PlaceGameObjectInCurrentStage(go);
            }
            go.transform.localRotation = Quaternion.identity;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            var com = go.AddComponent<T>();
            extraOper?.Invoke(com);
            Selection.activeObject = go;
            Undo.RegisterCreatedObjectUndo(go, $"Create {comName} Object");
        }
    }
}