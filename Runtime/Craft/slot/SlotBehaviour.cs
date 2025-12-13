using Nianxie.Components;
using XLua;

namespace Nianxie.Craft
{
    public class SlotBehaviour:LuaBehaviour, ISlot
    {
        public object ReadData()
        {
            throw new System.NotImplementedException();
        }

        public AbstractSlotJson PackToJson(AbstractPackContext packContext)
        {
			var behavJson = new SlotBehavJson();
			var reflectEnv = gameManager.reflectEnv;
			var reflectCls = reflectEnv.GetWarmedReflect(classPath, nestedKeys);
			foreach (var injection in reflectCls.nodeInjections)
			{
				var injectObj = injection.ToNodeObject(this, injection.nodePath);
				if (injectObj is AbstractSlotCom slotCom)
				{
					behavJson.slotDict[injection.key] = slotCom.PackToJson(packContext);
				} else 
				{
					// do nothing
				}
			}
			return behavJson;
        }

        public void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson)
        {
			var slotBehavJson = (SlotBehavJson) slotJson;
			var reflectEnv = gameManager.reflectEnv;
	        var reflectCls = reflectEnv.GetWarmedReflect(classPath, nestedKeys);
            foreach (var injection in reflectCls.nodeInjections)
            {
	            var injectObj = injection.ToNodeObject(this, injection.nodePath);
	            var childJson = slotBehavJson.slotDict[injection.key];
				if (injectObj is AbstractSlotCom slotCom)
				{
					slotCom.UnpackFromJson(unpackContext, childJson);
				} else 
				{
					// do nothing
				}
            }
        }
    }
}