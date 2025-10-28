using UnityEngine;
using UnityEditor;
using System.IO;
using Nianxie.Craft;
using Nianxie.Utils;
using XLua;

namespace Nianxie.Editor
{
    [CustomEditor(typeof(EditRoot), true)]
    public class EditRootEditor:UnityEditor.Editor
    {

        protected EditRoot editRoot;
        protected void OnEnable()
        {
            editRoot = (EditRoot) target;
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
                var (jsonBytes, pngData) = editRoot.PackJsonPng();
                var selectPath = EditorUtility.SaveFilePanel("Save Craft", Path.Combine(Application.dataPath, ".."), "craft", "json,png");
                var (jsonPath, pngPath) = ToJsonPngPath(selectPath);
                File.WriteAllBytes(jsonPath, jsonBytes.data);
                File.WriteAllBytes(pngPath, pngData);
                EditorUtility.RevealInFinder(Path.GetDirectoryName(jsonPath));
            }

            if (GUILayout.Button("Load"))
            {
                var selectPath = EditorUtility.OpenFilePanel("Load Craft", Path.Combine(Application.dataPath, ".."), "json,png");
                var (jsonPath, pngPath) = ToJsonPngPath(selectPath);
                var jsonBytes = new LargeBytes(File.ReadAllBytes(jsonPath));
                var pngData = File.ReadAllBytes(pngPath);
                editRoot.UnpackJsonPng(jsonBytes, pngData);
            }
        }
    }
}