using System.Collections;
using System.Collections.Generic;
using Nianxie.Components;
using Nianxie.Craft;
using UnityEngine;

namespace Nianxie.Craft
{
	[RequireComponent(typeof(MiniBehaviour))]
	public class BehavSlot : AbstractSlotCom
	{
		private MiniBehaviour _behav;
		public MiniBehaviour behav {
			get
			{
				if (_behav == null)
				{
					_behav = GetComponent<MiniBehaviour>();
				}
				return _behav;
			}
		}

		public override AbstractSlotJson PackToJson(AbstractPackContext context)
		{
			var behavJson = new BehavJson();
			var reflectEnv = behav.gameManager.reflectEnv;
			var reflectCls = reflectEnv.GetWarmedReflect(behav.classPath, behav.nestedKeys);
			foreach (var injection in reflectCls.nodeInjections)
			{
				var injectObj = injection.ToNodeObject(behav, injection.nodePath);
				if (injectObj is AbstractSlotCom slotCom)
				{
					behavJson.slotDict[injection.key] = slotCom.PackToJson(context);
				} else 
				{
					// do nothing
				}
			}
			return behavJson;
		}

		public override void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson)
		{
			var behavJson = (BehavJson) slotJson;
			var reflectEnv = behav.gameManager.reflectEnv;
	        var reflectCls = reflectEnv.GetWarmedReflect(behav.classPath, behav.nestedKeys);
            foreach (var injection in reflectCls.nodeInjections)
            {
	            var injectObj = injection.ToNodeObject(behav, injection.nodePath);
	            var childJson = behavJson.slotDict[injection.key];
				if (injectObj is AbstractSlotCom slotCom)
				{
					slotCom.UnpackFromJson(unpackContext, childJson);
				} else 
				{
					// do nothing
				}
            }
		}

		public override object ReadData()
		{
			return behav.luaTable;
		}
	}
}
