namespace Isitar.StorageWayCalculator.MasterData.Storage
{
    using System;

    public class StorageArea<T> where T : StorageAreaField
    {
        public StorageArea() { }

        public StorageArea(int dim1, int dim2)
        {
            Fields = new T[dim1, dim2];
            for (var x = 0; x < dim1; x++)
            {
                for (var y = 0; y < dim2; y++)
                {
                    Fields[x, y] = Activator.CreateInstance<T>();
                }
            }
        }

        public T[,] Fields { get; set; }
    }
}