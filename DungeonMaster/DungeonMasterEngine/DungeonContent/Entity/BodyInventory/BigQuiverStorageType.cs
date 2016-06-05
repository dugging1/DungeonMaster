using DungeonMasterEngine.DungeonContent.Entity.BodyInventory.@base;

namespace DungeonMasterEngine.DungeonContent.Entity.BodyInventory
{
    internal class BigQuiverStorageType : IStorageType
    {
        public int Size { get; } = 1;
        public static BigQuiverStorageType Instance { get; } = new BigQuiverStorageType();

        private BigQuiverStorageType() { }
    }
}