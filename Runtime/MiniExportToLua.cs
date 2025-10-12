using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nianxie.Craft;
using UnityEngine;
using UnityEngine.InputSystem;

namespace XLua
{
    public static class MiniExportToLua
    {
        [LuaCallCSharp] static List<Type> cfg = new List<Type>()
        {
            typeof(System.Web.HttpUtility),
            // framework items
            typeof(Nianxie.Utils.PlatformUtility),
            typeof(Nianxie.Utils.NianxieUnityExtension),
            typeof(Nianxie.Utils.LargeBytes),
            typeof(Nianxie.Framework.LuafabLoading),
            typeof(Nianxie.Framework.MiniHelper),
            typeof(Nianxie.Framework.AsyncHelper),
            typeof(Nianxie.Components.LuaBehaviour),
            typeof(Nianxie.Components.MiniBehaviour),
            typeof(Nianxie.Craft.SlotBehaviour),
            typeof(Nianxie.Craft.AbstractSlotCom),
            typeof(Nianxie.Craft.PositionSlot),
            typeof(Nianxie.Craft.CraftJson),
            // mini
            typeof(Nianxie.Framework.MiniGameManager),
            typeof(Nianxie.Framework.MiniPlayArgs),
            typeof(Nianxie.Craft.CraftModule),
            
            // unity items
            typeof(UnityEngine.LayerMask),
            typeof(UnityEngine.MonoBehaviour),
            typeof(UnityEngine.PolygonCollider2D),
            typeof(UnityEngine.BoxCollider2D),
            typeof(UnityEngine.Collider2D),
            typeof(UnityEngine.Physics2D),
            typeof(UnityEngine.Behaviour),
            typeof(UnityEngine.Sprite),
            typeof(UnityEngine.Texture),
            typeof(UnityEngine.Texture2D),
            typeof(UnityEngine.TextAsset),
            typeof(UnityEngine.GameObject),
            typeof(UnityEngine.Object),
            typeof(UnityEngine.Application),
            typeof(UnityEngine.RectTransform),
            typeof(UnityEngine.Transform),
            typeof(UnityEngine.Component),
            typeof(UnityEngine.Time),
            typeof(UnityEngine.Vector3),
            typeof(UnityEngine.Vector2),
            typeof(UnityEngine.Color),
            typeof(UnityEngine.Rect),
            typeof(UnityEngine.Canvas),
            typeof(UnityEngine.Screen),
            typeof(UnityEngine.CanvasGroup),
            typeof(UnityEngine.Camera),
            typeof(UnityEngine.SpriteRenderer),
            typeof(UnityEngine.Renderer),
            typeof(UnityEngine.RectTransformUtility),
            typeof(UnityEngine.AudioSource),
            typeof(UnityEngine.Mathf),
            typeof(UnityEngine.InputSystem.InputActionAsset),
            typeof(UnityEngine.InputSystem.InputAction),
            typeof(UnityEngine.InputSystem.InputAction.CallbackContext),
            
            // event trigger
            typeof(UnityEngine.EventSystems.EventTriggerType),
            typeof(UnityEngine.EventSystems.PointerEventData),
            typeof(UnityEngine.EventSystems.BaseEventData),

            // ui
            typeof(UnityEngine.UI.ScrollRect),
            typeof(UnityEngine.UI.Image),
            typeof(UnityEngine.UI.RawImage),
            typeof(UnityEngine.UI.Button),
            typeof(UnityEngine.UI.Selectable),
            typeof(UnityEngine.UI.Slider),
            typeof(UnityEngine.UI.Toggle),
            typeof(UnityEngine.UI.Graphic),
            typeof(UnityEngine.UI.MaskableGraphic),
            typeof(UnityEngine.UI.Dropdown.OptionData),
            typeof(UnityEngine.EventSystems.UIBehaviour),
            typeof(TMPro.TextMeshProUGUI),
            typeof(TMPro.TMP_Text),
            typeof(TMPro.TMP_InputField),
            typeof(TMPro.TMP_Dropdown),

            // DOTWeen
            typeof(DG.Tweening.Tween),
            typeof(DG.Tweening.Ease),
            typeof(DG.Tweening.Sequence),
            typeof(DG.Tweening.DOTween),
            typeof(DG.Tweening.TweenSettingsExtensions),
            typeof(DG.Tweening.ShortcutExtensions),
            typeof(DG.Tweening.DOTweenModuleUI),
            typeof(DG.Tweening.DOTweenModuleSprite),
            typeof(DG.Tweening.DOTweenModulePhysics),
            typeof(DG.Tweening.DOTweenModulePhysics2D),
            typeof(DG.Tweening.DOTweenModuleUnityVersion),
            typeof(DG.Tweening.DOTweenModifyTextMeshPro),
            // TODO 考虑EPOOueline是否需要引入？
            // typeof(DG.Tweening.DOTweenModuleEPOOutline),
            
            
            // TODO VideoPlayer似乎可以播放任意外部视频，需要考虑做限制。
            typeof(UnityEngine.Video.VideoPlayer),
        };

        [CSharpCallLua] static List<Type> csharp_call_lua_cfg = new List<Type>()
        {
            typeof(Action<float>),
            typeof(Action<InputAction.CallbackContext>),
            typeof(Action<UnityEngine.Video.VideoPlayer>),
            typeof(UnityEngine.Events.UnityAction<int>),
            typeof(UnityEngine.Events.UnityAction<string,int>),
            typeof(UnityEngine.Events.UnityAction<string>),
            typeof(UnityEngine.Events.UnityAction<bool>),
            typeof(UnityEngine.Events.UnityAction<Color>),
            typeof(UnityEngine.Events.UnityAction<Vector2>),
            typeof(Action<string, string, int>),
            typeof(Func<int, int>),
            typeof(Action<string, string, int, int>),
        };

        [BlackList] public static List<List<string>> BlackList = new List<List<string>>()
        {
            // ignore for hint
            new List<string>() {"UnityEngine.Texture", "imageContentsHash"},
            new List<string>() {"TMPro.TextMeshProUGUI", "ClearMesh"},
            new List<string>() {"TMPro.TMP_Text", "CrossFadeColor", "UnityEngine.Color", "System.Single", "System.Boolean", "System.Boolean"},
            
            // copy from https://github.com/Tencent/xLua/blob/master/Assets/XLua/Examples/ExampleGenConfig.cs
            new List<string>(){"System.Xml.XmlNodeList", "ItemOf"},
            new List<string>(){"UnityEngine.WWW", "movie"},
#if UNITY_WEBGL
            new List<string>(){"UnityEngine.WWW", "threadPriority"},
#endif
            new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
            new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
            new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
            new List<string>(){"UnityEngine.Light", "areaSize"},
            new List<string>(){"UnityEngine.Light", "lightmapBakeType"},
            new List<string>(){"UnityEngine.UI.Graphic", "OnRebuildRequested"},
#if UNITY_ANDROID
            new List<string>(){"UnityEngine.Light", "SetLightDirty"},
            new List<string>(){"UnityEngine.Light", "shadowRadius"},
            new List<string>(){"UnityEngine.Light", "shadowAngle"},
#endif
            new List<string>(){"UnityEngine.WWW", "MovieTexture"},
            new List<string>(){"UnityEngine.WWW", "GetMovieTexture"},
            new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
#if !UNITY_WEBPLAYER
            new List<string>(){"UnityEngine.Application", "ExternalEval"},
#endif
            new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
            new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
            new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
            new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
            new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
            new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
            new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
            new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
            new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
            new List<string>(){"UnityEngine.AudioSource", "gamepadSpeakerOutputType"},
            new List<string>(){"UnityEngine.AudioSource", "PlayOnGamepad", "System.Int32"},
            new List<string>(){"UnityEngine.AudioSource", "DisableGamepadOutput"},
            new List<string>(){"UnityEngine.AudioSource", "SetGamepadSpeakerMixLevel", "System.Int32", "System.Int32"},
            new List<string>(){"UnityEngine.AudioSource", "SetGamepadSpeakerMixLevelDefault", "System.Int32"},
            new List<string>(){"UnityEngine.AudioSource", "SetGamepadSpeakerRestrictedAudio", "System.Int32", "System.Boolean"},
            new List<string>(){"UnityEngine.AudioSource", "GamepadSpeakerSupportsOutputType", "UnityEngine.GamepadSpeakerOutputType"}
        };
        
        // codes copy from https://github.com/Tencent/xLua/issues/1056
        public static List<Type> BlackGenericTypeList = new List<Type>()
        {
            typeof(Span<>),
            typeof(ReadOnlySpan<>),
        };

        private static bool IsBlacklistedGenericType(Type type)
        {
            if (!type.IsGenericType) return false;
            return BlackGenericTypeList.Contains(type.GetGenericTypeDefinition());
        }

        [BlackList] public static Func<MemberInfo, bool> GenericTypeFilter = (memberInfo) =>
        {
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    return IsBlacklistedGenericType(propertyInfo.PropertyType);

                case ConstructorInfo constructorInfo:
                    return constructorInfo.GetParameters().Any(p => IsBlacklistedGenericType(p.ParameterType));

                case MethodInfo methodInfo:
                    return methodInfo.GetParameters().Any(p => IsBlacklistedGenericType(p.ParameterType));

                default:
                    return false;
            }
        };
    }
}

