using Nianxie.Framework;
using UnityEngine;

namespace Nianxie.Components
{
	public class Physics2DSubBehaviour : SubBehaviour<Physics2DVtbl>
	{
		private void OnTriggerEnter2D(Collider2D other)
		{
			subTable.OnTriggerEnter2D?.Action(self, other);
		}

		private void OnTriggerStay2D(Collider2D other)
		{
			subTable.OnTriggerStay2D?.Action(self, other);
		}

		private void OnTriggerExit2D(Collider2D other)
		{
			subTable.OnTriggerExit2D?.Action(self, other);
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			subTable.OnCollisionEnter2D?.Action(self, collision);
		}

		private void OnCollisionStay2D(Collision2D collision)
		{
			subTable.OnCollisionStay2D?.Action(self, collision);
		}

		private void OnCollisionExit2D(Collision2D collision)
		{
			subTable.OnCollisionExit2D?.Action(self, collision);
		}
	}
}
