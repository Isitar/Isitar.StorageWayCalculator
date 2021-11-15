namespace Isitar.StorageWayCalculator.MasterData.Storage
{
    using System;

    public enum StorageAreaFieldType
    {
        Free = 0,
        Wall = 1,
        Storage = 2,
    }

    public class StorageAreaField
    {
        public StorageAreaField() {}
        
        public Guid StorageId { get; set; } = Guid.NewGuid();
        public StorageAreaFieldType Type { get; set; } = StorageAreaFieldType.Free;
        public string DisplayName { get; set; }

        public override string ToString()
        {
            return Type switch
            {
                StorageAreaFieldType.Free => " ",
                StorageAreaFieldType.Storage => "S",
                StorageAreaFieldType.Wall => "X",
                _ => "?"
            };
        }
    }
}