using System;
using UnityEngine;
using XLua;

namespace Nianxie.Framework
{
    public class MiniHelper : AsyncHelper
    {
        private MiniGameManager miniGameManager => (MiniGameManager) gameManager;
        public void PlayEnding()
        {
            miniGameManager.PlayEnding();
        }

        public LuaTable GetCraftTable()
        {
            return miniGameManager.craftTable;
        }
    }
}
