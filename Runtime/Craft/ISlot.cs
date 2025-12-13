namespace Nianxie.Craft
{
    public interface ISlot
    {
        public object ReadData();
        public AbstractSlotJson PackToJson(AbstractPackContext packContext);
        public void UnpackFromJson(CraftUnpackContext unpackContext, AbstractSlotJson slotJson);
    }
}