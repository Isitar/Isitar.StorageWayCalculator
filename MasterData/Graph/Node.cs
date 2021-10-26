namespace Isitar.StorageWayCalculator.MasterData.Graph
{
    using System;
    using System.Collections.Generic;

    public class Node
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public ICollection<Edge> Edges { get; set; }
    }
}