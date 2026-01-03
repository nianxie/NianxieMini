using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Nianxie.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Nianxie.Preview
{
    public class PreviewMiniButtons : MonoBehaviour
    {
        public Button mainBtn;
        public Text mainText;
        public Button iosBtn;
        public Button androidBtn;
        public Image folderCraftable;
        public Image bundleCraftable;
        public void Main(Action<string, string> loadProjectOrBundle, PreviewMiniInfo miniInfo)
        {
            var folder = miniInfo.folder;
            mainBtn.onClick.AddListener(() => { 
                loadProjectOrBundle(folder, null);
            });
            mainText.text = folder;
            folderCraftable.sprite = miniInfo.config.craftable?PreviewAssets.instance.iconCraft:PreviewAssets.instance.iconGame;
            if (miniInfo.bundleInfo != null)
            {
                bundleCraftable.sprite = miniInfo.bundleInfo.config.craftable?PreviewAssets.instance.iconCraft:PreviewAssets.instance.iconGame;
                iosBtn.onClick.AddListener(() => { 
                    loadProjectOrBundle(folder, miniInfo.bundleInfo.iosBundle);
                });
                androidBtn.onClick.AddListener(() => { 
                    loadProjectOrBundle(folder, miniInfo.bundleInfo.androidBundle);
                });
            }
            else
            {
                iosBtn.interactable = false;
                androidBtn.interactable = false;
            }
            //newRect.GetComponentInChildren<Text>(true).text = project;
        }
    }
}
