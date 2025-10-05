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
        public void Main(PreviewManager previewManager, string projectName)
        {
            var iosBundleName = $"{NianxieConst.MiniBundlesOutput}/{projectName}/{projectName}_iOS.bundle";
            var androidBundleName = $"{NianxieConst.MiniBundlesOutput}/{projectName}/{projectName}_Android.bundle";
            mainBtn.onClick.AddListener(() => { 
                previewManager.LoadProject(projectName, null);
            });
            mainText.text = projectName;
            if (File.Exists(iosBundleName))
            {
                iosBtn.onClick.AddListener(() => { 
                    previewManager.LoadProject(projectName, iosBundleName);
                });
            }
            else
            {
                iosBtn.interactable = false;
            }

            if (File.Exists(androidBundleName))
            {
                androidBtn.onClick.AddListener(() => { 
                    previewManager.LoadProject(projectName, androidBundleName);
                });
            }
            else
            {
                androidBtn.interactable = false;
            }
            //newRect.GetComponentInChildren<Text>(true).text = project;
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
