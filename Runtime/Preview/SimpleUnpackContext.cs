using System;
using Nianxie.Utils;
using UnityEngine;
using WebP;
using Nianxie.Craft;
using Nianxie.Riff;

namespace Nianxie.Preview
{
    public class SimpleUnpackContext:AbstractUnpackContext
    {
        protected override RiffPackage package { get; }
        private CraftJson craftJson;
        public SimpleUnpackContext(RiffPackage riffPackage)
        {
            package = riffPackage;
            craftJson = (riffPackage.customJson as CraftJson)!;
        }
        
        public void UnpackRoot(SlotBehaviour rootBehav)
        {
            rootBehav.UnpackFromJson(this, craftJson.root);
        }

    }
}