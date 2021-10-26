namespace Isitar.StorageWayCalculator.MasterData.Storage
{
    public class StorageArea
    {
        public StorageArea() { }

        public StorageArea(int dim1, int dim2)
        {
            Fields = new StorageAreaField[dim1, dim2];
            for (var x = 0; x < dim1; x++)
            {
                for (var y = 0; y < dim2; y++)
                {
                    Fields[x, y] = new StorageAreaField();
                }
            }
        }

        public StorageAreaField[,] Fields { get; set; }
    }
}