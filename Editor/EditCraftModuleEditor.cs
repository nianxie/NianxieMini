using UnityEngine;
using UnityEditor;
using System.IO;
using Nianxie.Craft;
using XLua;

namespace Nianxie.Editor
{
    [CustomEditor(typeof(EditCraftModule), true)]
    public class EditCraftModuleEditor:UnityEditor.Editor
    {

        protected EditCraftModule editCraftModule;
        protected void OnEnable()
        {
            editCraftModule = (EditCraftModule) target;
        }

        public (string, string) ToJsonPngPath(string selectPath)
        {
            var jsonPath = $"{Path.GetDirectoryName(selectPath)}/{Path.GetFileNameWithoutExtension(selectPath)}.json";
            var pngPath = $"{Path.GetDirectoryName(selectPath)}/{Path.GetFileNameWithoutExtension(selectPath)}.png";
            return (jsonPath, pngPath);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Save"))
            {
                var (jsonBytes, pngData) = editCraftModule.PackJsonPng();
                var selectPath = EditorUtility.SaveFilePanel("Save Craft", Path.Combine(Application.dataPath, ".."), "craft", "json,png");
                var (jsonPath, pngPath) = ToJsonPngPath(selectPath);
                File.WriteAllBytes(jsonPath, jsonBytes);
                File.WriteAllBytes(pngPath, pngData);
                EditorUtility.RevealInFinder(Path.GetDirectoryName(jsonPath));
            }

            if (GUILayout.Button("Load"))
            {
                var selectPath = EditorUtility.OpenFilePanel("Load Craft", Path.Combine(Application.dataPath, ".."), "json,png");
                var (jsonPath, pngPath) = ToJsonPngPath(selectPath);
                var jsonBytes = File.ReadAllBytes(jsonPath);
                var pngData = File.ReadAllBytes(pngPath);
                editCraftModule.UnpackJsonPng(jsonBytes, pngData);
            }
        }
    }
}