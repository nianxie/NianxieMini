using Nianxie.Framework;

namespace Nianxie.Components 
{
	public class LateUpdateSubBehaviour : SubBehaviour<LateUpdateVtbl> 
	{
		private void LateUpdate()
		{
			subTable.LateUpdate.Action(self);
		}
	}
}
