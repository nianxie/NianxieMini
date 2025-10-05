using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nianxie.Components;

namespace Nianxie.Craft
{
	public class CraftPackContext
	{
		private Dictionary<string, AbstractSlotJson> json = new();
		private RenderTexture renderTexture;

		public CraftPackContext(int width, int height)
		{
			renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
			renderTexture.name = "CraftPack";
			renderTexture.autoGenerateMips = false;
			renderTexture.useMipMap = false;
			renderTexture.DiscardContents();
		}

		public void AddTexture(Texture texture)
		{
			Graphics.Blit(texture, renderTexture);
		}

		public void AddJsonField(string key, AbstractSlotJson value)
		{
			json[key] = value;
		}
	}
	
	public class SlotBehaviour : LuaBehaviour
    {
        public void FromJson(Dictionary<string, AbstractSlotJson> json)
        {
	        AssetBundle ab;
        }

        public SlotScriptJson PackToJson(AbstractPackContext context)
        {
	        var slotScriptJson = new SlotScriptJson();
	        var reflectEnv = gameManager.reflectEnv;
	        var reflectCls = reflectEnv.GetWarmedReflect(classPath, nestedKeys);
            foreach (var injection in reflectCls.nodeInjections)
            {
	            var injectObj = injection.ToNodeObject(this, injection.nodePath);
				if (injectObj is AbstractSlotCom slotCom)
				{
					slotScriptJson.slotDict[injection.key] = slotCom.PackToJson(context);
				} else if (injectObj is SlotBehaviour slotBehav)
				{
					slotScriptJson.slotDict[injection.key] = slotBehav.PackToJson(context);
				}else
				{
					Debug.LogError($"{injection.key} not existed in {gameObject.name}");
				}
            }
            return slotScriptJson;
        }
        public void UnpackFromJson(CraftUnpackContext context, SlotScriptJson slotScriptJson)
        {
	        var reflectEnv = gameManager.reflectEnv;
	        var reflectCls = reflectEnv.GetWarmedReflect(classPath, nestedKeys);
            foreach (var injection in reflectCls.nodeInjections)
            {
	            var injectObj = injection.ToNodeObject(this, injection.nodePath);
	            var childJson = slotScriptJson.slotDict[injection.key];
				if (injectObj is AbstractSlotCom slotCom)
				{
					slotCom.UnpackFromJson(context, childJson);
				} else if (injectObj is SlotBehaviour slotBehav)
				{
					slotBehav.UnpackFromJson(context, (SlotScriptJson)childJson);
				}else
				{
					Debug.LogError($"{injection.key} not existed in {gameObject.name}");
				}
            }
        }
#if UNITY_EDITOR
	    public Dictionary<string, Texture2D> editorTexDict = new();
#endif
    }
}
