namespace NrExtras.RandomCollection
{
    public static class RandomCollection
    {
        /// <summary>
        /// pick some random items from list without repeats
        /// </summary>
        /// <typeparam name="T">returned list</typeparam>
        /// <param name="list">input list</param>
        /// <param name="elementsCount">elements count to pick</param>
        /// <returns></returns>
        public static List<T> GetRandomElements<T>(this IEnumerable<T> list, int elementsCount)
        {
            return list.OrderBy(arg => Guid.NewGuid()).Take(elementsCount).ToList();
        }
    }
}