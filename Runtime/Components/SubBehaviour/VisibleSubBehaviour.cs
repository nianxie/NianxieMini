using Nianxie.Framework;

namespace Nianxie.Components
{
    public class VisibleSubBehaviour : SubBehaviour<VisibleVtbl>
    {
        private void OnBecameVisible()
        {
            subTable.OnBecameVisible?.Action(self);
        }

        private void OnBecameInvisible()
        {
            subTable.OnBecameInvisible?.Action(self);
        }
    }
}