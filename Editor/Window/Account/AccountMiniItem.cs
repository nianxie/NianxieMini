using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using log4net.Appender;
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
            public VisualElement kindGame;
            public VisualElement kindCraft;
            public Label name;
            public Label miniId;
            public Button selectBtn;
            public Button copyBtn;
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
            public ToolbarPopupSearchField folderField;
            public Button runBtn;
            public Button pingBtn;
        }

        private int index;
        private bool selected;
        private string folder;
        private OutViewHierachy view;

        public class PipelineStepContext
        {
            private Action miniRefresh;
            public DB_Mini dbMini;
            public MiniEditorEnvPaths envPaths;
            public string miniId => dbMini.miniId;

            public PipelineStepContext(Action miniRefresh)
            {
                this.miniRefresh = miniRefresh;
            }

            private void ReplaceFolderMeta(string folderPath, string oldGuid, string newGuid)
            {
                var folderMeta = $"{folderPath}.meta";
                if (oldGuid.Length==32 && Directory.Exists(folderPath))
                {
                    var newMeta = File.ReadAllText(folderMeta).Replace($"guid: {oldGuid}", $"guid: {newGuid}");
                    File.WriteAllText(folderMeta, newMeta);
                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.LogError($"{folderPath} is not a valid project");
                }
            }

            public void LinkFolder(string folder)
            {
                var folderPath = $"{NianxieConst.MiniPrefixPath}/{folder}";
                var conflictPath = AssetDatabase.GUIDToAssetPath(miniId);
                if (!string.IsNullOrEmpty(conflictPath) && conflictPath != folderPath)
                {
                    File.Delete($"{conflictPath}.meta");
                }
                var oldGuid = AssetDatabase.AssetPathToGUID(folderPath);
                if (oldGuid != miniId)
                {
                    ReplaceFolderMeta(folderPath, oldGuid, miniId);
                }
                miniRefresh();
            }
            public void UnlinkFolder()
            {
                var folderPath = AssetDatabase.GUIDToAssetPath(miniId);
                var folder = Path.GetFileName(folderPath);
                if (folderPath == $"{NianxieConst.MiniPrefixPath}/{folder}")
                {
                    ReplaceFolderMeta(folderPath, miniId, Guid.NewGuid().ToString("N"));
                }
                miniRefresh();
            }

            public bool TryLoadLinkedFolder(out string linkedFolder, out UnityEngine.Object folderObject)
            {
                var folderPath = AssetDatabase.GUIDToAssetPath(miniId);
                var folder = Path.GetFileName(folderPath??"");
                if (folderPath == $"{NianxieConst.MiniPrefixPath}/{folder}")
                {
                    linkedFolder = folder;
                    folderObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folderPath);
                    return true;
                }
                else
                {
                    linkedFolder = null;
                    folderObject = null;
                    return false;
                }
            }
        }

        public abstract class AbstractPipelineStep
        {
            public StepViewHierachy view { get; }
            protected PipelineStepContext context;

            protected MiniEditorEnvPaths envPaths => context.envPaths;

            protected abstract bool isOkay();
            
            protected virtual bool folderFieldDisplay()
            {
                return false;
            }

            protected virtual bool runBtnEnable()
            {
                return !isOkay();
            }

            protected virtual bool pingBtnEnable()
            {
                return isOkay();
            }

            protected AbstractPipelineStep(VisualElement stepElement, PipelineStepContext stepContext)
            {
                view = EasyHierarchy.CreateByQuery<StepViewHierachy>(stepElement);
                context = stepContext;
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

                if (folderFieldDisplay())
                {
                    view.folderField.SetDisplay(true);
                }
                else
                {
                    view.folderField.SetDisplay(false);
                }

                view.runBtn.SetEnabled(runBtnEnable());
                view.pingBtn.SetEnabled(pingBtnEnable());
            }
        }

        public class LinkProjectStep : AbstractPipelineStep
        {
            private SortedSet<string> folderSet = new ();

            private void updateFolderSet()
            {
                folderSet.Clear();
                foreach(var folder in BuildMiniWindow.ListProjectFolders())
                {
                    folderSet.Add(folder);
                }
            }

            public LinkProjectStep(OutViewHierachy pipelineView, PipelineStepContext stepContext):
                base(pipelineView.linkProject, stepContext)
            {
                updateFolderSet();
                var folderText = view.folderField.Q<TextField>();
                var firstClickAfterFocusIn = true;
                folderText.RegisterCallback((FocusInEvent e) =>
                {
                    firstClickAfterFocusIn = true;
                });
                folderText.RegisterCallback((ClickEvent e) =>
                {
                    if (firstClickAfterFocusIn)
                    {
                        firstClickAfterFocusIn = false;
                        view.folderField.ShowMenu();
                    }
                });
                folderText.RegisterValueChangedCallback((e) =>
                {
                    Refresh();
                });
                foreach (var folder in folderSet)
                {
                    view.folderField.menu.AppendAction(folder, (e) =>
                    {
                        view.folderField.SetValueWithoutNotify(folder);
                        Refresh();
                    }, (e) =>
                    {
                        return e.name == view.folderField.value
                            ? DropdownMenuAction.Status.Checked
                            : DropdownMenuAction.Status.Normal;
                    });
                }
                view.runBtn.clicked+=()=>
                {
                    Debug.LogError("create TODO");
                };
                view.pingBtn.clicked+=()=>
                {
                    stepContext.LinkFolder(view.folderField.value);
                };
            }
            protected override bool isOkay()
            {
                return envPaths != null && Directory.Exists(envPaths.pathPrefix);
            }
            protected override bool folderFieldDisplay()
            {
                return envPaths == null;
            }
            protected override bool runBtnEnable()
            {
                return envPaths == null && !folderSet.Contains(view.folderField.value);
            }
            protected override bool pingBtnEnable()
            {
                return envPaths == null && folderSet.Contains(view.folderField.value);
            }
        }
        public class BuildBundleStep: AbstractPipelineStep
        {
            public BuildBundleStep(OutViewHierachy pipelineView, PipelineStepContext stepContext):
                base(pipelineView.buildBundle, stepContext)
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
            public UploadBundleStep(OutViewHierachy pipelineView, PipelineStepContext stepContext):
                base(pipelineView.uploadBundle, stepContext)
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
                    Debug.Log($"资源地址：{context.dbMini.manifestUrl} {context.dbMini.iosUrl} {context.dbMini.androidUrl}");
                };
            }
            protected override bool isOkay()
            {
                var readyStatus = context.dbMini.readyStatus;
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
            public BuildPackageStep(OutViewHierachy pipelineView, PipelineStepContext stepContext):
                base(pipelineView.buildPackage, stepContext)
            {
            }
            protected override bool isOkay()
            {
                return false;
            }
        }
        public class UploadPackageStep: AbstractPipelineStep
        {
            public UploadPackageStep(OutViewHierachy pipelineView, PipelineStepContext stepContext):
                base(pipelineView.uploadPackage, stepContext)
            {
            }
            protected override bool isOkay()
            {
                return false;
            }
        }
        public class DistributeStep: AbstractPipelineStep
        {
            public DistributeStep(OutViewHierachy pipelineView, PipelineStepContext stepContext):
                base(pipelineView.distribute, stepContext)
            {
            }
            protected override bool isOkay()
            {
                return context.dbMini.readyStatus==DB_Mini.STATUS_VIDEO_USED;
            }
            public override void Refresh()
            {
                base.Refresh();
                view.runBtn.SetDisplay(false);
                view.pingBtn.SetDisplay(false);
            }
        }

        private AbstractPipelineStep[] steps;
        private PipelineStepContext stepContext;

        public void Setup(Action<int> onClick, Action pageRefresh)
        {
            view = EasyHierarchy.CreateByQuery<OutViewHierachy>(self);
            stepContext = new(LocalRefresh);
            steps = new AbstractPipelineStep[STEP_NUM]
            {
                new LinkProjectStep(view, stepContext),
                new BuildBundleStep(view, stepContext),
                new UploadBundleStep(view, stepContext),
                new BuildPackageStep(view, stepContext),
                new UploadPackageStep(view, stepContext),
                new DistributeStep(view, stepContext),
            };
            view.folderObject.SetEnabled(false);
            view.unlinkBtn.clicked += () =>
            {
                stepContext.UnlinkFolder();
            };
            view.deleteBtn.clicked += () =>
            {
                UniTask.Create(async () =>
                {
                    if (EditorUtility.DisplayDialog("删除游戏？", $"确认删除{stepContext.dbMini.name}吗?", "确认", "取消"))
                    {
                        await AccountController.DeleteMini(stepContext.miniId);
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
                GUIUtility.systemCopyBuffer = stepContext.miniId;
                Debug.Log($"{stepContext.dbMini.name}的ID：{stepContext.dbMini.miniId}已复制");
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
            view.name.text = stepContext.dbMini.name;
            view.radio.SetValueWithoutNotify(selected);
            view.deleteBtn.SetDisplay(selected);
            view.copyBtn.SetDisplay(selected);
            view.miniId.text = stepContext.dbMini.miniId;
            view.kindCraft.SetDisplay(stepContext.dbMini.craft);
            view.kindGame.SetDisplay(!stepContext.dbMini.craft);

            if(!stepContext.TryLoadLinkedFolder(out var linkedFolder, out var linkedFolderObject))
            {
                stepContext.envPaths = null;
                view.folderObject.value = null;
                view.unlinkBtn.SetDisplay(false);
            }
            else
            {
                stepContext.envPaths = MiniEditorEnvPaths.Get(linkedFolder);
                view.folderObject.value = linkedFolderObject;
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

        public void Refresh(bool selected, int index, DB_Mini dbMini)
        {
            stepContext.dbMini = dbMini;
            this.index = index;
            this.selected = selected;
            LocalRefresh();
        }

    }
}
