using Nianxie.Framework;

namespace Nianxie.Components
{
    public class UpdateSubBehaviour : SubBehaviour<UpdateVtbl>
    {
        private void Update()
        {
            subTable.Update.Action(self);
        }
    }
}