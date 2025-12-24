using System.Collections;
using System.Collections.Generic;
using Nianxie.Craft;
using UnityEngine;

namespace Nianxie.Preview
{
    public class PreviewEditView : MonoBehaviour
    {
        public PreviewEditGizmos gizmos;
        private CraftEdit craftEdit;
        public void Main(CraftEdit craftEdit)
        {
            this.craftEdit = craftEdit;
            gizmos.Main(craftEdit);
        }
    }
}
