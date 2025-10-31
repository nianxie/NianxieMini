using System.Collections;
using System.Collections.Generic;
using Nianxie.Components;
using Nianxie.Craft;
using UnityEngine;

namespace Nianxie.Craft
{
    [ExecuteAlways]
	public class EntitySlot : AbstractNodeSlot
	{
		[SerializeField]
		private AbstractAssetSlot[] assetSlotList;
		[SerializeField]
		private ListSlot listSlot;
		public override AbstractSlotJson PackToJson(AbstractPackContext context)
		{
			var entityJson = new EntityJson();
			return entityJson;
		}

		public override void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson)
		{
			var entityJson = (EntityJson) slotJson;
		}

		public override object ReadData()
		{
			return null;
		}
#if UNITY_EDITOR
        public override void ON_INSPECTOR_UPDATE(bool change)
        {
        }
#endif
	}
}
