using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Reflection;
using Nianxie.Components;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEditor.SceneManagement;
using XLua;

namespace Nianxie.Editor
{
    
    [InitializeOnLoad]
    public static class EditorWatchDog
    {
        public static System.Func<GameObject, (bool, Color)> customColorFunc = null;
        
        private static bool CheckVersion(int version, int major, string minor)
        {
            string[] v = Application.unityVersion.Split('.');
            int t = int.Parse(v[0]);
            if (t < version)
                return false;
            else if (t > version)
                return true;

            t = int.Parse(v[1]);
            if (t < major)
                return false;
            else if (t > major)
                return true;

            if (!string.Equals(v[2], minor))
                Debug.LogWarning("[EditorWatchDog] inconsistent minor ver.");
            return true;
        }
        
        
        // 场景树节点中显示lua script的属性和报错
        static void HierarchyItemCB(int instanceID, Rect selectionRect)
        {
            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null) return;
            
            // show sorting order, TODO, to be removed
            var spriteRenderer = go.GetComponent<SpriteRenderer>();
            var canvas = go.GetComponent<Canvas>();
            string order = "";
            if (spriteRenderer != null) {
                order = spriteRenderer.sortingOrder.ToString();
            } else if (canvas != null) {
                order = canvas.sortingOrder.ToString();
            }
            if (order != "")
            {
                var orderRect = new Rect(32, selectionRect.yMin, 30, selectionRect.height);
                GUI.Label(orderRect, order);
            }
            

            var iconRect = new Rect(selectionRect.xMin, selectionRect.yMin, selectionRect.height, selectionRect.height);
            if (customColorFunc != null)
            {
                var (useCustomColor, color) = customColorFunc(go);
                if (useCustomColor)
                {
                    GUI.DrawTexture(iconRect, prefabIcon, ScaleMode.StretchToFill, true, 0, color, 0, 0);
                }
            }

            // show custom 
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null && EditorEnvPaths.TryMapEnvPaths(prefabStage.assetPath, out var envPaths))
            {
                var luaBehav = go.GetComponentInParent<LuaBehaviour>(true);
                if (luaBehav != null && !string.IsNullOrEmpty(luaBehav.classPath))
                {
                    if (luaBehav.gameObject==go)
                    {
                        // 显示prefab icon:是view还是entity还是scene
                        GUI.DrawTexture(iconRect, prefabIcon, ScaleMode.StretchToFill, true, 0, luaBehav.ToIconColor(), 0, 0);
                        // 检查lua object的属性中node值的异常，并将节点标记出来
                        var reflectInfo = envPaths.reflectEnv.GetWarmedReflect(luaBehav.classPath, luaBehav.nestedKeys);
                        if (reflectInfo is ErrorReflectClass errReflect)
                        {
                            var errRect = new Rect(selectionRect);
                            errRect.x = errRect.xMax - 100;
                            errRect.width = 50;
                            if (GUI.Button(errRect, "error"))
                            {
                                Debug.Log($"{luaBehav.whichClass} script error : {errReflect.whichClass}");
                            }
                        }
                        else
                        {
                            var missingInjections = reflectInfo.injections.Where(a => envPaths.reflectEnv.CheckClassFieldMissing(luaBehav, a)).ToArray();
                            if (missingInjections.Length > 0)
                            {
                                var missingRect = new Rect(selectionRect);
                                missingRect.x = missingRect.xMax - 100;
                                missingRect.width = 55;
                                if (GUI.Button(missingRect, "missing"))
                                {
                                    Debug.Log($"{luaBehav.whichClass} field error : {string.Join(",", missingInjections.Select(a=>a.key))}");
                                }
                            }
                        }
                    }
                    else
                    {
                        // 显示lua object的属性
                        var nodeInjections = envPaths.reflectEnv.GetWarmedReflect(luaBehav.classPath, luaBehav.nestedKeys).nodeInjections;
                        foreach(var firstInjection in nodeInjections) {
                            foreach (var nodePath in firstInjection.EachNodePath())
                            {
                                var node = firstInjection.ToNodeObject(luaBehav, nodePath);
                                if (node != null)
                                {
                                    var com = node as Component;
                                    if (com?.gameObject==go || node == go)
                                    {
                                        var fieldRect = new Rect(selectionRect);
                                        fieldRect.x = fieldRect.xMax - 50;
                                        fieldRect.width = 50;
                                        GUI.Label(fieldRect, firstInjection.key);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static EditorWatchDog()
        {
            // TODO 这里检查一下版本，以后升级到稳定一些的版本再固定
            // Kanglai: only check in the first minute...
            if (EditorApplication.timeSinceStartup < 60)
            {
                if (Application.unityVersion != NianxieConst.UNITY_VERSION)
                {
                    Debug.LogWarning($"[EditorWatchDog] wrong unity version : {Application.unityVersion}. {NianxieConst.UNITY_VERSION} expected");
                }
            }

            EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
            var EditorWatchDogType = typeof(EditorWatchDog);
            {
                var ObjectListAreaType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ObjectListArea");
                var postAssetIconDrawCallbackEvent = ObjectListAreaType.GetEvent("postAssetIconDrawCallback", BindingFlags.Static | BindingFlags.NonPublic);
                var OnAssetIconDrawDelegate = System.Delegate.CreateDelegate(postAssetIconDrawCallbackEvent.EventHandlerType, EditorWatchDogType.GetMethod(nameof(OnAssetIconDrawForListArea), BindingFlags.Static | BindingFlags.NonPublic));
                postAssetIconDrawCallbackEvent.GetAddMethod(true).Invoke(null, new object[] { OnAssetIconDrawDelegate });
            }
            {
                var AssetsTreeViewGUIType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.AssetsTreeViewGUI");
                var postAssetIconDrawCallbackEvent = AssetsTreeViewGUIType.GetEvent("postAssetIconDrawCallback", BindingFlags.Static | BindingFlags.NonPublic);
                var OnAssetIconDrawDelegate = System.Delegate.CreateDelegate(postAssetIconDrawCallbackEvent.EventHandlerType, EditorWatchDogType.GetMethod(nameof(OnAssetIconDrawForTreeView), BindingFlags.Static | BindingFlags.NonPublic));
                postAssetIconDrawCallbackEvent.GetAddMethod(true).Invoke(null, new object[] { OnAssetIconDrawDelegate });
            }
            folderIcon = EditorGUIUtility.FindTexture("folder icon");
            prefabIcon = EditorGUIUtility.FindTexture("prefab icon");
        }
        static Texture2D folderIcon;
        static Texture2D prefabIcon;

        static Color ToIconColor(this LuaBehaviour luaBehav)
        {
            if (luaBehav is MiniBehaviour)
            {
                return new Color(1, 0.59f, 1);
            }
            else if (luaBehav.GetType() != typeof(LuaBehaviour))
            {
                return new Color(0.8f, 0.6f, 0.6f);
            }
            else
            {
                return new Color(1, 0.59f, 0.50f);
            }
        }
        // project window ListArea部分的item绘制
        static void OnAssetIconDrawForListArea(Rect iconRect, string guid, bool isListMode)
        {
            if (DrawIconForLuafabOrEditor(iconRect, guid, out var path))
            {
                return;
            }
            if (!Directory.Exists(path))
            {
                return;
            }

            if (EditorEnvPaths.TryMapEnvPaths(path, out var envPaths) && envPaths is MiniEditorEnvPaths miniEnvPaths && envPaths.pathPrefix == path)
            {
                GUI.DrawTexture(iconRect, folderIcon, ScaleMode.StretchToFill, true, 0, new Color(0.1f,0.1f,0.1f), 0, 0);
                var miniName = miniEnvPaths.GetCachedName();
                if (!isListMode)
                {
                    var style = new GUIStyle(EditorStyles.boldLabel)
                    {
                        normal = { textColor = Color.white},
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold,
                        wordWrap = false
                    };
                    GUI.Label(iconRect, miniName, style);
                }
                else
                {
                    var rect = new Rect(iconRect.x + 200, iconRect.y, iconRect.width + 80, iconRect.height);
                    var style = new GUIStyle(EditorStyles.boldLabel)
                    {
                        normal = { textColor = Color.white},
                        alignment = TextAnchor.MiddleRight,
                        fontStyle = FontStyle.Bold,
                        wordWrap = false
                    };
                    GUI.Label(rect, miniName, style);
                }
            }
        }

        // project window TreeView部分的item绘制
        static void OnAssetIconDrawForTreeView(Rect iconRect, string guid)
        {
            if (DrawIconForLuafabOrEditor(iconRect, guid, out var path))
            {
                return;
            }

            if (!Directory.Exists(path))
            {
                return;
            }

            if (EditorEnvPaths.TryMapEnvPaths(path, out var envPaths) && envPaths is MiniEditorEnvPaths miniEnvPaths && envPaths.pathPrefix == path)
            {
                GUI.DrawTexture(iconRect, folderIcon, ScaleMode.StretchToFill, true, 0, Color.black, 0, 0);
                var rect = new Rect(iconRect.x + 120, iconRect.y, iconRect.width + 300, iconRect.height);
                GUI.Label(rect, miniEnvPaths.GetCachedName());
            }
        }
        static bool DrawIconForLuafabOrEditor(Rect iconRect, string guid, out string path)
        {
            path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(".prefab"))
            {
                if (EditorEnvPaths.TryMapEnvPaths(path, out var envPaths))
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    var luaBehav = prefab.GetComponent<LuaBehaviour>();
                    if(luaBehav != null)
                    {
                        GUI.DrawTexture(iconRect, prefabIcon, ScaleMode.StretchToFill, true, 0, luaBehav.ToIconColor(), 0, 0);
                    } 
                }

                return true;
            }
            // https://docs.unity3d.com/Manual/SpecialFolders.html
            var folderName = Path.GetFileNameWithoutExtension(path);
            if (folderName == "Editor" || folderName == "Editor Default Resources" || folderName == "Gizmos")
            {
                GUI.DrawTexture(iconRect, folderIcon, ScaleMode.StretchToFill, true, 0, Color.green, 0, 0);
                return true;
            }
            return false;
        }
    }
}
