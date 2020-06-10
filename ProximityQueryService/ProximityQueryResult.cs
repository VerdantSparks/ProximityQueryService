namespace LocationData
{
    public class ProximityQueryResult<T>
    {
        public T Item { get; }
        public double Distance { get; }

        public ProximityQueryResult(T item, double distance)
        {
            Item = item;
            Distance = distance;
        }
    }
}
