namespace Wirex.Engine.Api
{
    public class PartitionProvider
    {
        public static long GetPartition(string baseCurrency, string quoteCurrency)
        {
            return baseCurrency.GetHashCode() ^ quoteCurrency.GetHashCode();
        }
    }
}
