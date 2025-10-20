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
            public Button unlinkBtn;
            public ObjectField folderObject;
            public Button deleteBtn;
            public VisualElement pipeline;
            public VisualElement initProject;
            public VisualElement syncConfig;
            public VisualElement buildBundle;
            public VisualElement uploadBundle;
            public VisualElement buildPackage;
            public VisualElement uploadPackage;
            public VisualElement distribute;
        }
        
        public class StepViewHierachy: EasyHierarchy
        {
            public Label okay;
            public Label titleIndex;
            public Label titleDot;
            public Label titleInfo;
            public Button btn1;
            public Button btn2;
            // element for init step
            public ToolbarPopupSearchField folderField;
        }

        private bool selected;
        private string folder;
        private OutViewHierachy view;

        public class PipelineStepContext
        {
            public readonly Action miniRefresh;
            public DB_Mini dbMini => AccountController.dbMiniDatas[miniIndex];
            public int miniIndex;
            public MiniEditorEnvPaths envPaths;
            public string miniId => dbMini.miniId;

            public PipelineStepContext(Action miniRefresh)
            {
                this.miniRefresh = miniRefresh;
            }

            public void LinkFolder(string folder)
            {
                AccountController.LinkFolder(dbMini, folder);
                miniRefresh();
            }

            public bool TryLoadLinkedFolder(out string linkedFolder, out UnityEngine.Object folderObject)
            {
                if (AccountController.TryMapLinkedFolder(dbMini, out var folderPath, out linkedFolder))
                {
                    folderObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folderPath);
                    return true;
                }
                else
                {
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
            
            protected virtual bool btn1Enable()
            {
                return !isOkay();
            }

            protected virtual bool btn2Enable()
            {
                return isOkay();
            }

            protected AbstractPipelineStep(VisualElement stepElement, PipelineStepContext stepContext, int stepIndex)
            {
                view = EasyHierarchy.CreateByQuery<StepViewHierachy>(stepElement);
                context = stepContext;
                view.titleIndex.text = stepIndex.ToString();
            }

            public virtual void Refresh()
            {
                view.okay.text = isOkay() ? "✓" : "?";
                var fontColor = new StyleColor()
                {
                    value=isOkay()?Color.green:Color.yellow,
                };
                view.okay.style.color = fontColor;
                view.titleIndex.style.color = fontColor;
                view.titleDot.style.color = fontColor;
                view.titleInfo.style.color = fontColor;
                if (this is InitProjectStep)
                {
                    view.folderField.SetDisplay(true);
                }
                else
                {
                    view.folderField.SetDisplay(false);
                }

                view.btn1.SetEnabled(btn1Enable());
                view.btn2.SetEnabled(btn2Enable());
            }
        }

        public class InitProjectStep : AbstractPipelineStep
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

            public InitProjectStep(OutViewHierachy pipelineView, PipelineStepContext stepContext, int stepIndex):
                base(pipelineView.initProject, stepContext, stepIndex)
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
                view.btn1.clicked+= () => {
                    var folder = view.folderField.value;
                    if (string.IsNullOrEmpty(folder))
                    {
                        Debug.LogError("路径为空");
                        return;
                    }
                    if (BuildMiniWindow.CopyTemplateAsProject(context.dbMini, folder))
                    {
                        view.folderField.SetValueWithoutNotify("");
                        AccountController.LinkFolder(context.dbMini, folder);
                        UniTask.Create(async () =>
                        {
                            await AccountController.SyncConfigs(context.dbMini);
                            context.miniRefresh();
                        }).Forget();
                    }
                };
                view.btn2.clicked+= () => { 
                    AccountController.LinkFolder(context.dbMini, view.folderField.value);
                    UniTask.Create(async () =>
                    {
                        await AccountController.SyncConfigs(context.dbMini);
                        context.miniRefresh();
                    }).Forget();
                };
            }
            protected override bool isOkay()
            {
                return envPaths != null && Directory.Exists(envPaths.pathPrefix);
            }
            protected override bool btn1Enable()
            {
                return envPaths == null && !folderSet.Contains(view.folderField.value);
            }
            protected override bool btn2Enable()
            {
                return envPaths == null && folderSet.Contains(view.folderField.value);
            }

            public override void Refresh()
            {
                base.Refresh();
                view.folderField.SetEnabled(envPaths==null);
            }
        }

        public class BuildBundleStep: AbstractPipelineStep
        {
            public BuildBundleStep(OutViewHierachy pipelineView, PipelineStepContext stepContext, int stepIndex):
                base(pipelineView.buildBundle, stepContext, stepIndex)
            {
                view.btn1.clicked += () =>
                {
                    BuildMiniWindow.OpenBuildWindow();
                };
                view.btn2.clicked += () =>
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
            public UploadBundleStep(OutViewHierachy pipelineView, PipelineStepContext stepContext, int stepIndex):
                base(pipelineView.uploadBundle, stepContext, stepIndex)
            {
                view.btn1.clicked += () =>
                {
                    UniTask.Create(async () =>
                    {
                        try
                        {
                            if (envPaths.config.IsError())
                            {
                                throw new Exception($"config error in {envPaths.miniProjectConfig}");
                            }
                            await AccountController.UploadBundle(stepContext.miniId, envPaths, (name, progress, total) =>
                            {
                                EditorUtility.DisplayProgressBar("上传文件", $"{progress}/{total} {name}", (progress*1.0f)/total);
                            });
                        } finally {
                            EditorUtility.ClearProgressBar();
                        }
                        context.miniRefresh();
                    }).Forget();
                };
                view.btn2.clicked += () =>
                {
                    Debug.Log($"资源地址：{context.dbMini.manifestUrl} {context.dbMini.iosUrl} {context.dbMini.androidUrl}");
                };
            }
            protected override bool isOkay()
            {
                var readyStatus = context.dbMini.readyStatus;
                return readyStatus==DB_Mini.STATUS_UPLOADED || readyStatus==DB_Mini.STATUS_VIDEO_USED;
            }

            protected override bool btn1Enable()
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
            public BuildPackageStep(OutViewHierachy pipelineView, PipelineStepContext stepContext, int stepIndex):
                base(pipelineView.buildPackage, stepContext, stepIndex)
            {
            }
            protected override bool isOkay()
            {
                return false;
            }
        }
        public class UploadPackageStep: AbstractPipelineStep
        {
            public UploadPackageStep(OutViewHierachy pipelineView, PipelineStepContext stepContext, int stepIndex):
                base(pipelineView.uploadPackage, stepContext, stepIndex)
            {
            }
            protected override bool isOkay()
            {
                return false;
            }
        }
        public class DistributeStep: AbstractPipelineStep
        {
            public DistributeStep(OutViewHierachy pipelineView, PipelineStepContext stepContext, int stepIndex):
                base(pipelineView.distribute, stepContext, stepIndex)
            {
            }
            protected override bool isOkay()
            {
                return context.dbMini.readyStatus==DB_Mini.STATUS_VIDEO_USED;
            }
            public override void Refresh()
            {
                base.Refresh();
                view.btn1.SetDisplay(false);
                view.btn2.SetDisplay(false);
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
                new InitProjectStep(view, stepContext, 1),
                new BuildBundleStep(view, stepContext, 2),
                new UploadBundleStep(view, stepContext, 3),
                new DistributeStep(view, stepContext, 4),
                new BuildPackageStep(view, stepContext, 5),
                new UploadPackageStep(view, stepContext, 6),
            };
            view.folderObject.SetEnabled(false);
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
                onClick(stepContext.miniIndex);
            };
            view.unlinkBtn.clicked += () =>
            {
                AccountController.UnlinkFolder(stepContext.dbMini);
                stepContext.miniRefresh();
            };
            /*view.copyBtn.clicked += () =>
            {
                GUIUtility.systemCopyBuffer = stepContext.miniId;
                Debug.Log($"{stepContext.dbMini.name}的ID：{stepContext.dbMini.miniId}已复制");
            };*/
            view.radio.RegisterValueChangedCallback((e) =>
            {
                if (e.newValue)
                {
                    onClick(stepContext.miniIndex);
                }
            });
        }

        private void LocalRefresh()
        {
            view.name.text = stepContext.dbMini.name;
            view.radio.SetValueWithoutNotify(selected);
            view.deleteBtn.SetDisplay(selected);
            view.unlinkBtn.SetDisplay(selected);
            view.miniId.text = stepContext.dbMini.miniId;

            if(!stepContext.TryLoadLinkedFolder(out var linkedFolder, out var linkedFolderObject))
            {
                stepContext.envPaths = null;
                view.folderObject.value = null;
                view.unlinkBtn.SetEnabled(false);
            }
            else
            {
                stepContext.envPaths = MiniEditorEnvPaths.Get(linkedFolder);
                view.folderObject.value = linkedFolderObject;
                view.unlinkBtn.SetEnabled(true);
            }
            view.kindCraft.SetDisplay(stepContext.dbMini.craftable);
            view.kindGame.SetDisplay(!stepContext.dbMini.craftable);
            
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

        public void Refresh(bool selected, int miniIndex)
        {
            stepContext.miniIndex = miniIndex;
            this.selected = selected;
            LocalRefresh();
        }

    }
}
