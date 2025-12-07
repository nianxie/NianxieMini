using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

namespace Nianxie.Editor
{
    public class AccountMiniPagination
    {
        public int pageNum { get; private set; } = 0;
        private int loadingPageNum = -1;
        public bool loading => loadingPageNum > 0;
        public DB_Mini[] miniItems { get; private set; } = {};
        public Dictionary<string, Texture2D> texDict = new();

        public void NavPage(int targetPageNum, Action callback)
        {
            if (!AccountController.signed)
            {
                return;
            }

            if (loadingPageNum > 0)
            {
                return;
            }
            loadingPageNum = targetPageNum;
            if (loadingPageNum < 1)
            {
                loadingPageNum = 1;
            }
            UniTask.Create(async () =>
            {
                try
                {
                    var arr = await AccountController.List(loadingPageNum);
                    if (arr.Length > 0 || loadingPageNum == 1)
                    {
                        pageNum = loadingPageNum;
                        miniItems = arr;
                    }
                }
                finally
                {
                    loadingPageNum = -1;
                    callback();
                }
            }).Forget();
        }
    }
}
