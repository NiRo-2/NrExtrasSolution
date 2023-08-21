namespace NrExtras.GuidGenerator
{
    public static class GuidGenerator
    {
        /// <summary>
        /// get unique id
        /// </summary>
        /// <returns></returns>
        public static string generateGUID()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}