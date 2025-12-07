using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
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

    public abstract class EasyState
    {
    }

    public abstract class EasyView<TView> : EasyView where TView:EasyView
    {
        public void Apply(Action<TView> applySelf)
        {
            applySelf((TView)(EasyView)this);
        }
    }

    public abstract class EasyView
    {
        public void SetDisplay(bool display)
        {
            node.SetDisplay(display);
        }

        public VisualElement node;
        /// <summary>
        /// 使用c#的反射能力，基于uxml的命名自动绑定view的属性
        /// </summary>
        private static EasyView CreateByQuery(VisualElement root, Type type)
        {
            var view = (EasyView)Activator.CreateInstance(type);
            foreach(var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)){
                if (field.FieldType.IsSubclassOf(typeof(VisualElement)) || field.FieldType == typeof(VisualElement))
                {
                    var value = root.Q(field.Name);
                    field.SetValue(view, value);
                } else if (field.FieldType.IsSubclassOf(typeof(EasyView)))
                {
                    var value = root.Q(field.Name);
                    if (value != null)
                    {
                        var child = CreateByQuery(value, field.FieldType);
                        child.node = value;
                        field.SetValue(view, child);
                    }
                }
            }
            return view;
        }

        public static TView CreateByQuery<TView>(VisualElement root) where TView:EasyView, new()
        {
            var view = (TView)CreateByQuery(root, typeof(TView));
            return view;
        }
    }

    public abstract class EasyWindow<TView, TState> : EditorWindow where TView:EasyView, new() where TState:EasyState, new()
    {
        [SerializeField]
        private VisualTreeAsset uxmlAsset = default;
        protected TView view;
        protected TState state;

        protected virtual void Setup()
        {
        }
        protected virtual void Refresh()
        {
        }

        public void CreateGUI()
        {
            uxmlAsset.CloneTree(rootVisualElement);
            view = EasyView.CreateByQuery<TView>(rootVisualElement);
            state = new TState();
            Setup();
        }
    }
}
