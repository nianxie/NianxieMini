using System;
using System.Collections;
using System.Collections.Generic;
using Nianxie.Components;
using Nianxie.Utils;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    public class SlotBehaviour:LuaBehaviour, IUnionSlot
    {
	    public class UnionSlotList:IReadOnlyList<IUnionSlot>
	    {
		    private List<IUnionSlot> list;
		    private SlotInjected slotInjected;
		    public UnionSlotList(SlotInjected slotInjected, List<IUnionSlot> list)
		    {
			    this.list = list;
			    this.slotInjected = slotInjected;
		    }

		    public void DuplicateElement()
		    {
			    var template = list[0];
				var newGo = Instantiate(template.gameObject, template.transform.parent);
				var newSlot = newGo.GetComponent(template.GetType()) as IUnionSlot;
				newSlot.Init(slotInjected);
				newSlot.transform.localPosition += Vector3.right*template.gameObject.GetComponent<AbstractRenderSlot>().selectHead.selectBody.touchCollider2D.size.x*list.Count;
				list.Add(newSlot);
			}

			public void DeleteElement(GameObject go)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].gameObject == go)
					{
						list.RemoveAt(i);
						UnityEngine.Object.Destroy(go);
						return;
					}
				}
				Debug.LogError($"try to remove {go} but it's not in this list");
			}

			public void RawUnpackList(IGetAsset getAsset, AbstractSlotJson[] slotJsonArr)
			{
				for (int i = list.Count; i < slotJsonArr.Length; i++)
				{
					DuplicateElement();
				}
				for (int i = list.Count-1; i >= slotJsonArr.Length; i--)
				{
					DeleteElement(list[i].gameObject);
				}

				for (int i = 0; i < slotJsonArr.Length; i++)
				{
					list[i].UnpackFromJson(getAsset, slotJsonArr[i]);
				}
			}

			public IEnumerator<IUnionSlot> GetEnumerator()
			{
				return list.GetEnumerator();
			}
			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
			public int Count => list.Count;
			public IUnionSlot this[int index] => list[index];
	    }

	    public SlotInjected slotInjected { get; private set; }
	    public ISlotHandler slotHandler { get; private set; }

        private Dictionary<string, IUnionSlot> slotSingleDict = new();
        private Dictionary<string, UnionSlotList> slotListDict = new();

        public void RootInit(CraftManager craftManager)
        {
	        slotHandler = craftManager;
	        (this as IUnionSlot).Init(null);
        }

        void IUnionSlot.Init(SlotInjected injected)
        {
	        if (injected != null)
	        {
				slotInjected = injected;
				slotHandler = injected.behav.slotHandler;
	        }
			var reflectEnv = gameManager.reflectEnv;
	        var reflectCls = reflectEnv.GetWarmedReflect(classPath, nestedKeys);
            foreach (var injection in reflectCls.nodeInjections)
            {
	            if (injection.multipleKind == InjectionMultipleKind.Single)
	            {
					var obj = injection.ToNodeObject(this, injection.nodePath);
					if (obj is IUnionSlot unionSlot)
					{
						unionSlot.Init(new SlotInjected(this, injection));
						slotSingleDict[injection.key] = unionSlot;
					}
					else
					{
						Debug.LogError($"invalid injection {whichClass}:{injection.key}");
					}
	            }
	            else
	            {
		            var list = new List<IUnionSlot>();
		            var childSlotField = new SlotInjected(this, injection);
					foreach (var path in injection.nodePathList)
					{
						var obj = injection.ToNodeObject(this, path);
						if (obj is IUnionSlot unionSlot)
						{
							unionSlot.Init(childSlotField);
							list.Add(unionSlot);
						}
						else
						{
							Debug.LogError($"invalid injection {whichClass}:{injection.key}");
						}
					}
					slotListDict[injection.key] = new UnionSlotList(childSlotField, list);
	            }
            }
        }
        
        protected override void Awake()
        {
	        // do nothing
        }

        protected override void CreateLuaTable(ref LuaTable luaSelf)
        {
	        // do nothing
        }

        public AbstractSlotJson PackToJson(IPutAsset putAsset)
        {
	        var behavJson = new SlotBehavJson();
	        foreach (var kv in slotSingleDict)
	        {
		        behavJson.singleDict[kv.Key] = kv.Value.PackToJson(putAsset);
	        }
	        foreach (var kv in slotListDict)
	        {
		        var arr = new AbstractSlotJson[kv.Value.Count];
		        behavJson.listDict[kv.Key] = arr;
		        for (int i = 0; i < arr.Length; i++)
		        {
			        arr[i] = kv.Value[i].PackToJson(putAsset);
		        }
	        }
			return behavJson;
        }

        public void UnpackFromJson(IGetAsset getAsset, AbstractSlotJson slotJson)
        {
	        var behavJson = (SlotBehavJson)slotJson;
	        foreach (var kv in slotSingleDict)
	        {
		        kv.Value.UnpackFromJson(getAsset, behavJson.singleDict[kv.Key]);
	        }
	        foreach (var kv in slotListDict)
	        {
		        kv.Value.RawUnpackList(getAsset, behavJson.listDict[kv.Key]);
	        }
        }

        [BlackList]
        public UnionSlotList GetSlotList(AbstractNodeInjection injection)
        {
            return slotListDict[injection.key];
        }
#if UNITY_EDITOR
	    [BlackList]
	    protected override void CachePutSlot(Component com)
	    {
		    // implement in SlotBehaviour
		    if (com is AbstractSlotCom slotCom)
		    {
			    cacheSlotIds.Add(slotCom.GetInstanceID());
		    }
	    }
#endif
    }
}