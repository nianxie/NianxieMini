using System;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Nianxie.Framework;
using Nianxie.Utils;
using UnityEditor;
using UnityEditor.EditorTools;
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
            public Button deleteBtn;
            public VisualElement pipeline;
            public VisualElement createProject;
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
        private OutViewHierachy view;
        
        public abstract class AbstractPipelineStep
        {
            public StepViewHierachy view { get; }
            private Func<RemoteMiniState> stateGetter;
            protected RemoteMiniState miniState => stateGetter();
            protected MiniEditorEnvPaths envPaths => stateGetter().envPaths;

            protected abstract bool isOkay();

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

        public class CreateProjectStep : AbstractPipelineStep
        {
            public CreateProjectStep(OutViewHierachy pipelineView, Func<RemoteMiniState> stateGetter):
                base(pipelineView.createProject, stateGetter)
            {
                view.runBtn.clicked += () =>
                {
                    var srcPath = NianxieConst.TemplateSimpleGame;
                    var dstPath = envPaths.pathPrefix;
                    if (!Directory.Exists(NianxieConst.MiniPrefixPath))
                    {
                        Directory.CreateDirectory(NianxieConst.MiniPrefixPath);
                    }

                    if (AssetDatabase.CopyAsset(srcPath, dstPath))
                    {
                        envPaths.UpdateProjectConfig(miniState.dbMini);
                    }
                    else
                    {
                        Debug.LogError($"project create error: copy maybe fail {srcPath} -> {dstPath}");
                    }
                    Refresh();
                    //Debug.Log($"{Paths.Template2D} -> {miniState.envPaths.pathPrefix}");
                };
                view.pingBtn.clicked += () =>
                {
                    var dir = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(envPaths.pathPrefix);
                    EditorGUIUtility.PingObject(dir);
                };
            }
            protected override bool isOkay()
            {
                return Directory.Exists(envPaths.pathPrefix);
            }
        }
        public class BuildBundleStep: AbstractPipelineStep
        {
            public BuildBundleStep(OutViewHierachy pipelineView, Func<RemoteMiniState> stateGetter):
                base(pipelineView.buildBundle, stateGetter)
            {
                view.runBtn.clicked += () =>
                {
                    MiniBuildWindow.OpenBuildWindow();
                };
                view.pingBtn.clicked += () =>
                {
                    EditorUtility.RevealInFinder(envPaths.finalManifest);
                };
            }
            protected override bool isOkay()
            {
                return File.Exists(envPaths.finalManifest) && envPaths.finalBundleDict.Values.All(path => File.Exists(path));
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
                return File.Exists(envPaths.finalManifest) && envPaths.finalBundleDict.Values.All(path => File.Exists(path));
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
                new CreateProjectStep(view, stateGetter),
                new BuildBundleStep(view, stateGetter),
                new UploadBundleStep(view, stateGetter),
                new BuildPackageStep(view, stateGetter),
                new UploadPackageStep(view, stateGetter),
                new DistributeStep(view, stateGetter),
            };
            view.deleteBtn.clicked += () =>
            {
                UniTask.Create(async () =>
                {
                    await AccountController.DeleteMini(miniState.miniId);
                    pageRefresh();
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

        public void Refresh(bool selected, int index, RemoteMiniState miniState)
        {
            this.miniState = miniState;
            this.index = index;
            view.name.text = miniState.dbMini.name;
            view.radio.SetValueWithoutNotify(selected);
            view.deleteBtn.SetDisplay(selected);
            view.miniId.text = miniState.dbMini.miniId;
            view.pipeline.SetDisplay(selected);
            if (selected)
            {
                foreach (var step in steps)
                {
                    step.Refresh();
                }
            }
        }

    }
}
