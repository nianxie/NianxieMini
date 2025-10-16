using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using Nianxie.Components;
using Nianxie.Framework;
using XLua;

namespace Nianxie.Editor
{
    using Utils;
    // The custom editor of the SgLuaMonoBehaviourEditor class.
    [CustomEditor(typeof(LuaBehaviour), true)]
    public class LuaBehaviourEditor : UnityEditor.Editor
    {
        // The lua params property.
        protected LuaBehaviour m_behav = null;
        protected GUIStyle m_errStyle = null;

        protected void OnEnable()
        {
            m_behav = (LuaBehaviour) target;
        }
        private void drawMultipleInjection(AbstractReflectEnv reflectEnv, Rect rect, AbstractMultipleInjection injection)
        {
            var rect1 = new Rect(rect.x, rect.y, rect.width/3, rect.height);
            EditorGUI.LabelField(rect1, injection.key);
            for (var i = 0; i < Mathf.Max(injection.count, 1); i++)
            {
                var y = rect.y + EditorGUIUtility.singleLineHeight * i;
                var rect2 = new Rect(rect.x+rect.width/3, y, rect.width/3, EditorGUIUtility.singleLineHeight);
                var rect3 = new Rect(rect.x+2*rect.width/3, y, rect.width/3, EditorGUIUtility.singleLineHeight);
                var rect23 = new Rect(rect.x+rect.width/3, y, 2*rect.width/3, EditorGUIUtility.singleLineHeight);
                if(injection is AssetInjection assetInjection)
                {
                    foreach (var assetPath in assetInjection.EachAssetPath())
                    {
                        var obj = AssetDatabase.LoadAssetAtPath(assetPath, injection.csharpType);
                        if (obj == null)
                        {
                            EditorGUI.LabelField(rect23, $"asset missing : {assetPath}", m_errStyle);
                        }
                        else
                        {
                            EditorGUI.ObjectField(rect3, obj, null, false);
                        }
                    }
                } else if (injection is SubAssetInjection subAssetInjection)
                {
                    var assetDict = AssetDatabase.LoadAllAssetsAtPath(subAssetInjection.assetPath).ToDictionary((a) => a.name);
                    foreach (var subName in subAssetInjection.EachSubName())
                    {
                        if (assetDict.TryGetValue(subName, out var obj))
                        {
                            EditorGUI.ObjectField(rect3, obj, null, false);
                        }
                        else
                        {
                            EditorGUI.LabelField(rect23, $"asset missing : {subAssetInjection.assetPath} {subName}", m_errStyle);
                        }
                    }
                } else if (injection is AbstractNodeInjection nodeInjection)
                {
                    foreach (var nodePath in nodeInjection.EachNodePath())
                    {
                        var obj = nodeInjection.ToNodeObject(m_behav, nodePath);
                        if(obj == null)
                        {
                            EditorGUI.LabelField(rect23, $"node missing : {nodePath}", m_errStyle);
                        }
                        else
                        {
                            if (reflectEnv is EditorReflectEnv editorReflectEnv && nodeInjection is ScriptInjection scriptInjection && !editorReflectEnv.CheckFieldClassMatch(scriptInjection, obj as LuaBehaviour))
                            {
                                EditorGUI.LabelField(rect23, $"class mismatch : {nodePath}", m_errStyle);
                            }
                            else
                            {
                                EditorGUI.ObjectField(rect2, obj, injection.csharpType, false);
                            }
                        }
                    }
                }
                else
                {
                    EditorGUI.LabelField(rect23, injection.csharpType?.ToString());
                }
            }
        }

        private void drawSingleInjection(AbstractReflectEnv reflectEnv, Rect rect, AbstractReflectInjection injection)
        {
            var rect1 = new Rect(rect.x, rect.y, rect.width/3, rect.height);
            var rect23 = new Rect(rect.x+rect.width/3, rect.y, 2*rect.width/3, rect.height);
            EditorGUI.LabelField(rect1, injection.key);
            if(injection is LuafabInjection luafabInjection)
            {
                var rect3 = new Rect(rect.x+2*rect.width/3, rect.y, rect.width/3, rect.height);
                var obj = AssetDatabase.LoadAssetAtPath(luafabInjection.assetPath, typeof(LuaBehaviour));
                if (obj == null)
                {
                    EditorGUI.LabelField(rect23, $"asset missing : {luafabInjection.assetPath}", m_errStyle);
                }
                else
                {
                    EditorGUI.ObjectField(rect3, obj, typeof(LuaBehaviour), false);
                }
            } else 
            {
                EditorGUI.LabelField(rect23, injection.csharpType?.ToString());
            }
        }

        private EditorReflectEnv CheckReflectEnvAndRefreshPath(PrefabStage prefabStage)
        {
            var assetPath = prefabStage.assetPath;
            if (!EditorEnvPaths.TryMapEnvPaths(assetPath, out var envPaths))
            {
                EditorGUILayout.HelpBox("error, invalid path for luafab..", MessageType.Error);
                return null;
            }

            if (!prefabStage.prefabContentsRoot.TryGetComponent<LuaBehaviour>(out var rootBehav))
            {
                EditorGUILayout.HelpBox("error, prefab root has no LuaBehaviour..", MessageType.Error);
                return null;
            }

            var reflectEnv = envPaths.reflectEnv;

            // 如果没有在运行，则检查是否对luaBehaviour的class path进行赋值
            if (!EditorApplication.isPlaying)
            {

                var behavIdToReflectClass = new Dictionary<int, WarmedReflectClass>();
                // 递归检查 nestedKey
                void recurCheckNestedKeys(WarmedReflectClass reflectClass, LuaBehaviour behav)
                {
                    if (behavIdToReflectClass.TryGetValue(behav.GetInstanceID(), out var existReflectClass))
                    {
                        Debug.LogError($"one LueBehaviour match two class : {reflectClass.whichClass} {existReflectClass.whichClass}");
                        return;
                    }
                    behavIdToReflectClass[behav.GetInstanceID()] = reflectClass;

                    if (reflectClass is ErrorReflectClass)
                    {
                        return;
                    }
                    if (behav.classPath != reflectClass.classPath)
                    {
                        return;
                    }

                    if (behav.nestedKeys == null || !behav.nestedKeys.SequenceEqual(reflectClass.nestedKeys))
                    {
                        behav.nestedKeys = reflectClass.nestedKeys;
                        EditorUtility.SetDirty(behav);
                    }
                    foreach (var nestedInjection in reflectClass.eachNestedInjection)
                    {
                        foreach (var nodePath in nestedInjection.EachNodePath())
                        {
                            var nestedBehav = nestedInjection.ToLuaBehaviour(behav, nodePath);
                            if (nestedBehav != null)
                            {
                                recurCheckNestedKeys(nestedInjection.nestedClass, nestedBehav);
                            }
                        }
                    }
                }
                var rootReflectClass = reflectEnv.GetFileWarmedReflect(rootBehav.classPath);
                recurCheckNestedKeys(rootReflectClass, rootBehav);
                
                var assignScriptPath = reflectEnv.envPaths.assetPath2classPath(assetPath);
                var prefabAssetType = PrefabUtility.GetPrefabAssetType(m_behav.gameObject);
                if (prefabAssetType == PrefabAssetType.NotAPrefab)
                {
                    if (m_behav.classPath != assignScriptPath)
                    {
                        m_behav.classPath = assignScriptPath;
                        EditorUtility.SetDirty(m_behav);
                    }

                    if (!behavIdToReflectClass.ContainsKey(m_behav.GetInstanceID()))
                    {
                        EditorGUILayout.HelpBox("error, no nested class match for this LuaBehaviour..", MessageType.Error);
                    }
                }
            }

            return reflectEnv;
        }

        protected virtual void DrawInjections(AbstractReflectEnv reflectEnv, WarmedReflectClass reflectInfo)
        {
            EditorGUI.BeginDisabledGroup(true);
            var injectionList = new ReorderableList(reflectInfo.injections, typeof(AbstractReflectInjection), false, true, false, false);
            injectionList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Lua Injection List");
            injectionList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var injection = reflectInfo.injections[index];
                if (injection is AbstractMultipleInjection collectionInjection)
                {
                    drawMultipleInjection(reflectEnv, rect, collectionInjection);
                }
                else
                {
                    drawSingleInjection(reflectEnv, rect, injection);
                }
            };
            injectionList.elementHeightCallback = (index) =>
            {
                var injection = reflectInfo.injections[index];
                if (injection is AbstractMultipleInjection collectionInjection)
                {
                    return EditorGUIUtility.singleLineHeight * Mathf.Max(collectionInjection.count, 1);
                }
                else
                {
                    return EditorGUIUtility.singleLineHeight;
                }
            };
            injectionList.DoLayoutList();
            EditorGUI.EndDisabledGroup();
        }

        public override void OnInspectorGUI()
        {
            if (m_errStyle == null)
            {
                m_errStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    normal = { textColor = new Color(1,0.5f,0.5f)},
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Bold,
                    wordWrap = false
                };
            }
            AbstractReflectEnv reflectEnv = null;
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                reflectEnv = CheckReflectEnvAndRefreshPath(prefabStage);
            }
            else
            {
                var prefabAssetType = PrefabUtility.GetPrefabAssetType(m_behav.gameObject);
                if (prefabAssetType != PrefabAssetType.NotAPrefab)
                {
                    var assetPath = AssetDatabase.GetAssetPath(m_behav.gameObject);
                    if (!EditorEnvPaths.TryMapEnvPaths(assetPath, out var envPaths))
                    {
                        EditorGUILayout.HelpBox("error, invalid path for luafab.", MessageType.Warning);
                        return;
                    }
                    reflectEnv = envPaths.reflectEnv;
                } else if (EditorApplication.isPlaying)
                {
                    reflectEnv = m_behav.gameManager?.reflectEnv;
                }
            }

            if (reflectEnv == null)
            {
                EditorGUILayout.HelpBox("error, env not found", MessageType.Warning);
                return;
            }

            var classPath = m_behav.classPath;
            EditorGUILayout.ObjectField(m_behav.whichClass, reflectEnv.searchTextAssetForRequire(ref classPath), typeof(TextAsset), false);
            var reflectInfo = reflectEnv.GetWarmedReflect(m_behav.classPath, m_behav.nestedKeys);
            if (reflectInfo is ErrorReflectClass errReflect)
            {
                EditorGUILayout.HelpBox($"error class: {errReflect.message}", MessageType.Error);
                return;
            }
            if (reflectInfo.behavType != m_behav.GetType())
            {
                EditorGUILayout.HelpBox($"error behavType: script match {reflectInfo.behavType} but got {m_behav.GetType()}", MessageType.Error);
                return;
            }
            
            DrawInjections(reflectEnv, reflectInfo);
        }
    }
}
