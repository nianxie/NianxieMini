using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nianxie.Editor
{
    public static class VisualElementExtension
    {
        public static void SetDisplay(this VisualElement element, bool display)
        {
            element.style.display = display?DisplayStyle.Flex:DisplayStyle.None;
        }
    }

    public abstract class EasyHierarchy
    {
        /// <summary>
        /// 使用c#的反射能力，基于uxml的命名自动绑定view的属性
        /// </summary>
        private static EasyHierarchy CreateByQuery(VisualElement root, Type type)
        {
            var view = (EasyHierarchy)Activator.CreateInstance(type);
            foreach(var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)){
                if (field.FieldType.IsSubclassOf(typeof(VisualElement)) || field.FieldType == typeof(VisualElement))
                {
                    var value = root.Q(field.Name);
                    field.SetValue(view, value);
                } else if (field.FieldType.IsSubclassOf(typeof(EasyHierarchy)))
                {
                    var value = root.Q(field.Name);
                    if (value != null)
                    {
                        var child = CreateByQuery(value, field.FieldType);
                        field.SetValue(view, child);
                    }
                }
            }
            return view;
        }

        public static TView CreateByQuery<TView>(VisualElement root) where TView:EasyHierarchy, new()
        {
            return (TView)CreateByQuery(root, typeof(TView));
        }
    }

    public abstract class EasyViewModel: ScriptableObject
    {
        [SerializeField]
        private VisualTreeAsset uxmlAsset = default;

        public VisualElement self { get; private set; }
        private VisualElement _root;
        private VisualElement _parent;

        public void SetDisplay(bool display)
        {
            self.SetDisplay(display);
        }

        public static TViewModel CreateViewModelAsNode<TViewModel>(VisualElement node) where TViewModel:EasyViewModel
        {
            var instance = ScriptableObject.CreateInstance<TViewModel>();
            instance.uxmlAsset.CloneTree(node, out int index, out _);
            instance._parent = node;
            instance._root = node[index];
            instance.self = node; 
            return instance;
        }
        public static TViewModel CreateViewModelAsChild<TViewModel>(VisualElement node) where TViewModel:EasyViewModel
        {
            var instance = ScriptableObject.CreateInstance<TViewModel>();
            instance.uxmlAsset.CloneTree(node, out int index, out _);
            instance._parent = node;
            instance._root = node[index];
            instance.self = node[index]; 
            return instance;
        }
        public void RemoveSelf()
        {
            self.RemoveFromHierarchy();
        }
    }
}
