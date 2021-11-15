namespace Isitar.StorageWayCalculator.StorageAreaToGraphProcessor
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MasterData.Storage;

    public class Processor
    {
        private readonly Dictionary<Guid, int> storagesFound = new Dictionary<Guid, int>();
        private int storageCnt;
        private readonly List<(int, int)> storages = new();

        public StorageArea<StorageAreaFieldWithDistances> CalculateDistances(StorageArea<StorageAreaField> storageArea)
        {
            var intermediateField = new StorageArea<StorageAreaFieldWithDistances>(0, 0)
            {
                Fields = new StorageAreaFieldWithDistances[storageArea.Fields.GetLength(0) + 2, storageArea.Fields.GetLength(1) + 2]
            };
            var queue = new ConcurrentQueue<(int, int, Guid)>();

            storageCnt = 0;

            for (var x = 0; x < storageArea.Fields.GetLength(0); ++x)
            {
                for (var y = 0; y < storageArea.Fields.GetLength(1); ++y)
                {
                    var convertedField = new StorageAreaFieldWithDistances(storageArea.Fields[x, y]);
                    intermediateField.Fields[x + 1, y + 1] = convertedField;
                    if (storageArea.Fields[x, y].Type == StorageAreaFieldType.Storage)
                    {
                        storageCnt++;
                        storagesFound[convertedField.StorageId] = 1;
                        storages.Add((x + 1, y + 1));
                    }
                }
            }

            for (var x = 0; x < intermediateField.Fields.GetLength(0); ++x)
            {
                intermediateField.Fields[x, 0] = new StorageAreaFieldWithDistances {Type = StorageAreaFieldType.Wall};
                intermediateField.Fields[x, intermediateField.Fields.GetLength(1) - 1] = new StorageAreaFieldWithDistances {Type = StorageAreaFieldType.Wall};
            }

            for (var y = 0; y < intermediateField.Fields.GetLength(1); ++y)
            {
                intermediateField.Fields[0, y] = new StorageAreaFieldWithDistances {Type = StorageAreaFieldType.Wall};
                intermediateField.Fields[intermediateField.Fields.GetLength(0) - 1, y] = new StorageAreaFieldWithDistances {Type = StorageAreaFieldType.Wall};
            }


            foreach (var (fieldX, fieldY) in storages)
            {
                var fieldId = intermediateField.Fields[fieldX, fieldY].StorageId;
                queue.Enqueue((fieldX, fieldY, fieldId));
                  
                while (queue.Any())
                {
                    if (queue.TryDequeue(out var res))
                    {
                        var (x, y, id) = res;
                        var currDistance = intermediateField.Fields[x, y].Distances[id];
                        var tasks = new[]
                        {
                            Task.Run(() => TestField(intermediateField, x, y + 1, id, queue, currDistance + 1)), // top
                            Task.Run(() => TestField(intermediateField, x, y - 1, id, queue, currDistance + 1)), // bottom
                            Task.Run(() => TestField(intermediateField, x + 1, y, id, queue, currDistance + 1)), // right
                            Task.Run(() => TestField(intermediateField, x - 1, y, id, queue, currDistance + 1)), // left

                            Task.Run(() => TestField(intermediateField, x + 1, y + 1, id, queue, currDistance + 1.4142135)), // top right
                            Task.Run(() => TestField(intermediateField, x + 1, y - 1, id, queue, currDistance + 1.4142135)), // bottom right
                            Task.Run(() => TestField(intermediateField, x - 1, y + 1, id, queue, currDistance + 1.4142135)), // top left
                            Task.Run(() => TestField(intermediateField, x - 1, y - 1, id, queue, currDistance + 1.4142135)), // bottom left
                        };


                        Task.WaitAll(tasks, 100);
                    }
                }

                foreach (var (otherStorageX, otherStorageY) in storages)
                {
                    if (otherStorageX == fieldX && otherStorageY == fieldY)
                    {
                        continue;
                    }
                    var otherFieldId = intermediateField.Fields[fieldX, fieldY].StorageId;

                    if (!intermediateField.Fields[otherStorageX, otherStorageY].Distances.TryGetValue(fieldId, out var distToOtherField))
                    {
                        continue;
                    }
                    
                    
                    for (var x = 0; x < intermediateField.Fields.GetLength(0); ++x)
                    {
                        for (var y = 0; y < intermediateField.Fields.GetLength(1); ++y)
                        {
                            // if current field visited this
                            if (intermediateField.Fields[x, y].Distances.TryGetValue(fieldId, out var distFromField))
                            {
                                var distViaOtherField = distFromField + distToOtherField;
                                
                                //if already visited by other field / calculated from other field
                                if (intermediateField.Fields[x, y].Distances.TryGetValue(otherFieldId, out var distFromOtherField))
                                {
                                    if (distViaOtherField < distFromOtherField)
                                    {
                                        intermediateField.Fields[x, y].Distances[otherFieldId] = distViaOtherField;
                                    }
                                }
                                else
                                {
                                    // other field never visited before
                                    intermediateField.Fields[x, y].Distances[otherFieldId] = distViaOtherField;
                                }
                            }
                        }    
                    }
                }

            }
          
            var retVal = new StorageArea<StorageAreaFieldWithDistances>(0, 0)
            {
                Fields = new StorageAreaFieldWithDistances[storageArea.Fields.GetLength(0), storageArea.Fields.GetLength(1)]
            };
            for (var x = 0; x < retVal.Fields.GetLength(0); ++x)
            {
                for (var y = 0; y < retVal.Fields.GetLength(1); ++y)
                {
                    retVal.Fields[x, y] = intermediateField.Fields[x + 1, y + 1];
                }
            }

            return retVal;
        }

        private void TestField(StorageArea<StorageAreaFieldWithDistances> storageArea, int x, int y, Guid id, ConcurrentQueue<(int, int, Guid)> queue, double distance)
        {
            var f = storageArea.Fields[x, y];
            if (f.Type == StorageAreaFieldType.Wall)
            {
                return;
            }

            if (f.Distances.TryGetValue(id, out var dist) && dist <= distance)
            {
                return;
            }

            f.Distances[id] = distance;
            if (f.Type == StorageAreaFieldType.Storage)
            {
                storagesFound[id]++;
            }
            if (storagesFound[id] >= storageCnt)
            {
                return;
            }

            queue.Enqueue((x, y, id));
        }
    }
}