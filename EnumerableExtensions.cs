public static class Extensions
{
    public static IEnumerable<Position> Flattened(this Position[,] array)
    {
        for (int x = 0; x < array.GetLength(0); x++)
        {
            for (int y = 0; y < array.GetLength(1); y++)
            {
                yield return array[x,y];
            }
        }
    }
}
