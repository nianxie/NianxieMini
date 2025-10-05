using Nianxie.Framework;
using UnityEngine;

namespace Nianxie.Components
{
    public class PhysicsSubBehaviour : SubBehaviour<PhysicsVtbl>
    {
        private void OnTriggerEnter(Collider other)
        {
            subTable.OnTriggerEnter?.Action(self, other);
        }

        private void OnTriggerStay(Collider other)
        {
            subTable.OnTriggerStay?.Action(self, other);
        }

        private void OnTriggerExit(Collider other)
        {
            subTable.OnTriggerExit?.Action(self, other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            subTable.OnCollisionEnter?.Action(self, collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            subTable.OnCollisionStay?.Action(self, collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            subTable.OnCollisionExit?.Action(self, collision);
        }
    }
}