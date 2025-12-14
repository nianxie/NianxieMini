namespace Nianxie.Craft
{
	public class BlankSlot: AbstractNodeSlot
    {
	    public override AbstractSlotJson PackToJson(AbstractPackContext packContext)
	    {
		    return new BlankJson();
	    }

	    public override void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson)
	    {
	    }
    }
}