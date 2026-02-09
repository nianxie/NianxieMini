using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nianxie.Components;
using Nianxie.Utils;
using UnityEngine;
using XLua;

namespace Nianxie.Craft
{
    public class SlotBehaviour:LuaBehaviour, IUnionSlot
    {
        [NonSerialized] SlotSelectHead m_SlotSelectHead;
        public SlotSelectHead selectHead
        {
            get
            {
                if (!m_SlotSelectHead)
                {
                    m_SlotSelectHead = GetComponent<SlotSelectHead>();
                }
                return m_SlotSelectHead;
            }
        }
	    public class BehavList:IReadOnlyList<SlotBehaviour>
	    {
		    private List<SlotBehaviour> list;
		    private SlotInjected slotInjected;
		    public BehavList(SlotInjected slotInjected, List<SlotBehaviour> list)
		    {
			    this.list = list;
			    this.slotInjected = slotInjected;
		    }

		    public void DuplicateElement()
		    {
			    var template = list[0];
				var newBehav = Instantiate(template, template.transform.parent);
				newBehav.Init(slotInjected.IndexChildDynamicInjected());
				newBehav.transform.localPosition += Vector3.right*template.selectHead.selectBody.touchCollider2D.size.x*list.Count;
				list.Add(newBehav);
			}

			public void DeleteElement(SlotBehaviour behav)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] == behav)
					{
						list.RemoveAt(i);
						UnityEngine.Object.Destroy(behav.gameObject);
						return;
					}
				}
				Debug.LogError($"try to remove {behav} but it's not in this list");
			}

			public void UnpackFromJsonList(SlotBehavJson[] behavJsonArr)
			{
				for (int i = list.Count; i < behavJsonArr.Length; i++)
				{
					DuplicateElement();
				}
				for (int i = list.Count-1; i >= behavJsonArr.Length; i--)
				{
					DeleteElement(list[i]);
				}

				for (int i = 0; i < behavJsonArr.Length; i++)
				{
					list[i].TypedUnpackFromJson(behavJsonArr[i]);
				}
			}

			public IEnumerator<SlotBehaviour> GetEnumerator()
			{
				return list.GetEnumerator();
			}
			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
			public int Count => list.Count;
			public SlotBehaviour this[int index] => list[index];
	    }

	    public SlotInjected slotInjected { get; private set; }
	    public ISlotHandler slotHandler { get; private set; }

        private Dictionary<string, IUnionSlot> singleSlotDict = new();
        private Dictionary<string, BehavList> behavListDict = new();

        public void RootInit(CraftManager craftManager)
        {
	        slotHandler = craftManager;
	        (this as IUnionSlot).Init(new SlotInjected.RootInjected());
        }

        public void Init(SlotInjected injected)
        {
			slotInjected = injected;
	        if (injected is not SlotInjected.RootInjected)
	        {
				slotHandler = injected.ancestor.slotHandler;
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
						unionSlot.Init(injected.FieldChildInjected(this, injection));
						singleSlotDict[injection.key] = unionSlot;
					}
					else
					{
						Debug.LogError($"invalid injection {whichClass}:{injection.key}");
					}
	            }
	            else
	            {
		            var listInjected = injected.FieldChildInjected(this, injection);
		            var list = new List<SlotBehaviour>();
					for(int i=0;i<injection.nodePathList.Length;i++)
					{
						var obj = injection.ToNodeObject(this, injection.nodePathList[i]);
						if (obj is SlotBehaviour slotBehav)
						{
							slotBehav.Init(listInjected.IndexChildDefaultInjected(i));
							list.Add(slotBehav);
						}
						else
						{
							Debug.LogError($"only SlotBehaviour support list in injection {whichClass}:{injection.key}");
						}
					}
					behavListDict[injection.key] = new BehavList(listInjected, list);
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

        public SlotBehavJson TypedPackToJson()
        {
	        var behavJson = new SlotBehavJson()
	        {
		        classPath = classPath,
		        nestedKeys = nestedKeys,
	        };
	        foreach (var kv in singleSlotDict)
	        {
		        behavJson.singleDict[kv.Key] = kv.Value.PackToJson();
	        }
	        foreach (var kv in behavListDict)
	        {
		        var arr = new SlotBehavJson[kv.Value.Count];
		        behavJson.listDict[kv.Key] = arr;
		        for (int i = 0; i < arr.Length; i++)
		        {
			        arr[i] = kv.Value[i].TypedPackToJson();
		        }
	        }
			return behavJson;
        }

        AbstractSlotJson IUnionSlot.PackToJson()
        {
	        return TypedPackToJson();
        }

        public void TypedUnpackFromJson(SlotBehavJson behavJson)
        {
	        foreach (var kv in singleSlotDict)
	        {
		        kv.Value.UnpackFromJson(behavJson.singleDict[kv.Key]);
	        }
	        foreach (var kv in behavListDict)
	        {
		        kv.Value.UnpackFromJsonList(behavJson.listDict[kv.Key]);
	        }
        }

        void IUnionSlot.UnpackFromJson(AbstractSlotJson slotJson)
        {
	        TypedUnpackFromJson(slotJson as SlotBehavJson);
        }

        public bool IsListField()
        {
	        return slotInjected.IsList();
        }

        public void DuplicateSelf()
        {
	        if (slotInjected.IsList())
	        {
				slotInjected.ancestor.behavListDict[slotInjected.injection.key].DuplicateElement();
	        }
	        else
	        {
		        Debug.LogError($"try to delete single behav {this}");
	        }
        }
        public void DeleteSelf()
        {
	        if (slotInjected.IsList())
	        {
				slotInjected.ancestor.behavListDict[slotInjected.injection.key].DeleteElement(this);
	        }
	        else
	        {
		        Debug.LogError($"try to delete single behav {this}");
	        }
        }
        private BehavList GetBehavList(AbstractNodeInjection injection)
        {
            return behavListDict[injection.key];
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