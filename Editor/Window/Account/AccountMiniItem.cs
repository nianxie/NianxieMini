using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nianxie.Editor
{
    public class AccountMiniItem: EasyViewModel
    {
        private const int STEP_NUM = 6;
        public class OutViewHierachy: EasyHierarchy
        {
            public RadioButton radio;
            public Label name;
            public Label miniId;
            public Button selectBtn;
            public Button copyBtn;
            public DropdownField folderDropdown;
            public Button linkBtn;
            public ObjectField folderObject;
            public Button unlinkBtn;
            public Button deleteBtn;
            public VisualElement pipeline;
            public VisualElement linkProject;
            public VisualElement buildBundle;
            public VisualElement uploadBundle;
            public VisualElement buildPackage;
            public VisualElement uploadPackage;
            public VisualElement distribute;
        }
        
        public class StepViewHierachy: EasyHierarchy
        {
            public Label okay;
            public Label info;
            public Button runBtn;
            public Button pingBtn;
        }

        private RemoteMiniState miniState;
        private int index;
        private bool selected;
        private string folder;
        private OutViewHierachy view;
        
        public abstract class AbstractPipelineStep
        {
            protected enum TernaryShow
            {
                ENABLE = 0,
                DISABLE = 1,
                HIDDEN = 2,
            }

            public StepViewHierachy view { get; }
            private Func<RemoteMiniState> stateGetter;
            protected RemoteMiniState miniState => stateGetter();

            protected MiniEditorEnvPaths envPaths
            {
                get
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(miniState.miniId);
                    if (string.IsNullOrEmpty(assetPath))
                    {
                        return null;
                    }
                    return MiniEditorEnvPaths.MapPrefix(assetPath);
                }
            }

            protected abstract bool isOkay();
            
            protected virtual TernaryShow objectFieldShow()
            {
                return TernaryShow.HIDDEN;
            }
            
            protected virtual void dropdownFieldShow(DropdownField dropdownField)
            {
                dropdownField.SetDisplay(false);
            }

            protected virtual bool runBtnEnable()
            {
                return !isOkay();
            }

            protected virtual bool pingBtnEnable()
            {
                return isOkay();
            }

            protected AbstractPipelineStep(VisualElement stepElement, Func<RemoteMiniState> stateGetter)
            {
                view = EasyHierarchy.CreateByQuery<StepViewHierachy>(stepElement);
                this.stateGetter = stateGetter;
            }

            public virtual void Refresh()
            {
                view.okay.text = isOkay() ? "✓" : "?";
                var fontColor = new StyleColor()
                {
                    value=isOkay()?Color.green:Color.yellow,
                };
                view.okay.style.color = fontColor;
                view.info.style.color = fontColor;

                view.runBtn.SetEnabled(runBtnEnable());
                view.pingBtn.SetEnabled(pingBtnEnable());
            }
        }

        public class LinkProjectStep : AbstractPipelineStep
        {
            public string folderSelect = "";
            public LinkProjectStep(OutViewHierachy pipelineView, Func<RemoteMiniState> stateGetter):
                base(pipelineView.linkProject, stateGetter)
            {
                view.runBtn.clicked += () =>
                {
                    Refresh();
                };
                view.pingBtn.clicked += () =>
                {
                    //Refresh();
                    //var dir = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(envPaths.pathPrefix);
                    //EditorGUIUtility.PingObject(dir);
                };
            }
            protected override bool isOkay()
            {
                return envPaths != null && Directory.Exists(envPaths.pathPrefix);
            }
        }
        public class BuildBundleStep: AbstractPipelineStep
        {
            public BuildBundleStep(OutViewHierachy pipelineView, Func<RemoteMiniState> stateGetter):
                base(pipelineView.buildBundle, stateGetter)
            {
                view.runBtn.clicked += () =>
                {
                    BuildMiniWindow.OpenBuildWindow();
                };
                view.pingBtn.clicked += () =>
                {
                    EditorUtility.RevealInFinder(envPaths.finalManifest);
                };
            }
            protected override bool isOkay()
            {
                return envPaths != null && File.Exists(envPaths.finalManifest) && envPaths.finalBundleDict.Values.All(path => File.Exists(path));
            }
        }
        public class UploadBundleStep: AbstractPipelineStep
        {
            public UploadBundleStep(OutViewHierachy pipelineView, Func<RemoteMiniState> stateGetter):
                base(pipelineView.uploadBundle, stateGetter)
            {
                view.runBtn.clicked += () =>
                {
                    UniTask.Create(async () =>
                    {
                        try
                        {
                            await AccountController.UploadBundle(envPaths, (name, progress, total) =>
                            {
                                EditorUtility.DisplayProgressBar("上传文件", $"{progress}/{total} {name}", (progress*1.0f)/total);
                            });
                        } finally {
                            EditorUtility.ClearProgressBar();
                        }
                        Refresh();
                    }).Forget();
                };
                view.pingBtn.clicked += () =>
                {
                    Debug.Log($"资源地址：{miniState.dbMini.manifestUrl} {miniState.dbMini.iosUrl} {miniState.dbMini.androidUrl}");
                };
            }
            protected override bool isOkay()
            {
                var readyStatus = miniState.dbMini.readyStatus;
                return readyStatus==DB_Mini.STATUS_UPLOADED || readyStatus==DB_Mini.STATUS_VIDEO_USED;
            }

            protected override bool runBtnEnable()
            {
                if(envPaths != null && File.Exists(envPaths.finalManifest) && envPaths.finalBundleDict.Values.All(path => File.Exists(path)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class BuildPackageStep: AbstractPipelineStep
        {
            public BuildPackageStep(OutViewHierachy pipelineView, Func<RemoteMiniState> stateGetter):
                base(pipelineView.buildPackage, stateGetter)
            {
            }
            protected override bool isOkay()
            {
                return false;
            }
        }
        public class UploadPackageStep: AbstractPipelineStep
        {
            public UploadPackageStep(OutViewHierachy pipelineView, Func<RemoteMiniState> stateGetter):
                base(pipelineView.uploadPackage, stateGetter)
            {
            }
            protected override bool isOkay()
            {
                return false;
            }
        }
        public class DistributeStep: AbstractPipelineStep
        {
            public DistributeStep(OutViewHierachy pipelineView, Func<RemoteMiniState> stateGetter):
                base(pipelineView.distribute, stateGetter)
            {
            }
            protected override bool isOkay()
            {
                return miniState.dbMini.readyStatus==DB_Mini.STATUS_VIDEO_USED;
            }
            public override void Refresh()
            {
                base.Refresh();
                view.runBtn.SetDisplay(false);
                view.pingBtn.SetDisplay(false);
            }
        }

        public AbstractPipelineStep[] steps;

        public void Setup(Action<int> onClick, Action pageRefresh)
        {
            view = EasyHierarchy.CreateByQuery<OutViewHierachy>(self);
            Func<RemoteMiniState> stateGetter = () => miniState;
            steps = new AbstractPipelineStep[STEP_NUM]
            {
                new LinkProjectStep(view, stateGetter),
                new BuildBundleStep(view, stateGetter),
                new UploadBundleStep(view, stateGetter),
                new BuildPackageStep(view, stateGetter),
                new UploadPackageStep(view, stateGetter),
                new DistributeStep(view, stateGetter),
            };

            void replaceFolderMeta(string folderPath, string newGuid)
            {
                var folderMeta = $"{folderPath}.meta";
                var oldGuid = AssetDatabase.AssetPathToGUID(folderPath);
                if (oldGuid.Length==32 && Directory.Exists(folderPath))
                {
                    var newMeta = File.ReadAllText(folderMeta).Replace($"guid: {oldGuid}", $"guid: {newGuid}");
                    File.WriteAllText(folderMeta, newMeta);
                    AssetDatabase.Refresh();
                    LocalRefresh();
                }
                else
                {
                    Debug.LogError($"{folderPath} is not a valid project");
                }
            }
            view.linkBtn.clicked += () =>
            {
                var conflictPath = AssetDatabase.GUIDToAssetPath(miniState.miniId);
                if (!string.IsNullOrEmpty(conflictPath))
                {
                    File.Delete($"{conflictPath}.meta");
                }
                var folderPath = $"{NianxieConst.MiniPrefixPath}/{view.folderDropdown.value}";
                replaceFolderMeta(folderPath, miniState.miniId);
            };
            view.unlinkBtn.clicked += () =>
            {
                var folderPath = $"{NianxieConst.MiniPrefixPath}/{folder}";
                replaceFolderMeta(folderPath, Guid.NewGuid().ToString("N"));
            };
            view.deleteBtn.clicked += () =>
            {
                UniTask.Create(async () =>
                {
                    if (EditorUtility.DisplayDialog("删除游戏？", $"确认删除{miniState.dbMini.name}吗?", "确认", "取消"))
                    {
                        await AccountController.DeleteMini(miniState.miniId);
                        pageRefresh();
                    }
                }).Forget();
            };
            view.selectBtn.clicked += () =>
            {
                onClick(index);
            };
            view.copyBtn.clicked += () =>
            {
                GUIUtility.systemCopyBuffer = miniState.miniId;
                Debug.Log($"{miniState.dbMini.name}的ID：{miniState.dbMini.miniId}已复制");
            };
            view.radio.RegisterValueChangedCallback((e) =>
            {
                if (e.newValue)
                {
                    onClick(index);
                }
            });
        }

        private void LocalRefresh()
        {
            view.name.text = miniState.dbMini.name;
            view.radio.SetValueWithoutNotify(selected);
            view.deleteBtn.SetDisplay(selected);
            view.miniId.text = miniState.dbMini.miniId;

            var folderPath = AssetDatabase.GUIDToAssetPath(miniState.miniId);
            folder = Path.GetFileName(folderPath);
            view.folderDropdown.choices = BuildMiniWindow.ListProjectFolders();
            if (string.IsNullOrEmpty(folderPath) || folderPath != $"{NianxieConst.MiniPrefixPath}/{folder}")
            {
                view.folderDropdown.value = "(未绑定)";
                view.folderDropdown.SetDisplay(true);
                view.folderObject.SetDisplay(false);
                view.linkBtn.SetDisplay(true);
                view.unlinkBtn.SetDisplay(false);
            }
            else
            {
                view.folderObject.value = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folderPath);
                view.folderObject.SetEnabled(false);
                view.folderDropdown.SetDisplay(false);
                view.folderObject.SetDisplay(true);
                view.linkBtn.SetDisplay(false);
                view.unlinkBtn.SetDisplay(true);
            }
            
            // refresh pipeline step
            view.pipeline.SetDisplay(selected);
            if (selected)
            {
                foreach (var step in steps)
                {
                    step.Refresh();
                }
            }
        }

        public void Refresh(bool selected, int index, RemoteMiniState miniState)
        {
            this.miniState = miniState;
            this.index = index;
            this.selected = selected;
            LocalRefresh();
        }

    }
}
