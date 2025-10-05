using Nianxie.Framework;

namespace Nianxie.Components 
{
	public class FixedUpdateSubBehaviour : SubBehaviour<FixedUpdateVtbl> 
	{
		private void FixedUpdate()
		{
			subTable.FixedUpdate.Action(self);
		}
	}
}
