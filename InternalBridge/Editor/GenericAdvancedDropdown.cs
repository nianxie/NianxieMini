using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Nianxie.Editor
{
    public class GenericAdvancedDropdown:AdvancedDropdown
    {
        private SortedDictionary<string, string> key2display;
        private string[] keys;
        private System.Action<string> onSelected;

        public GenericAdvancedDropdown(AdvancedDropdownState state, SortedDictionary<string, string> key2display, System.Action<string> onSelected) : base(state)
        {
            this.key2display = key2display;
            this.onSelected = onSelected;
            keys = key2display.Keys.ToArray();
            minimumSize = new Vector2(250, 300);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("");
            for (int i = 0; i < keys.Length; i++)
            {
                AddPath(root, i, keys[i], key2display[keys[i]]);
            }
            return root;
        }

        private void AddPath(AdvancedDropdownItem root, int index, string key, string display)
        {
            string[] parts = key.Split('.');
            AdvancedDropdownItem current = root;
            for (int i = 0; i < parts.Length; i++)
            {
                AdvancedDropdownItem child = null;
                // 查找有下一级的同名节点
                if (current.children != null)
                {
                    foreach (var c in current.children)
                    {
                        if (c.name == parts[i] && c.id == -1) { child = c; break; }
                    }
                }

                if (child == null)
                {
                    child = new AdvancedDropdownItem(parts[i]);
                    if (i == parts.Length - 1)
                    {
                        // 如果没有下一级节点，则id赋值为index
                        child.id = index;
                        child.displayName = $"{parts[i]} - {display}";
                    }
                    else
                    {
                        // 如果有下一级节点，则id为-1
                        child.id = -1;
                    }
                    current.AddChild(child);
                }
                current = child;
            }
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            onSelected(keys[item.id]);
        }
    }
}
