namespace Isitar.StorageWayCalculator.StorageAreaToGraphProcessor
{
    using System;
    using System.Collections.Generic;
    using MasterData.Storage;

    public class StorageAreaFieldWithDistances : StorageAreaField
    {
        public StorageAreaFieldWithDistances() { }

        public StorageAreaFieldWithDistances(StorageAreaField c)
        {
            StorageId = c.StorageId;
            Type = c.Type;
            DisplayName = c.DisplayName;
            if (Type == StorageAreaFieldType.Storage)
            {
                Distances[StorageId] = 0;
            }
        }

        public Dictionary<Guid, double> Distances { get; set; } = new();
    }
}