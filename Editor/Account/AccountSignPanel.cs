using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using ZXing;
using ZXing.QrCode;

namespace Nianxie.Editor
{
    public class AccountSignPage: EasyViewModel
    {
        enum LoginKind
        {
            Account = 1,
            QRCode = 2,
        }

        private static Texture2D qrCodeTexture;
        private LoginKind loginKind = LoginKind.Account;

        class View: EasyHierarchy
        {
            public Button qrCodeBtn;
            public Button accountBtn;
            public TextField accountInput;
            public Button signinBtn;
            public VisualElement accountPanel;
            public VisualElement qrCodePanel;
        }

        private View view;

        public AccountSignPage Setup(Action onSignin)
        {
            view = EasyHierarchy.CreateByQuery<View>(self);
            view.qrCodeBtn.clicked+=()=>
            {
                loginKind = LoginKind.QRCode;
                Refresh();
            };
            view.accountBtn.clicked+=()=>
            {
                loginKind = LoginKind.Account;
                Refresh();
            };
            view.accountInput.RegisterValueChangedCallback((e) =>
            {
                if (e.newValue.EndsWith("\n"))
                {
                }
                if (!Regex.IsMatch(e.newValue, @"^[a-zA-Z0-9_]*$"))
                {
                    view.accountInput.SetValueWithoutNotify(e.previousValue);
                }
            });
            view.signinBtn.clicked+=()=>
            {
                if (!AccountController.signinRunning)
                {
                    UniTask.Create(async () =>
                    {
                        await AccountController.Signin(view.accountInput.value);
                        onSignin();
                        Refresh();
                    });
                }
            };
            return this;
        }

        public void Refresh()
        {
            view.accountPanel.SetDisplay(loginKind == LoginKind.Account);
            view.accountBtn.SetDisplay(loginKind == LoginKind.Account);
            view.qrCodePanel.SetDisplay(loginKind == LoginKind.QRCode);
            view.qrCodeBtn.SetDisplay(loginKind == LoginKind.QRCode);
        }
        
        public void updateTexture()
        {
            if (qrCodeTexture != null)
            {
                return;
            }

            var writer = new BarcodeWriter()
            {
                Format=BarcodeFormat.QR_CODE,
                Options=new QrCodeEncodingOptions()
                {
                    Height=256,
                    Width=256,
                }
            };
            Color32[] dosth = writer.Write("二维码功能尚未开发，敬请期待");
            qrCodeTexture = new Texture2D(256, 256, TextureFormat.RGBA32, false, false);
            qrCodeTexture.SetPixels32(dosth);
            qrCodeTexture.Apply();
        }
    }
}
