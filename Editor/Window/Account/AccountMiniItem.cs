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
            public Button deleteBtn;
            public VisualElement pipeline;
            public VisualElement initProject;
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
            public Button btn1;
            public Button btn2;
            // element for init step
            public VisualElement initTools;
            public Button initByCreate;
            public Button initByLink;
            public ToolbarPopupSearchField initFolderField;
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

            public void LinkFolder(string folder)
            {
                AccountController.LinkFolder(dbMini, folder);
                miniRefresh();
            }
            public void UnlinkFolder()
            {
                AccountController.UnlinkFolder(dbMini);
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
            
            protected virtual bool btn1Enable()
            {
                return !isOkay();
            }

            protected virtual bool btn2Enable()
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
                if (this is InitProjectStep && envPaths == null)
                {
                    view.initTools.SetDisplay(true);
                }
                else
                {
                    view.initTools.SetDisplay(false);
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

            public InitProjectStep(OutViewHierachy pipelineView, PipelineStepContext stepContext):
                base(pipelineView.initProject, stepContext)
            {
                updateFolderSet();
                var folderText = view.initFolderField.Q<TextField>();
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
                        view.initFolderField.ShowMenu();
                    }
                });
                folderText.RegisterValueChangedCallback((e) =>
                {
                    Refresh();
                });
                foreach (var folder in folderSet)
                {
                    view.initFolderField.menu.AppendAction(folder, (e) =>
                    {
                        view.initFolderField.SetValueWithoutNotify(folder);
                        Refresh();
                    }, (e) =>
                    {
                        return e.name == view.initFolderField.value
                            ? DropdownMenuAction.Status.Checked
                            : DropdownMenuAction.Status.Normal;
                    });
                }
                view.initByCreate.clicked+= () => {
                    var folder = view.initFolderField.value;
                    if (string.IsNullOrEmpty(folder))
                    {
                        Debug.LogError("路径为空");
                        return;
                    }
                    
                    BuildMiniWindow.CopyTemplateAsProject(context.dbMini.name, folder, context.dbMini.craft);
                    context.LinkFolder(folder);
                    Refresh();
                };
                view.initByLink.clicked+= () => { 
                    context.LinkFolder(view.initFolderField.value);
                };
                view.btn1.clicked+=()=>
                {
                    envPaths.FlushWithRemoteInfo(context.dbMini.name, context.dbMini.craft);
                    Refresh();
                };
                view.btn2.clicked+=()=>
                {
                    context.UnlinkFolder();
                };
            }
            protected override bool isOkay()
            {
                return envPaths != null && Directory.Exists(envPaths.pathPrefix);
            }
            protected override bool btn1Enable()
            {
                return true;
            }

            public override void Refresh()
            {
                base.Refresh();
                if (envPaths == null)
                {
                    view.initByCreate.SetEnabled(!folderSet.Contains(view.initFolderField.value));
                    view.initByLink.SetEnabled(folderSet.Contains(view.initFolderField.value));
                    view.btn1.SetDisplay(false);
                    view.btn2.SetDisplay(false);
                }
                else
                {
                    string fixConfigTooltip = "";
                    if (envPaths.config.IsError())
                    {
                        fixConfigTooltip = "本地配置异常，点击修正配置";
                    } else if(!envPaths.config.MatchRemote(context.dbMini.name, context.dbMini.craft))
                    {
                        fixConfigTooltip = $"本地配置与远端不一致，点击修正配置";
                    }
                    view.btn1.tooltip = fixConfigTooltip;
                    view.btn1.SetEnabled(fixConfigTooltip!="");
                    view.btn1.SetDisplay(true);
                    view.btn2.SetDisplay(true);
                }
            }
        }
        public class BuildBundleStep: AbstractPipelineStep
        {
            public BuildBundleStep(OutViewHierachy pipelineView, PipelineStepContext stepContext):
                base(pipelineView.buildBundle, stepContext)
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
            public UploadBundleStep(OutViewHierachy pipelineView, PipelineStepContext stepContext):
                base(pipelineView.uploadBundle, stepContext)
            {
                view.btn1.clicked += () =>
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
                new InitProjectStep(view, stepContext),
                new BuildBundleStep(view, stepContext),
                new UploadBundleStep(view, stepContext),
                new BuildPackageStep(view, stepContext),
                new UploadPackageStep(view, stepContext),
                new DistributeStep(view, stepContext),
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
            }
            else
            {
                stepContext.envPaths = MiniEditorEnvPaths.Get(linkedFolder);
                view.folderObject.value = linkedFolderObject;
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
