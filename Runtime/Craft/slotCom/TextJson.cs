using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nianxie.Craft
{
    public class TextJson:AbstractSlotJson<string>
    {
        public string text;
        public override string Export(UnpackContext unpackContext)
        {
            return text;
        }
    }
}
