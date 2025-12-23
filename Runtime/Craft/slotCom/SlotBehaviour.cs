using System;
using System.Collections;
using System.Collections.Generic;
using Nianxie.Components;
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
			public void DuplicateElement(SlotSelectHead slotSelect)
			{
				foreach (var child in list)
				{
					if (child.gameObject == slotSelect.gameObject)
					{
						var newSelect = Instantiate(slotSelect, slotSelect.transform.parent);
						var newSlot = newSelect.GetComponent(child.GetType()) as IUnionSlot;
						newSlot.Init(slotInjected);
						/*foreach (var dupNodeSlot in newObj.GetComponentsInChildren<AbstractNodeSlot>())
						{
							dupNodeSlot.PostDuplicate();
						}*/
						list.Add(newSlot);
						return;
					}
				}
			}

			public void DeleteElement(SlotSelectHead slotSelect)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].gameObject == slotSelect.gameObject)
					{
						list.RemoveAt(i);
						UnityEngine.Object.Destroy(slotSelect.gameObject);
						return;
					}
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
	    public SlotCallback slotCallback { get; private set; }

        private Dictionary<string, IUnionSlot> slotSingleDict = new();
        private Dictionary<string, UnionSlotList> slotListDict = new();

        public void RootInit(CraftEdit edit)
        {
	        slotCallback = edit;
	        (this as IUnionSlot).Init(null);
        }

        void IUnionSlot.PostDuplicate()
        {
	        foreach (var slot in slotSingleDict.Values)
	        {
		        slot.PostDuplicate();
	        }

	        foreach (var list in slotListDict.Values)
	        {
		        foreach (var slot in list)
		        {
			        slot.PostDuplicate();
		        }
	        }
        }

        void IUnionSlot.Init(SlotInjected injected)
        {
	        if (injected != null)
	        {
				slotInjected = injected;
				slotCallback = injected.behav.slotCallback;
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
        
        public AbstractSlotJson PackToJson(AbstractPackContext packContext)
        {
	        throw new NotImplementedException("TODO");
			var behavJson = new SlotBehavJson();
			var reflectEnv = gameManager.reflectEnv;
			var reflectCls = reflectEnv.GetWarmedReflect(classPath, nestedKeys);
			foreach (var injection in reflectCls.nodeInjections)
			{
				var injectObj = injection.ToNodeObject(this, injection.nodePath);
				if (injectObj is IUnionSlot slotCom)
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
	        throw new NotImplementedException("TODO");
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

        [BlackList]
        public UnionSlotList GetSlotList(AbstractNodeInjection injection)
        {
            return slotListDict[injection.key];
        }
    }
}