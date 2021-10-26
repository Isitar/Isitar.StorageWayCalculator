namespace Isitar.StorageWayCalculator.MasterData.Graph
{
    public class Edge
    {
        public Node NodeFrom { get; set; }
        public Node NodeTo { get; set; }

        public double Distance { get; set; }
    }
}