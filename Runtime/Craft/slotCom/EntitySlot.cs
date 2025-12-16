using System.Collections;
using System.Collections.Generic;
using Nianxie.Components;
using Nianxie.Craft;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    [ExecuteAlways]
	public class EntitySlot : AbstractRenderSlot
	{
		[Tooltip("选项array")]
		[SerializeField]
		private AbstractAssetSlot[] optionSlotArray;
		public override AbstractSlotJson PackToJson(AbstractPackContext context)
		{
			var entityJson = new EntityJson();
			return entityJson;
		}

		public override object slotValue { get; set; }

		public override void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson)
		{
			var entityJson = (EntityJson) slotJson;
		}

#if UNITY_EDITOR
	    [BlackList]
        public override void ON_INSPECTOR_UPDATE(bool change)
        {
        }
#endif
	}
}
